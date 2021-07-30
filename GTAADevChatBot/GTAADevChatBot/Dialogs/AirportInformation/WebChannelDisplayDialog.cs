using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation
{
    public class WebChannelDisplayDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region Constructor
        public WebChannelDisplayDialog(ILoggerFactory loggerFactory) : base(nameof(WebChannelDisplayDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Web channel display dialog.");

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
        #endregion

        /// <summary>
        /// Present available Food, Retail and Service Amnenities option.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foodRetailServiceBOState = (FoodRetailServiceBO)stepContext.Options;
            int skip = foodRetailServiceBOState.Skip;
            var reply = stepContext.Context.Activity.CreateReply();
            List<IFoodRetailService> lstFoodRetailService = foodRetailServiceBOState.FoodRetailServiceCollection;
            foreach (var item in lstFoodRetailService.Skip(skip))
            {
                reply.Attachments.Add(CreateFoodAndBeverageAdaptiveCardAttachment(item, foodRetailServiceBOState));
            }
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            await stepContext.Context.SendActivityAsync(reply);

            if (!(foodRetailServiceBOState.Skip >= 0 && (foodRetailServiceBOState.Skip + 25) >= foodRetailServiceBOState.FoodRetailServiceCollection.Count))
            {
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("We have even more options! Do you want to see other shops nearby?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)

                });
            }
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else? "),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
            });
        }

        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foodRetailServiceBOState = (FoodRetailServiceBO)stepContext.Options;


            if ((bool)stepContext.Result)
            {
                if ((foodRetailServiceBOState.Skip + 25) >= foodRetailServiceBOState.FoodRetailServiceCollection.Count)
                {
                    await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken);
                }
                else
                {
                    foodRetailServiceBOState.Skip += 25;
                    return await stepContext.ReplaceDialogAsync(nameof(WebChannelDisplayDialog), foodRetailServiceBOState, cancellationToken);
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


        /// <summary>
        /// Show Adaptive card
        /// </summary>
        /// <param name="restaurant"></param>
        /// <param name="objFoodRetailServiceBO"></param>
        /// <returns></returns>
        private Attachment CreateFoodAndBeverageAdaptiveCardAttachment(IFoodRetailService foodRetailService, FoodRetailServiceBO objFoodRetailServiceBO)
        {
            var terminal = foodRetailService.Terminal == "T1" ? "1" : "3";
            var location = foodRetailService.Location;
            var country = objFoodRetailServiceBO?.DepartureArea;

            var locationText = string.Empty;
            if (foodRetailService.Security == "Post")
            {
                locationText = "After security" + " (" + country + ") - " + "Near gate " + location;

            }
            else
            {
                locationText = "Before security";
            }
            string[] paths = new string[] { ".", "Dialogs", "Resources", "FoodAndBeverageAdptiveCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);

            adaptiveCard = adaptiveCard.Replace("<Name>", foodRetailService.Name);
            adaptiveCard = adaptiveCard.Replace("<Description>", foodRetailService.Description);
            adaptiveCard = adaptiveCard.Replace("<ImageUrl>", foodRetailService.Image);
            adaptiveCard = adaptiveCard.Replace("<Location>", "Terminal " + terminal + " - " + locationText);
            adaptiveCard = adaptiveCard.Replace("<Timings>", foodRetailService.Hours);
            adaptiveCard = adaptiveCard.Replace("<RestURL>", foodRetailService.URL);
            adaptiveCard = adaptiveCard.Replace("<altText>", foodRetailService.Name);
            adaptiveCard = adaptiveCard.Replace("<OpenImageURL>", foodRetailService.URL);

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}
