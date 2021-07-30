using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.CarRentals
{

    public class CarRentalsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region Constructor
        public CarRentalsDialog(ILoggerFactory loggerFactory) : base(nameof(CarRentalsDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Car rentals Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                ProvideCarRentalsDetailStepAsync,
                ConfirmStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        private async Task<DialogTurnResult> ProvideCarRentalsDetailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CarRentalsDialog.ProvideCarRentalsDetailStepAsync starts.");
            await stepContext.Context.SendActivityAsync("You're in luck! We've got a number of car rental companies available at Toronto Pearson. For a full list please click this link to our website: [https://www.torontopearson.com/en/transportation/car-rentals](https://www.torontopearson.com/en/transportation/car-rentals)");
            _logger.LogTrace("CarRentalsDialog.ProvideCarRentalsDetailStepAsync end.");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Is there anything else I can help you with?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
            },
                cancellationToken: cancellationToken);
        }
        /// <summary>
        ///restart or end the conversation.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CarRentalsDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("CarRentalsDialog.ConfirmStepAsync end.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("CarRentalsDialog.ConfirmStepAsync end.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

    }
}
