using GTAADevChatBot.Dialogs.YesNoDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.Accessibility
{
    public class TravellingWithAnimalDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public TravellingWithAnimalDialog(ILoggerFactory loggerFactory) : base(nameof(TravellingWithAnimalDialog))
        {
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("TravellingWithAnimalDialog.");

            var waterfallSteps = new WaterfallStep[]
           {
                   TravellingwithanAnimalDialogStepAsync

           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> TravellingwithanAnimalDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("TravellingwithanAnimalDialogStepAsync Begin");

            var msg = "All leashed service animals are welcome. Please notify your airline if you will be travelling with a service animal.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            var msg1 = "For your convenience, pet relief areas are available before and after security. Learn more at https://www.torontopearson.com/en/while-you-are-here/travel-with-pets.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg1), cancellationToken);

            _logger.LogTrace("TravellingwithanAnimalDialogStepAsync  End");
            return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
        }
    }
}





