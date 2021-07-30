using GTAADevChatBot.Dialogs.YesNoDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.SecurityScreening
{
    public class SecurityScreeningDialog : CancelAndHelpDialog
    {

        private readonly ILogger _logger;
        public SecurityScreeningDialog(ILoggerFactory loggerFactory) : base(nameof(SecurityScreeningDialog))
        {
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("AirportAndSecurityScreeningInfoDialog.");

            var waterfallSteps = new WaterfallStep[]
           {
                    SecurityScreeningDialogAsync

           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SecurityScreeningDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AirportAndSecurityScreeningInfoDialog.AirportAndSecurityScreeningInfo  Begin");
            var msg = "After you’ve checked in and dropped off any bags, you’re ready to go through security. The security process checks all carry-on bags and personal items as well as each individual person according to the rules set out by the Canadian Air Transport Security Authority (CATSA).";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            var msg1 = "Visit CATSA's website to learn what you can bring on your flight at https://www.catsa-acsta.gc.ca/en.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg1), cancellationToken);

            _logger.LogTrace("AirportAndSecurityScreeningInfoDialog.AirportAndSecurityScreeningInfo  End");

            return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
        }
    }
}
