using GTAADevChatBot.Dialogs.YesNoDialog;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.LostAndFound.TeminalsAndOther
{
    public class OnTerminalAndOtherDialog : CancelAndHelpDialog
    {

        private readonly ILogger _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        public OnTerminalAndOtherDialog(ILoggerFactory loggerFactory) : base(nameof(OnTerminalAndOtherDialog))
        {
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("OnTerminalandOtherDialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                LostandFoundOnTerminalAndOther,
                LostAndFiledReportDialogStepAsync
                //ConfirmStepAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), CustomPromptValidatorAsync) { Style = ListStyle.SuggestedAction });
            //   AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });            
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> LostandFoundOnTerminalAndOther(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("OnTerminalandOtherDialog.LostandFoundOnTerminalAndOther End ");

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Ok, have you filed a report for your item?"),
                       RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                       Choices = new List<Choice>() {
                        new Choice() { Value = "Yes",
                           Synonyms = new List<string>(){"yes","YES","Y", "ya", "yep", "Yeah" } },

                        new Choice(){Value = "No" ,
                            Synonyms = new List<string>(){"no","NO","N" } }

                       }
                   }, cancellationToken);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> LostAndFiledReportDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            _logger.LogTrace("OnTerminalandOtherDialog.LostAndFiledReportDialogStepAsync Begin ");
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value == "Yes")
            {
                var msg = "Thanks for filing a report. You will receive a notification if the item has been found. Please be sure to check your email junk/spam folders if you haven't received any notifications.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                _logger.LogTrace("OnTerminalandOtherDialog.LostAndFiledReportDialogStepAsync. Yes End ");
                return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));

            }

            else
            {
                var msg = "Please file an online report for your missing item, including any information that you think will help our team identify your item. \r\r After submitting this report, you will receive an email confirmation with a report number. The email will also have instructions on how to update your report, if needed. \r\r File a report at https://www.torontopearson.com/en/while-you-are-here/toronto-airport-services/lost-and-found";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                _logger.LogTrace("OnTerminalandOtherDialog.LostAndFiledReportDialogStepAsync.No End ");
                return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
            }
        }


        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {

                return await Task.FromResult(true);

            }
            else
            {
                return await Task.FromResult(false);
            }
        }

    }
}
