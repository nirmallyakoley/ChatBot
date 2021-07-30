using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Dialogs.AirlineInformation;
using GTAADevChatBot.Dialogs.FlightStatus;
using GTAADevChatBot.Dialogs.FlightStatus.ByCity;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
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

namespace GTAADevChatBot.Dialogs.ByCity
{
    public class FlightStatusByCityDialog : CancelAndHelpDialog
    {
        private BotServices _botServices;
        public FlightStatusByCityDialog(BotServices botServices) : base(nameof(FlightStatusByCityDialog))
        {
            _botServices = botServices;
            var waterfallSteps = new WaterfallStep[]
            {
                    AskforCityStepAsync,
                    AskforAirlinesStepAsync,
                    RouteTodayandTomorrowstepAsync,
                    AskConfirmStepAsync,
                    ConfirmStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new FlightForTodayDialog());
            AddDialog(new FlightForTomorrowDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });

            //The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskforCityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchFlightState = (FlightStatusState)stepContext.Options;

            if (searchFlightState.City != null)
                return await stepContext.NextAsync(searchFlightState.City, cancellationToken);

            // prompt for City, if missing
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "Please enter your city?",
                },
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> AskforAirlinesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var flightStatusState = (FlightStatusState)stepContext.Options;
                flightStatusState.City = stepContext.Result.ToString();
                var response = await GetAsync(flightStatusState);
                FlightStatusSearchAPIResponseJson flights = null;
                if (response != string.Empty)
                {
                    flights = JsonConvert.DeserializeObject<FlightStatusSearchAPIResponseJson>(response);
                }
                if (flights != null)
                {

                    if (flights.list != null)
                    {
                        if (flights.list.Count() > 0)
                        {
                            if (flightStatusState.AD == "ARR")
                            {
                                flightStatusState.FlightsList = flights.list.Where(a => a.routes.Any(route => route.city.ToLower().Contains(flightStatusState.City.ToLower()))).ToList();
                                if (flightStatusState.FlightsList.Count() <= 0)
                                {
                                    string toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                                    string City = Utility.ModifyCityFlightNoByRegex(flightStatusState.City);
                                    var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {City}. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                                    {
                                        Prompt = MessageFactory.Text("Can I help you with anything else? "),
                                        RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                                    },
                                    cancellationToken: cancellationToken);
                                }
                            }
                            else
                            {
                                flightStatusState.FlightsList = flights.list.Where(a => a.routes.Any(route => route.city.ToLower().Contains(flightStatusState.City.ToLower()))).ToList();
                                if (flightStatusState.FlightsList.Count() <= 0)
                                {
                                    string toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                                    string City = Utility.ModifyCityFlightNoByRegex(flightStatusState.City);
                                    var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {City}. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                                    {
                                        Prompt = MessageFactory.Text("Can I help you with anything else?"),
                                        RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                                    },
                                    cancellationToken: cancellationToken);
                                }
                            }
                        }
                    }
                    else
                    {
                        string toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                        var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {flightStatusState.City}.  \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                        return await stepContext.EndDialogAsync();
                    }
                }
                else
                {
                    string toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                    string City = Utility.ModifyCityFlightNoByRegex(flightStatusState.City);
                    var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {City}. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                    return await stepContext.EndDialogAsync();
                }

                var optChoices = new List<Choice>();
                var result = (from item in flightStatusState.FlightsList
                              select new
                              {
                                  FlightName = item.al
                              }).ToList().Distinct();

                //-------------------- Code Share ------------------------------------------------//			  
                /*
                var result1 = (from item
                               in flightStatusState.FlightsList
                               from item1 in item.ids
                               select new
                               {
                                   FlightName = item1.alName
                               }).ToList().Distinct();

                  */
                //------------------------------------------------------------------------------------//           

                List<string> displayflights = new List<string>();
                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        displayflights.Add(item.FlightName);
                    }
                    //-------------------- Code Share ------------------------------------------------//			  
                    /*
                      foreach (var item in result1)
                          {
                              displayflights.Add(item.FlightName);
                           }
                   */
                    //------------------------------------------------------------------------------------// 

                    foreach (var item in displayflights.Distinct())
                    {
                        AirlineInformationBO airlineCode = AirlineInfo.GetAirlineInfo(item.Replace(" ", String.Empty).ToLower());
                        string code1 = string.Empty;
                        string code2 = string.Empty;
                        if (airlineCode != null)
                        {
                            code1 = airlineCode.Code1;
                            code2 = airlineCode.Code2;
                        }
                        optChoices.Add(new Choice()
                        {
                            Value = item,
                            Synonyms = new List<string>() { code1, code2 },
                        });
                    }

                    var toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                    return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
                    {
                        Prompt = MessageFactory.Text($"Here are the airlines with flights {Utility.GetFlightType(flightStatusState.AD)} {toOrfrom} {flightStatusState.City}"),
                        RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                        Choices = optChoices
                    });
                }
                else
                {

                    string toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                    string City = Utility.ModifyCityFlightNoByRegex(flightStatusState.City);
                    var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {City}. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Can I help you with anything else?"),
                        RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                    });

                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByCityDialog", "AskforAirlinesStepAsync");
                return await stepContext.EndDialogAsync();
             }
           
        }

        private async Task<DialogTurnResult> RouteTodayandTomorrowstepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if prompt called previously-----------------------------------------------//
            if (stepContext.Result is bool)
            {
                if ((bool)stepContext.Result)
                {
                    await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken);
                }
                if (!(bool)stepContext.Result)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks for chatting with me! If you have any other questions, just type **RESTART**"));
                    return await stepContext.EndDialogAsync(cancellationToken);
                }
            }
            //----------------------------------------------------------------------------------//

            var flightStatusState = (FlightStatusState)stepContext.Options;
            FoundChoice result = (FoundChoice)stepContext.Result;
            flightStatusState.al = result.Value;

            List<FlightByCity> flights = null;
            //if flights more than 3 go to today/tomorrow logic//
            if ((from item in flightStatusState.FlightsList where item.al.ToLower() == flightStatusState.al.ToLower() || (from item1 in item.ids where item1.alName.ToLower() == flightStatusState.al.ToLower() select item1).ToList().Count > 0 select item).ToList().Count() > 3)
            {
                if (flightStatusState.TT.ToLower() == "today")
                {
                    return await stepContext.BeginDialogAsync(nameof(FlightForTodayDialog), flightStatusState, cancellationToken: cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync(nameof(FlightForTomorrowDialog), flightStatusState, cancellationToken: cancellationToken);
                }
            }
            //if flights  <=3 directly show //
            else
            {
                //flights = flightStatusState.FlightsList.Where(a => a.al.ToLower() == flightStatusState.al.ToLower()).ToList();

                flights = (from item in flightStatusState.FlightsList where item.al.ToLower() == flightStatusState.al.ToLower() || (from item1 in item.ids where item1.alName.ToLower() == flightStatusState.al.ToLower() select item1).ToList().Count > 0 select item).ToList();

                if (flights.Count() > 0)
                {
                    var reply = stepContext.Context.Activity.CreateReply();
                    var resultString = flights.Count() > 1 ? "results" : "result";
                    if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                    {
                        reply.Text = $"Here you go : {flights.Count()} { resultString} found";

                        foreach (var item in flights.Take(25))
                        {
                            reply.Attachments.Add(FlightCardUtility.CreateSearchFlightAdaptiveCardAttachment(item, flightStatusState));
                        }

                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        await stepContext.Context.SendActivityAsync(reply);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Here you go : {flights.Count()} { resultString} found.");
                        foreach (var item in flights)
                        {
                            reply.Attachments.Add(FlightCardUtility.CreateFacebookSearchFlightAdaptiveCardAttachment(item,flightStatusState));
                            await stepContext.Context.SendActivityAsync(reply);
                            reply.Attachments.Clear();
                        }
                    }
                }
                else
                {
                    var toOrfrom = flightStatusState.AD == "ARR" ? "from" : "to";
                    string City = Utility.ModifyCityFlightNoByRegex(flightStatusState.City);
                    var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {City}. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                }
                return await stepContext.NextAsync(null);
            }
        }

        private async Task<DialogTurnResult> AskConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
            },
            cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);

                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

        public async Task<string> GetAsync(FlightStatusState FlightStatusState)
        {
            string result = string.Empty;
            try
            {
                RestClient client = new RestClient(new Uri(GtaaChatConstant.HABANERO_API_BASE_ADDRESS));
                RestRequest request = new RestRequest($"api/flights/list?type={FlightStatusState.AD}&day={FlightStatusState.TT}", Method.GET);
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
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByCityDialog", "GetAsync");

            }
            return result;
        }

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
        //            HttpResponseMessage response = await client.GetAsync($"api/flights/list?type={FlightStatusState.AD}&day={FlightStatusState.TT}");
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
        //        Dictionary<string, string> objDictionary = new Dictionary<string, string>();
        //        objDictionary.Add("Message", ex.Message);
        //        objDictionary.Add("InternalException", ex.InnerException.ToString());
        //        objDictionary.Add("StackTrace", ex.StackTrace);
        //        TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusByCityDialog", "GetAsync");

        //    }
        //    return result;
        //}

    }
}
