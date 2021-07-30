using GTAADevChatBot.Dialogs.YesNoDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.Accessibility
{
    public class MagnusCardsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public MagnusCardsDialog(ILoggerFactory loggerFactory) : base(nameof(MagnusCardsDialog))

        {
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("MagnusCardsDialog.");

            var waterfallSteps = new WaterfallStep[]
           {
                   MagnusCardsDialogStepAsync

           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> MagnusCardsDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("MagnusCardsDialogStepAsync Begin");
            var msg = "MagnusCards is a life skills app developed for people with autism and other cognitive special needs. \r\r Before you travel, you can download any of the Pearson Card Decks. These decks will help you navigate common airport processes, like airline check-in and boarding a plane. Learn more at https://www.torontopearson.com/en/accessibility/autism-app-magnuscards.";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            _logger.LogTrace("MagnusCardsDialogStepAsync  End");
            return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));

        }
    }
}
