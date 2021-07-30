using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.Dialogs.AirportInformation.CarRentals;
using GTAADevChatBot.Dialogs.AirportInformation.ConnectionsGuide;
using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using GTAADevChatBot.Dialogs.AirportInformation.Parking;
using GTAADevChatBot.Dialogs.AirportInformation.ParkingAndTransportation;
using GTAADevChatBot.Dialogs.AirportInformation.PublicTransit;
using GTAADevChatBot.Dialogs.AirportInformation.RetailAndDutyFree;
using GTAADevChatBot.Dialogs.AirportInformation.SecurityScreening;
using GTAADevChatBot.Dialogs.AirportInformation.ServiceAmenities;
using GTAADevChatBot.Dialogs.AirportInformation.TaxiAndLimosOrRideShare;
using GTAADevChatBot.Dialogs.AirportInformation.UPExpress;
using GTAADevChatBot.Helper;
using GTAADevChatBot.Social_Miner;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation
{
    public class AirportInformationDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public IStatePropertyAccessor<CSMState> CSMStatus { get; }
        #region contructor
        private BotServices _services = null;
        public AirportInformationDialog(BotServices botServices, IStatePropertyAccessor<CSMState> statePropertyAccessor, ILoggerFactory loggerFactory) : base(nameof(AirportInformationDialog))
        {
            CSMStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            _services = botServices;
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Airport Information Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                ProvideAvailableOptionsStepAsync,
                RouteToSpecificDialogAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), CustomPromptValidatorAsync) { Style = ListStyle.SuggestedAction });
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion
        /// <summary>
        /// Initialize state
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AirportInformationDialog.InitializeStateStepAsync start");
            var objCSMState = (CSMState)stepContext.Options;
            if (objCSMState == null)
            {
                var CSMStatusStateOpt = stepContext.Options as CSMState;
                if (CSMStatusStateOpt != null)
                {
                    await CSMStatus.SetAsync(stepContext.Context, CSMStatusStateOpt);
                }
                else
                {
                    await CSMStatus.SetAsync(stepContext.Context, new CSMState());
                }
            }
            _logger.LogTrace("AirportInformationDialog.InitializeStateStepAsync end");
            return await stepContext.NextAsync();
        }
        /// <summary>
        /// Provide available Airport information options.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideAvailableOptionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AirportInformationDialog.ProvideAvailableOptionsStepAsync starts");

            _logger.LogTrace("AirportInformationDialog.ProvideAvailableOptionsStepAsync ends");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Let me know what you are looking for."),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Parking & transportation", Synonyms = new List<string>(){ "parking","park","vehicle parking","transportation","parking and transportation","parking facility","transportation facility", "parking and transportation options","transport","transport options", "car rental", "rental car", "taxi and limos", "taxi", "limos", "taxi & limos", "ride share", "ride", "share ride", "taxi and limos/Ride share", "limousine", "limousine", "rideshare", "shareride", "limo", "Up Express", "Train to downtown", "up express", "Train to city", "express", "upexpress", "UP","car rental","car rentals", "parking", "park", "vehicle parking","rental car", "public transit", "publictransit", "ttc", "subway", "go transit", "miway", "brampton transit", "public transport","airport parking & transportation options","parking & transporta..." } },
                        new Choice(){Value = "Eat and drink", Synonyms = new List<string>(){ "eat","food", "beverage","foods","beverages","food and beverage", "food and beverages","starbucks","tim hortons","subway","bar","cafe","juice","juices","meal","meals","alcohol","drink","drinks","veg","non veg","thirsty","hungry","food options","food choices" } },
                        new Choice(){Value = "Shopping", Synonyms = new List<string>(){ "retail", "duty free","shopping", "Convenience", "Fashion","Health and Beauty", "Electronics","News and books","Travel needs","Gifts and Specialty" } },
                        new Choice(){Value = "Services", Synonyms = new List<string>(){ "Service Amenities", "Lounges", "Banks and Currency","Health and Wellness","VIP services","pets"} },
                        new Choice()
                        {
                            Value = "Security screening",
                            Synonyms = new List<string>()
                            {
                              "Security", "Screening",  "Screening Information", "Screening Info", "Security Information", "Security Info", "security screening",
                              "CATSA","passenger screening","Pre-board screening","metal detector","Screening checkpoint","security checkpoint","security inspection",
                              "X-Ray","screen","prohibited items","restricted items","carry on items","carry on belongings","personal items","personal belongings","belongings"
                            }
                        },

                        new Choice()
                        {
                            Value = "Connections Guide",
                            Synonyms = new List<string>()
                            {
                              "Airport Guide", "Connecting Flights", "Connection guide","Guide for Connecting flights","Need Details for Conneting flights",
                              "Connecting flights details","Connecting","Transfer","connection process","Connections","Connections guide","Connecting guide"
                            }
                        }
             }
                 }, cancellationToken);
        }

        /// <summary>
        /// Route to specific information
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> RouteToSpecificDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AirportInformationDialog.RouteToSpecificDialogAsync starts");
            var objCSMState = await CSMStatus.GetAsync(stepContext.Context);
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"AirportInformationDialog.RouteToSpecificDialogAsync Choice:{foundChoice.Value}");

            if (foundChoice.Value.ToLower() == "parking & transportation")
            {
                objCSMState.CSQ = "Transportation";
                objCSMState.ID = "79";
                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                var synonym = foundChoice.Synonym.ToLower();
                if (synonym == "taxi" || synonym == "limo" || synonym == "limousine" || synonym == "uber" || synonym == "lyft" || synonym == "rideshare")
                {
                    return await stepContext.BeginDialogAsync(nameof(TaxiLimosRideShareDialog));
                }
                else if (synonym == "train to city" || synonym == "up express" || synonym == "train to downtown" || synonym == "upexpress" || synonym == "up")
                {
                    return await stepContext.BeginDialogAsync(nameof(UPExpressDialog));
                }
                else if (synonym == "public transit" || synonym == "ttc" || synonym == "subway" || synonym == "go transit" || synonym == "miway" || synonym == "brampton transit" || synonym == "Public Transport")
                {
                    return await stepContext.BeginDialogAsync(nameof(PublicTransitDialog));
                }
                else if (synonym == "parking" || synonym == "vehicle parking" || synonym == "parking facility" || synonym == "park" || synonym == "parking options")
                {
                    return await stepContext.BeginDialogAsync(nameof(ParkingDialog));
                }
                else if (synonym == "car rentals" || synonym == "car rental" || synonym == "rental car")
                {
                    return await stepContext.BeginDialogAsync(nameof(CarRentalsDialog));
                }
                else
                {
                    _logger.LogTrace("AirportInformationDialog.RouteToSpecificDialogAsync end");
                    return await stepContext.BeginDialogAsync(nameof(ParkingAndTransaportationDialog));
                }

            }

            else if (foundChoice.Value.ToLower() == "eat and drink")
            {
                _logger.LogTrace("AirportInformationDialog.RouteToSpecificDialogAsync end");
                objCSMState.CSQ = "Food & Beverage";
                objCSMState.ID = "77";
                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                return await stepContext.BeginDialogAsync(nameof(FoodAndBeverageDialog));
            }
            else if (foundChoice.Value.ToLower() == "shopping")
            {
                _logger.LogTrace("AirportInformationDialog.RouteToSpecificDialogAsync end");
                objCSMState.CSQ = "Retail";
                objCSMState.ID = "78";
                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                return await stepContext.BeginDialogAsync(nameof(RetailAndDutyFreeDialog));
            }
            else if (foundChoice.Value.ToLower() == "services")
            {
                objCSMState.CSQ = "Service Amenities";
                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                _logger.LogTrace("AirportInformationDialog.RouteToSpecificDialogAsync end");
                return await stepContext.BeginDialogAsync(nameof(ServiceAmenitiesDialog));
            }

            else if (foundChoice.Value.ToLower() == "security screening")
            {
                _logger.LogTrace("AirportInformationDialog.Security Screening Information end");
                objCSMState.CSQ = "Security";
                objCSMState.ID = "80";
                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                return await stepContext.BeginDialogAsync(nameof(SecurityScreeningDialog));
            }


            else if (foundChoice.Value.ToLower() == "connections guide")
            {
                _logger.LogTrace("AirportInformationDialog.Connections Guide end");
                objCSMState.CSQ = "Connection Guide";
                objCSMState.ID = "81";
                TelemetryLog.LogCustomTrace(_services, objCSMState, "Regular Chat");
                return await stepContext.BeginDialogAsync(nameof(ConnectionsGuideDialog));
            }


            else
            {
                var msg = $"I'm sorry, I can't seem to find that. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                _logger.LogTrace("AirportInformationDialog.RouteToSpecificDialogAsync end");
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync();
            }

        }

        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }




    }
}
