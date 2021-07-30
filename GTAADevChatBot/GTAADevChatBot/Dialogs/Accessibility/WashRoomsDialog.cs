using GTAADevChatBot.Dialogs.YesNoDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.Accessibility
{
    public class WashRoomsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public WashRoomsDialog(ILoggerFactory loggerFactory) : base(nameof(WashRoomsDialog))

        {
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("WashRoomsDialog.");

            var waterfallSteps = new WaterfallStep[]
           {
                   WashroomsDialogStepAsync

           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> WashroomsDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {
            _logger.LogTrace("WashroomsDialogStepAsync Begin");

            var msg = "All washrooms include wheelchair accessible facilities. \r\r Assisted change rooms are available in Terminal 1 and 3. Learn more at https://www.torontopearson.com/en/accessibility/accessible-toilets-changing-rooms.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            _logger.LogTrace("WashroomsDialogStepAsync  End");
            return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));



        }
    }
}
