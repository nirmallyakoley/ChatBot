using GTAADevChatBot.Dialogs.AirportInformation.CarRentals;
using GTAADevChatBot.Dialogs.AirportInformation.Parking;
using GTAADevChatBot.Dialogs.AirportInformation.PublicTransit;
using GTAADevChatBot.Dialogs.AirportInformation.TaxiAndLimosOrRideShare;
using GTAADevChatBot.Dialogs.AirportInformation.UPExpress;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.ParkingAndTransportation
{
    public class ParkingAndTransaportationDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region constructor
        public ParkingAndTransaportationDialog(ILoggerFactory loggerFactory) : base(nameof(ParkingAndTransaportationDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Airport Information Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                ProvideAvailableOptionsStepAsync,
                RouteToSpecificDialogAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new CarRentalsDialog(loggerFactory));
            AddDialog(new TaxiLimosRideShareDialog(loggerFactory));
            AddDialog(new UPExpressDialog(loggerFactory));
            AddDialog(new PublicTransitDialog(loggerFactory));

            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Provide available Airport information options.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// 
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideAvailableOptionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("ParkingAndTransaportationDialog.ProvideAvailableOptionsStepAsync starts");

            _logger.LogTrace("ParkingAndTransaportationDialog.ProvideAvailableOptionsStepAsync ends");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Getting to and from Pearson couldn't be easier. We've got options for every traveller! What are you looking for?"),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Parking", Synonyms = new List<string>(){ "parking","park","vehicle parking" } },
                        new Choice(){Value = "Car Rentals", Synonyms = new List<string>(){ "car rental", "rental car" }  },
                        new Choice(){Value = "Taxi/Limos/Rideshare", Synonyms = new List<string>(){ "taxi and limos", "taxi", "limos","taxi & limos","ride share","ride","share ride", "taxi and limos/Ride share","limousine", "limousine","rideshare","shareride","limo" }  },
                        new Choice(){Value = "UP Express", Synonyms = new List<string>(){ "Up Express","Train to downtown","up express", "Train to city" ,"express", "upexpress","UP" }  },
                        new Choice(){Value = "Public Transit", Synonyms = new List<string>(){ "public transit", "publictransit","ttc","subway","go transit","miway","brampton transit","public transport" }  }
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
            _logger.LogTrace("ParkingAndTransaportationDialog.RouteToSpecificDialogAsync starts");
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"ParkingAndTransaportationDialog.RouteToSpecificDialogAsync Choice:{foundChoice.Value}");
            if (foundChoice.Value.ToLower() == "parking")
            {
                _logger.LogTrace("ParkingAndTransaportationDialog.RouteToSpecificDialogAsync end");
                return await stepContext.BeginDialogAsync(nameof(ParkingDialog));
            }
            else if (foundChoice.Value.ToLower() == "car rentals")
            {
                _logger.LogTrace("ParkingAndTransaportationDialog.RouteToSpecificDialogAsync end");
                return await stepContext.BeginDialogAsync(nameof(CarRentalsDialog));
            }
            else if (foundChoice.Value.ToLower() == "taxi/limos/rideshare")
            {
                _logger.LogTrace("ParkingAndTransaportationDialog.RouteToSpecificDialogAsync end");
                return await stepContext.BeginDialogAsync(nameof(TaxiLimosRideShareDialog));
            }
            else if (foundChoice.Value.ToLower() == "up express")
            {
                _logger.LogTrace("ParkingAndTransaportationDialog.RouteToSpecificDialogAsync end");
                return await stepContext.BeginDialogAsync(nameof(UPExpressDialog));
            }
            else
            {
                _logger.LogTrace("ParkingAndTransaportationDialog.RouteToSpecificDialogAsync end");
                return await stepContext.BeginDialogAsync(nameof(PublicTransitDialog));
            }
        }


    }
}
