using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using GTAADevChatBot.Facebook;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation
{
    public class FaceBookChannelDisplayDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        // public IStatePropertyAccessor<FoodRetailServiceBO> FoodRetailServicebOState { get; }
        public FaceBookChannelDisplayDialog(ILoggerFactory loggerFactory) : base(nameof(FaceBookChannelDisplayDialog))
        {
            //FoodRetailServicebOState = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("FaceBookChannel display dialog.");

            var waterfallSteps = new WaterfallStep[]
            {

                    ShowCardStepAsync,
                    SummarystepAsync,
                    ConfirmStepAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foodRetailServiceBOState = (FoodRetailServiceBO)stepContext.Options;
            var reply = stepContext.Context.Activity.CreateReply();
            int skip = foodRetailServiceBOState.Skip;

            FacebookChannelData objFacebookChannelData = FacebookData.ProvideFaceBookChannelData(foodRetailServiceBOState.FoodRetailServiceCollection.Skip(skip).Take(10).ToList<IFoodRetailService>(), foodRetailServiceBOState);
            reply.ChannelData = objFacebookChannelData;
            await stepContext.Context.SendActivityAsync(reply);


            if (!(foodRetailServiceBOState.Skip >= 0 && (foodRetailServiceBOState.Skip + 10) >= foodRetailServiceBOState.FoodRetailServiceCollection.Count))
            {
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("We have even more options! Do you want to see other shops nearby?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)

                });
            }
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
            });
        }

        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foodRetailServiceBOState = (FoodRetailServiceBO)stepContext.Options;


            if ((bool)stepContext.Result)
            {
                if ((foodRetailServiceBOState.Skip + 10) >= foodRetailServiceBOState.FoodRetailServiceCollection.Count)
                {
                    await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken);
                }
                else
                {
                    foodRetailServiceBOState.Skip += 10;
                    return await stepContext.ReplaceDialogAsync(nameof(FaceBookChannelDisplayDialog), foodRetailServiceBOState, cancellationToken);
                }

            }
            else
            {
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }

        }
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

    }
}
