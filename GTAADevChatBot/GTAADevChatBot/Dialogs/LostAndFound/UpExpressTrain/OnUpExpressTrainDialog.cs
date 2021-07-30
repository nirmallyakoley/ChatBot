using GTAADevChatBot.Dialogs.YesNoDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.LostAndFound.UpExpressTrain
{
    public class OnUpExpressTrainDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        public OnUpExpressTrainDialog(ILoggerFactory loggerFactory) : base(nameof(OnUpExpressTrainDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();

            _logger.LogTrace("OnUpExpressTrainDialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                LostandFoundOnUpExpressTrain

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            // AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> LostandFoundOnUpExpressTrain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("OnUpExpressTrainDialog.LostandFoundOnUpExpressTrain Begin");

            var msg = "For any items left on the UP Express train, you will need to file a report with UP Express directly at https://www.upexpress.com/CustomerSupport/LostFound";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            _logger.LogTrace("OnUpExpressTrainDialog.LostandFoundOnUpExpressTrain End");
            return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));

        }
    }
}
