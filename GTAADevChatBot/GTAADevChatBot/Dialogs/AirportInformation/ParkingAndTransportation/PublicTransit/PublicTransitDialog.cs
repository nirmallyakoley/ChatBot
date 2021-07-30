using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.PublicTransit
{
    public class PublicTransitDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region Constructor
        public PublicTransitDialog(ILoggerFactory loggerFactory) : base(nameof(PublicTransitDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Public transit Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                ProvidePublicTransitInfoStepAsync,
                ConfirmStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion
        /// <summary>
        /// Provide infor on Public transit and render for restart or end options
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvidePublicTransitInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("PublicTransitDialog.ProvidePublicTransitInfoStepAsync starts.");
            await stepContext.Context.SendActivityAsync("There are many transit options for Toronto Pearson throughout the Greater Toronto Area. Please use the trip planner on our website to find the best route for you! Click here for details: [https://www.torontopearson.com/en/transportation/public-transit-buses](https://www.torontopearson.com/en/transportation/public-transit-buses)");
            _logger.LogTrace("PublicTransitDialog.ProvidePublicTransitInfoStepAsync end.");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else?"),
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
            _logger.LogTrace("PublicTransitDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("PublicTransitDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("PublicTransitDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

    }
}
