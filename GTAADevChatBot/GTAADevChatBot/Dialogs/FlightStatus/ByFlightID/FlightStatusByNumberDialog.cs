using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Dialogs.ByCity;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.FlightStatus.ByFlightID
{
    public class FlightStatusByNumberDialog : CancelAndHelpDialog
    {
        // Dialog IDs
        private const string FlightStatusDialogId = "FlightStatusByFlightIDDialog";
        private const string FlightNumberPrompt = "FlightNumberPrompt";
        private const string DateTimePrompt = "DateTimePrompt";
        public IStatePropertyAccessor<FlightStatusState> flightStatus { get; }
        public static readonly string LuisConfiguration = GtaaChatConstant.LUIS_CONFIGURATION;
        private readonly ILogger _logger;
        private string _strFlightorCityText = "'flight/city'";
        private BotServices _botServices = null;
        public FlightStatusByNumberDialog(IStatePropertyAccessor<FlightStatusState> statePropertyAccessor, BotServices botServices, ILoggerFactory loggerFactory) : base(nameof(FlightStatusByNumberDialog))
        {
            flightStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            _botServices = botServices;
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Flight Status Dialog.");

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    AskFlightNumberStepAsync,
                    SummarystepAsync,
                    RouteToFlightByIdOrCity

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new TextPrompt("RetryTextPrompt", CustomPromptValidatorAsync));
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = (FlightStatusState)stepContext.Options;
            if (flightStatusState == null)
            {
                var flightStatusStateOpt = stepContext.Options as FlightStatusState;
                if (flightStatusStateOpt != null)
                {
                    await flightStatus.SetAsync(stepContext.Context, flightStatusStateOpt);
                }
                else
                {
                    await flightStatus.SetAsync(stepContext.Context, new FlightStatusState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskFlightNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchFlightState = (FlightStatusState)stepContext.Options;

            if (searchFlightState.FlightNumber != null)
                return await stepContext.NextAsync(searchFlightState.FlightNumber, cancellationToken);

            // prompt for Flight Number, if missing
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "Please enter your Flight Number?",
                },
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var flightStatusState = (FlightStatusState)stepContext.Options;

                if (flightStatusState.FlightNumber != null)
                {
                    flightStatusState.FlightNumber = stepContext.Result.ToString();
                }

                var response = await GetAsync(flightStatusState);
                List<Flight> flightsList = null;
                Flight flight = null;
                if (response != string.Empty)
                {
                    flightsList = JsonConvert.DeserializeObject<List<Flight>>(response);
                    flight = flightsList.Where(f => (Utility.GetESTCurrentTime(f.schTime).Date == flightStatusState.dateTime.Date && f.type == flightStatusState.AD)).FirstOrDefault();
                }

                if (flight != null)
                {
                    var reply = stepContext.Context.Activity.CreateReply();
                    if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                    {
                        // reply.Text = "Here you go";
                        reply.Attachments.Add(CreateSearchFlightAdaptiveCardAttachment(flight));

                    }
                    else
                    {
                        reply.Attachments.Add(CreateFacebookSearchFlightAdaptiveCardAttachment(flight));
                    }
                    await stepContext.Context.SendActivityAsync(reply);
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Can I help you with anything else?"),
                        RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                    });
                }
                else
                {
                    var searchFlightState = (FlightStatusState)stepContext.Options;
                    string flightNumber = Utility.ModifyCityFlightNoByRegex(searchFlightState.FlightNumber);
                    return await stepContext.PromptAsync("RetryTextPrompt", new PromptOptions()
                    {
                        Prompt = MessageFactory.Text($"I'm sorry, I can't seem to find {flightNumber}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent."),
                        //RetryPrompt = MessageFactory.Text($"I'm sorry, I can't seem to find that {flightNumber}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.")
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByNumberDialog", "SummarystepAsync");
                return await stepContext.EndDialogAsync();

            }
        }
        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            try
            {
                var searchFlightState = await flightStatus.GetAsync(promptContext.Context);
                if (promptContext.Recognized.Succeeded)
                {
                    _strFlightorCityText = promptContext.Context.Activity.Text;
                    if (_strFlightorCityText.StartsWith("#"))
                    {
                        promptContext.Context.Activity.Text = _strFlightorCityText = _strFlightorCityText.Replace("#", "MU");
                    }
                    RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(promptContext.Context, cancellationToken);
                    if (recognizerResult.Entities.Count > 0)
                    {
                        string number = recognizerResult.Entities["flight#"]?.FirstOrDefault()?.ToString();
                        string city = recognizerResult.Entities["city"]?.FirstOrDefault()?.ToString();
                        if (number != null || city != null)
                            return await Task.FromResult(true);
                        else
                        {
                            string toOrfrom = searchFlightState.AD == "ARR" ? "from" : "to";
                            string flightOrCity = Utility.ModifyCityFlightNoByRegex(_strFlightorCityText);
                            if (flightOrCity.Any(char.IsDigit))
                            {
                                var msg = $"I'm sorry, I can't seem to find {flightOrCity}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                                await promptContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                                return await Task.FromResult(false);
                            }
                            else
                            {
                                var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {flightOrCity}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                                await promptContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                                return await Task.FromResult(false);
                            }

                        }
                    }
                    else
                        return await Task.FromResult(false);
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByNumberDialog", "CustomPromptValidatorAsync");
                return await Task.FromResult(false);
            }
        }
        private async Task<DialogTurnResult> RouteToFlightByIdOrCity(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                if (stepContext.Result is bool)
                {
                    if ((bool)stepContext.Result)
                    {
                        await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                        return await stepContext.EndDialogAsync();
                    }
                    else
                    {
                        await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                        return await stepContext.EndDialogAsync(cancellationToken);
                    }
                }
                else
                {
                    var searchFlightState = (FlightStatusState)stepContext.Options;
                    string input = (string)stepContext.Result;
                    if (input.StartsWith("#"))
                    {
                        stepContext.Context.Activity.Text = searchFlightState.userInput = input.Replace("#", "MU");
                    }

                    RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(stepContext.Context, cancellationToken);

                    if (recognizerResult.Entities.Count > 0)
                    {
                        searchFlightState.FlightNumber = recognizerResult.Entities["flight#"]?.FirstOrDefault()?.ToString().Replace(" ", String.Empty);
                        searchFlightState.City = recognizerResult.Entities["city"]?.FirstOrDefault()?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                    }

                    if (searchFlightState.FlightNumber != null)
                        return await stepContext.ReplaceDialogAsync(nameof(FlightStatusByNumberDialog), searchFlightState, cancellationToken);
                    else if (searchFlightState.City != null)
                        return await stepContext.BeginDialogAsync(nameof(FlightStatusByCityDialog), searchFlightState, cancellationToken);
                    else
                    {
                        string toOrfrom = searchFlightState.AD == "ARR" ? "from" : "to";
                        var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {input}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                    }

                    return await stepContext.ReplaceDialogAsync(nameof(FlightStatusDialog), null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByNumberDialog", "RouteToFlightByIdOrCity");
                return await stepContext.EndDialogAsync();

            }
            
            
        }




        /// <param name="uri"></param>
        /// <returns></returns>
        //public async Task<string> GetAsync(FlightStatusState FlightStatusState)
        //{
        //    string result = string.Empty;
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(GtaaChatConstant.HABANERO_API_BASE_ADDRESS);
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/Json"));
        //            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/Json; charset=utf-8");
        //            //GET Method
        //            HttpResponseMessage response = await client.GetAsync($"api/flights/findById/{FlightStatusState.FlightNumber}");
        //            if (response.IsSuccessStatusCode)
        //            {
        //                result = await response.Content.ReadAsStringAsync();
        //            }
        //            else
        //            {
        //                Console.WriteLine("Internal server Error");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        //.LogError("Exception while fetching the flight status : " + ex.Message);
        //    }
        //    return result;
        //}

        private Attachment CreateSearchFlightAdaptiveCardAttachment(Flight rootObject)
        {
            string adaptiveCard=string.Empty;
            try
            {
                string[] paths = { ".", "Dialogs", "Resources", "flightStatus.json" };
                string fullPath = Path.Combine(paths);
                 adaptiveCard = File.ReadAllText(fullPath);
                adaptiveCard = adaptiveCard.Replace("<status>", Utility.ShowFlightStatus(rootObject.status));
                adaptiveCard = adaptiveCard.Replace("<flightnumber>", rootObject.id2);
                adaptiveCard = adaptiveCard.Replace("<schTime>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("HH:mm"));
                adaptiveCard = adaptiveCard.Replace("<latestTm>", Utility.GetESTCurrentTime(rootObject.latestTm).ToString("HH:mm"));

                if (rootObject.type == "ARR")
                {
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", rootObject.routes.FirstOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", "YYZ");
                    adaptiveCard = adaptiveCard.Replace("<departcity>", rootObject.routes.FirstOrDefault().cnty == "CAN" ? rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().region : rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", "Toronto, ON");
                    adaptiveCard = adaptiveCard.Replace("<gate>", string.Empty);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", string.Empty);

                }

                if (rootObject.type == "DEP")
                {
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", "YYZ");
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", rootObject.routes.LastOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<departcity>", "Toronto, ON");
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", rootObject.routes.LastOrDefault().cnty == "CAN" ? rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().region : rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", "Gate");


                }

                adaptiveCard = adaptiveCard.Replace("<Airline>", rootObject.al + " (" + rootObject.alCode + ")");
                adaptiveCard = adaptiveCard.Replace("<term>", rootObject.term);

                //
                adaptiveCard = adaptiveCard.Replace("<date>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("MMMM dd, HH:mm"));


                if (rootObject.ids != null && rootObject.ids.Count() > 0)
                {
                    string codeshares = "This flight is also known as: \r\r";
                    rootObject.ids.ToList().ForEach(s => codeshares += string.Format(" {0} : {1} \r\r", s.id2, s.alName));
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", codeshares);
                    //adaptiveCard = adaptiveCard.Replace("<codesharecount>", rootObject.ids.ToList().Count().ToString());
                }

                else
                {
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", string.Empty);
                    //adaptiveCard = adaptiveCard.Replace("<codesharecount>", "0");
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByNumberDialog", "CreateSearchFlightAdaptiveCardAttachment");
                
            }

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        private  Attachment CreateFacebookSearchFlightAdaptiveCardAttachment(Flight rootObject)
        {
            string adaptiveCard = string.Empty;
            try
            {
                string[] paths = { ".", "Dialogs", "Resources", "Facebook_FlightStatus_Card.json" };
                string fullPath = Path.Combine(paths);
                adaptiveCard = File.ReadAllText(fullPath);
                adaptiveCard = adaptiveCard.Replace("<status>", Utility.ShowFlightStatus(rootObject.status));
                adaptiveCard = adaptiveCard.Replace("<flightnumber>", rootObject.id2);
                adaptiveCard = adaptiveCard.Replace("<schTime>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("HH:mm"));
                adaptiveCard = adaptiveCard.Replace("<latestTm>", Utility.GetESTCurrentTime(rootObject.latestTm).ToString("HH:mm"));

                if (rootObject.type == "ARR")
                {
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", rootObject.routes.FirstOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", "YYZ");
                    adaptiveCard = adaptiveCard.Replace("<departcity>", rootObject.routes.FirstOrDefault().cnty == "CAN" ? rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().region : rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", "Toronto, ON");
                    adaptiveCard = adaptiveCard.Replace("<gate>", string.Empty);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", string.Empty);

                }

                if (rootObject.type == "DEP")
                {
                    adaptiveCard = adaptiveCard.Replace("<departcitycode>", "YYZ");
                    adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", rootObject.routes.LastOrDefault().code);
                    adaptiveCard = adaptiveCard.Replace("<departcity>", "Toronto, ON");
                    adaptiveCard = adaptiveCard.Replace("<arrivecity>", rootObject.routes.LastOrDefault().cnty == "CAN" ? rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().region : rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().cnty);
                    adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
                    adaptiveCard = adaptiveCard.Replace("<GateLiteral>", "Gate");


                }

                adaptiveCard = adaptiveCard.Replace("<Airline>", rootObject.al + " (" + rootObject.alCode + ")");
                adaptiveCard = adaptiveCard.Replace("<term>", rootObject.term);

                //
                adaptiveCard = adaptiveCard.Replace("<date>", Utility.GetESTCurrentTime(rootObject.schTime).ToString("MMMM dd, HH:mm"));


                if (rootObject.ids != null && rootObject.ids.Count() > 0)
                {
                    string codeshares = "This flight is also known as: \r\r";
                    rootObject.ids.ToList().ForEach(s => codeshares += string.Format(" {0} : {1} \r\r", s.id2, s.alName));
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", codeshares);
                    //adaptiveCard = adaptiveCard.Replace("<codesharecount>", rootObject.ids.ToList().Count().ToString());
                }

                else
                {
                    adaptiveCard = adaptiveCard.Replace("<codeshare>", string.Empty);
                    //adaptiveCard = adaptiveCard.Replace("<codesharecount>", "0");
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByNumberDialog", "CreateFacebookSearchFlightAdaptiveCardAttachment");
            }

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        public async Task<string> GetAsync(FlightStatusState FlightStatusState)
        {
            string result = string.Empty;
            try
            {               
                RestClient client = new RestClient(new Uri(GtaaChatConstant.HABANERO_API_BASE_ADDRESS));
                RestRequest request = new RestRequest($"api/flights/findById/{FlightStatusState.FlightNumber}", Method.GET);
                request.AddHeader("Content-Type", "application/Json; charset=utf-8");
                request.AddHeader("Accept", "application/Json");
                IRestResponse response = await client.ExecuteTaskAsync(request);
                if (response.IsSuccessful)
                {
                    result = response.Content;
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByNumberDialog", "GetAsync");
            }
            return result;
        }

    }
}
