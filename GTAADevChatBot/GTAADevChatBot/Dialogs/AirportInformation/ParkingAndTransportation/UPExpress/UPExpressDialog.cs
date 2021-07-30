using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.UPExpress
{

    public class UPExpressDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region Constructor
        public UPExpressDialog(ILoggerFactory loggerFactory) : base(nameof(UPExpressDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("UP Express Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                ProvideUPExpressInfoStepAsync,
                ConfirmStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Provide UP Express information and render if anything else is needed
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideUPExpressInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("UPExpressDialog.ProvideUPExpressInfoStepAsync starts.");
            await stepContext.Context.SendActivityAsync("Getting downtown has never been easier! The UP Express train departs Toronto Pearson every 15 minutes for Union Station with 2 stops along the way. They can have you in the heart of the city in just 25 minutes!");
            await stepContext.Context.SendActivityAsync(" If you would like information on ticket prices and service hours please click here: [https://www.upexpress.com/](https://www.upexpress.com/)");
            _logger.LogTrace("UPExpressDialog.ProvideUPExpressInfoStepAsync end.");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else? "),
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
            _logger.LogTrace("TaxiLimosRideShareDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("TaxiLimosRideShareDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("TaxiLimosRideShareDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }
    }
}
