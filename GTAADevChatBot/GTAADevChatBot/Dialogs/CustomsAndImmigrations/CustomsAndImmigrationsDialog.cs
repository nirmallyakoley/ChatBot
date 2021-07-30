using GTAADevChatBot.Dialogs.CustomsAndImmigrations.ForCanada;
using GTAADevChatBot.Dialogs.CustomsAndImmigrations.ForUSA;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.CustomsAndImmigrations
{
    public class CustomsAndImmigrationsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region constructor
        public CustomsAndImmigrationsDialog(ILoggerFactory loggerFactory) : base(nameof(CustomsAndImmigrationsDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Customs And Immigartions Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                AskForCustomsStepAsync,
                RouteToUSAOrCanadaCustoms
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new USACustomsDialog(loggerFactory));
            AddDialog(new CanadaCustomsDialog(loggerFactory));
        }
        #endregion

        /// <summary>
        /// Rendering choice for USA/Canada Customs 
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskForCustomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CustomsAndImmigrationsDialog.AskForCustomsStepAsync starts.");
            _logger.LogTrace("CustomsAndImmigrationsDialog.AskForCustomsStepAsync ends.");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Are you looking for information on Canadian or US Customs?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                    Choices = new List<Choice>() {
                        new Choice() { Value = "US", Synonyms = new List<string>(){ "united states", "united states of america", "usa","america" } },
                        new Choice(){Value = "Canada", Synonyms = new List<string>(){ "canada", "canadian" }  }
            }
                }, cancellationToken);
        }
        /// <summary>
        /// Route to USA or Canada as per user choice.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> RouteToUSAOrCanadaCustoms(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CanadaCustomsDialog.RouteToUSAOrCanadaCustoms starts.");
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"CanadaCustomsDialog.RouteToUSAOrCanadaCustoms: Choice Value = {foundChoice.Value.ToLower()}.");
            if (foundChoice.Value.ToLower() == "us")
            {
                _logger.LogTrace("CanadaCustomsDialog.RouteToUSAOrCanadaCustoms ends.");
                return await stepContext.BeginDialogAsync(nameof(USACustomsDialog), cancellationToken: cancellationToken);
            }
            else
            {
                _logger.LogTrace("CanadaCustomsDialog.RouteToUSAOrCanadaCustoms ends.");
                return await stepContext.BeginDialogAsync(nameof(CanadaCustomsDialog), cancellationToken: cancellationToken);

            }
        }
    }
}
