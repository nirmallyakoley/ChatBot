using GTAADevChatBot.Dialogs.FlightStatus.ByAirline;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.FlightStatus
{
    public class FlightStatusByAirlineDialog : CancelAndHelpDialog
    {
        // Dialog IDs
        private const string FlightStatusDialogId = "FlightStatusByFlightIDDialog";
        private const string FlightNumberPrompt = "FlightNumberPrompt";
        private const string DateTimePrompt = "DateTimePrompt";
        private const string ConfirmPrompt = "ConfirmPrompt";

        private readonly ILogger _logger;
        
        public IStatePropertyAccessor<FlightStatusState> flightStatus { get; }

        public FlightStatusByAirlineDialog(IStatePropertyAccessor<FlightStatusState> statePropertyAccessor, ILoggerFactory loggerFactory) : base(nameof(FlightStatusByAirlineDialog))
        {
            flightStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));

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
                    SummarystepAsync
            };
            AddDialog(new WaterfallDialog(nameof(FlightStatusByCityDialog), waterfallSteps));
            AddDialog(new TextPrompt(FlightNumberPrompt));
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
            var searchFlightState = await flightStatus.GetAsync(stepContext.Context);
            
            // prompt for name, if missing
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "Please enter your Airline name?",
                },
            };
            return await stepContext.PromptAsync(FlightNumberPrompt, opts);
        }
        
        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = await flightStatus.GetAsync(stepContext.Context);
            flightStatusState.Airline = stepContext.Result.ToString();
            
            var response = await GetAsync(flightStatusState);
            FlightByAirlineResponse flights = null;
            if (response != string.Empty)
            {
                flights = JsonConvert.DeserializeObject<FlightByAirlineResponse>(response);
                
            }

            if (flights != null)
            {
                
                if (flights.list != null && flights.list.Count() > 0)
                {
                    var reply = stepContext.Context.Activity.CreateReply();
                    
                    List<List> lists = flights.list.Where(a => a.al.ToLower() == flightStatusState.Airline.ToLower()).ToList();
                    reply.Text = $"Here you go : {lists.Count()} results found";

                    foreach (var item in lists.Take(25))
                    {
                        reply.Attachments.Add(CreateSearchFlightAdaptiveCardAttachment(item));
                    }

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    await stepContext.Context.SendActivityAsync(reply);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("No Flights are available.");
                }

                
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Sorry!.. No Flight details is available.");
            }
            HeroCard heroCard = new HeroCard()
            {
                Buttons = new List<CardAction>() {
                    new CardAction() { DisplayText = "I’m done", Value = "Imdone", Text = "I’m done", Title = "I’m done ", Type = "postBack" },
                    new CardAction() { DisplayText = "Have another question", Value = "Have another question", Text = "Have another question", Title = "Have another question", Type = "postBack" },
                }
            };


            var reply2 = stepContext.Context.Activity.CreateReply();
            reply2.Text = "Can I help you in something else?";
            reply2.Attachments.Add(heroCard.ToAttachment());
            await stepContext.Context.SendActivityAsync(reply2);




            return await stepContext.EndDialogAsync();
        }
        
        /// <summary>
        /// Send OTP GET https://api.lufthansa.com/v1/operations/schedules/{origin}/{destination}/{fromDateTime}[?directFlights=true]
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> GetAsync(FlightStatusState FlightStatusState)
        {
            string result = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://gtaa-fl-uat.azureedge.net/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/Json"));
                    //client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/Json; charset=utf-8");

                    
                    //GET Method
                    HttpResponseMessage response = await client.GetAsync($"api/flights/list?type={FlightStatusState.AD}&day={FlightStatusState.TT}");
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine("Internal server Error");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching the flight status : " + ex.Message);
            }
            return result;
        }

        private Attachment CreateSearchFlightAdaptiveCardAttachment(List rootObject)
        {
            string[] paths = { ".", "Dialogs", "FlightStatus", "Resources", "flightStatus.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("<status>", rootObject.status);
            adaptiveCard = adaptiveCard.Replace("<flightnumber>", rootObject.id2);
            adaptiveCard = adaptiveCard.Replace("<schTime>", rootObject.schTime.Split('T')[1].Split('-')[0]);
            adaptiveCard = adaptiveCard.Replace("<latestTm>", rootObject.latestTm.Split('T')[1].Split('-')[0]);

            if (rootObject.type == "ARR")
            {
                adaptiveCard = adaptiveCard.Replace("<departcitycode>", rootObject.routes.FirstOrDefault().code);
                adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", "YYZ");
                adaptiveCard = adaptiveCard.Replace("<departcity>", rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().cnty);
                adaptiveCard = adaptiveCard.Replace("<arrivecity>", "Toronto, ON, CA");

            }

            if (rootObject.type == "DEP")
            {
                adaptiveCard = adaptiveCard.Replace("<departcitycode>", "YYZ");
                adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", rootObject.routes.LastOrDefault().code);
                adaptiveCard = adaptiveCard.Replace("<departcity>", "Toronto, ON, CA");
                adaptiveCard = adaptiveCard.Replace("<arrivecity>", rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().cnty);

            }

            adaptiveCard = adaptiveCard.Replace("<Airline>", rootObject.al + " ("+ rootObject.alCode + ")");
            adaptiveCard = adaptiveCard.Replace("<term>", rootObject.term);
            adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
            //
            adaptiveCard = adaptiveCard.Replace("<type>", rootObject.type);
            //<Airline>
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        private Attachment CreateSearchFlightAdaptiveCardAttachment(ByCity.Departure rootObject)
        {
            string[] paths = { ".", "Dialogs", "FlightStatus", "Resources", "flightStatus.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("<status>", rootObject.status);
            adaptiveCard = adaptiveCard.Replace("<flightnumber>", rootObject.id2);
            adaptiveCard = adaptiveCard.Replace("<schTime>", rootObject.schTime.Split('T')[1].Split('-')[0]);
            adaptiveCard = adaptiveCard.Replace("<latestTm>", rootObject.latestTm.Split('T')[1].Split('-')[0]);

            if (rootObject.type == "ARR")
            {
                adaptiveCard = adaptiveCard.Replace("<departcitycode>", rootObject.routes.FirstOrDefault().code);
                adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", "YYZ");
                adaptiveCard = adaptiveCard.Replace("<departcity>", rootObject.routes.FirstOrDefault().city + ", " + rootObject.routes.FirstOrDefault().cnty);
                adaptiveCard = adaptiveCard.Replace("<arrivecity>", "Toronto, ON, CA");

            }

            if (rootObject.type == "DEP")
            {
                adaptiveCard = adaptiveCard.Replace("<departcitycode>", "YYZ");
                adaptiveCard = adaptiveCard.Replace("<arrivecitycode>", rootObject.routes.LastOrDefault().code);
                adaptiveCard = adaptiveCard.Replace("<departcity>", "Toronto, ON, CA");
                adaptiveCard = adaptiveCard.Replace("<arrivecity>", rootObject.routes.LastOrDefault().city + ", " + rootObject.routes.LastOrDefault().cnty);

            }

            adaptiveCard = adaptiveCard.Replace("<Airline>", rootObject.al + " (" + rootObject.alCode + ")");
            adaptiveCard = adaptiveCard.Replace("<term>", rootObject.term);
            adaptiveCard = adaptiveCard.Replace("<gate>", rootObject.gate);
            //
            adaptiveCard = adaptiveCard.Replace("<type>", rootObject.type);
            //<Airline>
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        
    }
}
