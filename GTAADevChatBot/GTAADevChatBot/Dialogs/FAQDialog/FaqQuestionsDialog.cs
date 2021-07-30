using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.FaQDialog
{
    public class FaqQuestionsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        private BotServices _botServices = null;
        public const string QnAMakerKey = "gtaadevchatbotqnamaker-aseqg4mb6nonjwe";
        #region constructor
        public FaqQuestionsDialog(ILoggerFactory loggerFactory, BotServices botService) : base(nameof(FaqQuestionsDialog))
        {
            _botServices = botService;

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("FaqQuestionsDialog Dialog.");
            var waterfallSteps = new WaterfallStep[]
            {
                ProvideAnswerStepAsync,
                ConfirmStepAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });

            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion
        /// <summary>
        /// Call QnA and return the answer
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideAnswerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("FaqQuestionsDialog.ProvideAnswerStepAsync start");
            QueryResult[] response;
            response = await _botServices.QnAServices[QnAMakerKey].GetAnswersAsync(stepContext.Context).ConfigureAwait(false);
            if (response != null && response.Length > 0)
            {
                await stepContext.Context.SendActivityAsync(response[0].Answer, cancellationToken: cancellationToken);
                _logger.LogTrace("FaqQuestionsDialog.ProvideAnswerStepAsync end");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                     cancellationToken: cancellationToken);

            }
            else
            {
                _logger.LogTrace("FaqQuestionsDialog.ProvideAnswerStepAsync end");
                var msg = "I’m sorry I did not understand you. You may also type **RESTART** to start over or type **SUPPORT** for a live agent.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                    cancellationToken: cancellationToken);
            }

        }

        /// <summary>
        /// Restart or end the conversation as user wants
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("FaqQuestionsDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("FaqQuestionsDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("FaqQuestionsDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }
    }
}
