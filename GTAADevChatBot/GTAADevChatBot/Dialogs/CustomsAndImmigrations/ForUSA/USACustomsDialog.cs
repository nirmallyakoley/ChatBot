using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.CustomsAndImmigrations.ForUSA
{
    public class USACustomsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region contructor
        public USACustomsDialog(ILoggerFactory loggerFactory) : base(nameof(USACustomsDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("USA Customs Dialog.");

            var waterfallSteps = new WaterfallStep[]
           {
                    ProvideInformationStepAsync,
                    ProvideContactInforamtionStepAsync,
                    ConfirmStepAsync
           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });

            //The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Render choice for Contact info
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideInformationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("USACustomsDialog.ProvideInformationStepAsync starts.");
            var msg = "Okay! At Pearson Airport, passengers travelling to the US will clear customs and immigration before their flight. US Customs opens daily at 0330 in Terminal 3 and 0430 in Terminal 1.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            _logger.LogTrace("USACustomsDialog.ProvideInformationStepAsync ends.");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = MessageFactory.Text("Do you need more information about US Customs?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                Choices = new List<Choice>()
                {
                    new Choice() { Value = "Yes",
                           Synonyms = new List<string>(){"Y","Yeah","yes","YES","yep","ya" } },

                        new Choice(){Value = "No" ,
                            Synonyms = new List<string>(){"N","Nope","no" } }
                }
            }, cancellationToken);

        }
        /// <summary>
        /// Provide Contact info andrender for anything else is needed.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideContactInforamtionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("USACustomsDialog.ProvideContactInforamtionStepAsync starts.");
            FoundChoice choice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"USACustomsDialog.ProvideContactInforamtionStepAsync : Choice {choice.Value.ToLower()}.");
            if (choice.Value.ToLower() == "yes")
            {
                await stepContext.Context.SendActivityAsync($"Please contact US Customs and Border Protection at 1-212-325-8000 or visit their website at https://www.cbp.gov/.");

                _logger.LogTrace("USACustomsDialog.ProvideContactInforamtionStepAsync ends.");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                 cancellationToken: cancellationToken);
            }
            else
            {
                _logger.LogTrace("USACustomsDialog.ProvideContactInforamtionStepAsync ends.");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                 cancellationToken: cancellationToken);
            }

        }

        /// <summary>
        ///restart or end the conversation.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("USACustomsDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("USACustomsDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("USACustomsDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

    }
}
