using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.Parking
{
    public class ParkingDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region Constructor
        public ParkingDialog(ILoggerFactory loggerFactory) : base(nameof(ParkingDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }
            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Parking Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                ProvideAvailableOptionsStepAysnc,
                ProvideInfoBasedOnSelectionStepAsync,
                ConfirmStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });

            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Provide available parking options
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideAvailableOptionsStepAysnc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("ParkingDialog.ProvideAvailableOptionsStepAysnc start");
            _logger.LogTrace("ParkingDialog.ProvideAvailableOptionsStepAysnc end");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
             new PromptOptions
             {
                 Prompt = MessageFactory.Text("Great! How long do you intend to park with us?"),
                 RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                 Choices = new List<Choice>() {
                        new Choice() { Value = "Less than 3 hours", Synonyms = new List<string>(){ "hourly","hour basis","hourly basis"," hour","per hour","1 hour","2 hour","less than 3" } },
                        new Choice(){Value = "More than 3 hours", Synonyms = new List<string>(){ "daily", "long term","daily basis","daily and long term","more than 3 hour","more than 3" }  },
                        new Choice(){Value = "Just picking someone up", Synonyms = new List<string>(){ "cell phone", "cell phone wait area", "waiting area", "Just picking someone..." }  }
                }
             }, cancellationToken);
        }

        /// <summary>
        /// Provide parking info based on user selection
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideInfoBasedOnSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("ParkingDialog.ProvideInfoBasedOnSelectionStepAsync start");
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"ParkingDialog.ProvideInfoBasedOnSelectionStepAsync choice:{foundChoice.Value}");
            if (foundChoice.Value.ToLower() == "less than 3 hours")
            {
                _logger.LogTrace("ParkingDialog.ProvideInfoBasedOnSelectionStepAsync end");
                var msg = "For short stays, the best options are Express or Daily Park. Learn more at https://www.torontopearson.com/parking.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                _logger.LogTrace("ParkingDialog.ProvideInfoBasedOnSelectionStepAsync end");
            }
            else if (foundChoice.Value.ToLower() == "more than 3 hours")
            {
                var msg = "For longer stays, we have on-site parking at Terminals 1 and 3, Valet Care, or Value Park options. For more information on your options, or to guarantee the best rate and reserve your spot online visit https://www.torontopearson.com/parking.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                _logger.LogTrace("ParkingDialog.ProvideInfoBasedOnSelectionStepAsync end");
            }
            else
            {
                var msg = "We have free cell phone waiting areas. If you're picking up friends or family, you can use these lots while you wait. Learn more and get directions at https://www.torontopearson.com/en/transportation/parking/cell-phone-waiting-area.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                _logger.LogTrace("ParkingDialog.ProvideInfoBasedOnSelectionStepAsync end");
                
            }
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
            },
                     cancellationToken: cancellationToken);

        }


        /// <summary>
        /// Ask for restart or end the conversation
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {

                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                _logger.LogTrace("ParkingDialog.ConfirmStepAsync ends.");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("ParkingDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }
    }

}
