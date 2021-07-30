using GTAADevChatBot.Dialogs.AirlineInformation;
using GTAADevChatBot.Dialogs.FlightStatus;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.FlightBranchDialog
{
    public class BranchFlightDialog : CancelAndHelpDialog
    {
        private BotServices _botServices;
        public BranchFlightDialog(IStatePropertyAccessor<FlightStatusState> statePropertyAccessor, IStatePropertyAccessor<AirlineInformationBO> airlineStatePropertyAccessor, BotServices botServices, ILoggerFactory loggerFactory) : base(nameof(BranchFlightDialog))
        {
            _botServices = botServices;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new FlightStatusDialog(statePropertyAccessor, botServices, loggerFactory));
            AddDialog(new AirlineInformationDialog(airlineStatePropertyAccessor, loggerFactory, botServices));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ChooseDialogStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Okay! Do you need to check on flight status or airline information?"),
                    Choices = new List<Choice>() {
                        new Choice() {  Value = "Flight status", Synonyms = new List<string> { "Flight", "flt", "Flight Info","flight status","status" } },
                        new Choice(){ Value = "Airline information", Synonyms = new List<string> { "Airline", "al", "Airline Info" } }
                    }
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ChooseDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value == "Flight status")
                return await stepContext.BeginDialogAsync(nameof(FlightStatusDialog));
            else if (foundChoice.Value == "Airline information")
            {
                return await stepContext.BeginDialogAsync(nameof(AirlineInformationDialog));
            }
            else
                return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Context.Activity.Text == "Flight Information")
                    return await Task.FromResult(true);
                else if (promptContext.Context.Activity.Text == "Airline Information")
                    return await Task.FromResult(true);
                else
                    return await Task.FromResult(false);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }
    }
}
