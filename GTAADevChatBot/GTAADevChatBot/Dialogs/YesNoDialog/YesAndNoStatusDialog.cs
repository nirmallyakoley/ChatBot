using GTAADevChatBot.DialogEnd;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.YesNoDialog
{
    public class YesAndNoStatusDialog : ComponentDialog
    {
        public YesAndNoStatusDialog() : base(nameof(YesAndNoStatusDialog))
        {

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                   PromptstepAsync,
                    ConfirmStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }




        private async Task<DialogTurnResult> PromptstepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else?"),
            });
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync();
            }
            else
            {
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }





    }
}
