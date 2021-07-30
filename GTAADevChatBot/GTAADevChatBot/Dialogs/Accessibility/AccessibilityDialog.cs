using GTAADevChatBot.Dialogs.YesNoDialog;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.Accessibility
{
    public class AccessibilityDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        BotServices _botServices = null;
        public AccessibilityDialog(BotServices botServices, ILoggerFactory loggerFactory) : base(nameof(AccessibilityDialog))
        {
            _botServices = botServices;
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("AccessibilityDialog.");

            var waterfallSteps = new WaterfallStep[]
             {
                    AccessibilityInformation,
                    ChooseAccessibilityDialogStepAsync

             };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), CustomPromptValidatorAsync) { Style = ListStyle.SuggestedAction });
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> AccessibilityInformation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AccessibilityDialog.AccessibilityInformation  Begin");

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("We’re committed to providing a smooth and enjoyable airport experience for all. How can I help?"),
                   RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                   Choices = new List<Choice>() {

                        new Choice() { Value = "Mobility & special assistance",
                           Synonyms = new List<string>(){ "Mobility Assistance", "Blind", "Visually imparied", "Visually challenged", "Seeing imparied", "Deaf", "Hearing impaired", "Special Assistance", "Mobility",  "Assistance"   } },

                        new Choice(){Value = "Service animals" ,
                            Synonyms = new List<string>(){ "With a Service Animal", "Travelling with a service animal", "Travelling with a se..." } },

                         new Choice(){Value = "Accessible washrooms" ,
                            Synonyms = new List<string>(){ "wash rooms", "Change rooms", "Accessibile Washrooms" ,"Assisted Change Rooms", "Accessible changerooms" ,"Washrooms","ChangeRooms","Family Rooms"} },

                        new Choice(){Value = "MagnusCards" ,
                            Synonyms = new List<string>(){"Magnus", "cognitive special needs", "Autism" ,"cognitive needs","Cognitive","MagnusCards"},
                        }
                   }
               }, cancellationToken);

        }

        private async Task<DialogTurnResult> ChooseAccessibilityDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AccessibilityDialog.ChooseAccessibilityDialogStepAsync Begin");

            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value == "Mobility & special assistance")
            {
                return await stepContext.BeginDialogAsync(nameof(MobilityAssistanceDialog), cancellationToken);
            }

            else if (foundChoice.Value == "Service animals")
            {
                return await stepContext.BeginDialogAsync(nameof(TravellingWithAnimalDialog), cancellationToken);
            }

            else if (foundChoice.Value == "Accessible washrooms")
            {
                return await stepContext.BeginDialogAsync(nameof(WashRoomsDialog), cancellationToken);

            }
            else if (foundChoice.Value == "MagnusCards")
            {
                return await stepContext.BeginDialogAsync(nameof(MagnusCardsDialog), cancellationToken);
            }

            else if (foundChoice.Value == "MagnusCards")
            {
                return await stepContext.BeginDialogAsync(nameof(MagnusCardsDialog), cancellationToken);
            }

            else
            {
                var msg = "I'm sorry, I can't seem to find. Special Assistance, including mobility and wheelchair services are available to all passengers at Toronto Pearson. How can I help you?";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
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
