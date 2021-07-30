using GTAADevChatBot.Dialogs.YesNoDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.ConnectionsGuide
{

    public class ConnectionsGuideDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public ConnectionsGuideDialog(ILoggerFactory loggerFactory) : base(nameof(ConnectionsGuideDialog))
        {
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("ConnectionGuideDialog.");

            var waterfallSteps = new WaterfallStep[]
           {
                    ConnectionsGuide


           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> ConnectionsGuide(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("ConnectionsGuideDialog.ConnectionGuide  Begin");
            var msg = "Enter your travel information on our website for a step-by-step flight connection guide at https://www.torontopearson.com/connections.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            _logger.LogTrace("ConnectionsGuideDialog.ConnectionGuide  End");
            return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
        }
    }

}
