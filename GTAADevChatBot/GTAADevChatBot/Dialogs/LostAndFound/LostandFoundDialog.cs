using GTAADevChatBot.Dialogs.LostAndFound.AirCraft;
using GTAADevChatBot.Dialogs.LostAndFound.TeminalsAndOther;
using GTAADevChatBot.Dialogs.LostAndFound.UpExpressTrain;
using GTAADevChatBot.Dialogs.YesNoDialog;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.LostAndFound
{
    public class LostandFoundDialog : CancelAndHelpDialog
    {
        BotServices _botServices = null;
        private readonly ILogger _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        public LostandFoundDialog(BotServices botServices, ILoggerFactory loggerFactory) : base(nameof(LostandFoundDialog))
        {
            _botServices = botServices;
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("LostandFoundDialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                AskLostandFoundStepAsync,
                ChooseLostDialogStepAsync,
                LostSomethingElseDialogStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), CustomPromptValidatorAsync) { Style = ListStyle.SuggestedAction });
            AddDialog(new OnAircraftDialog(_botServices, loggerFactory));
            AddDialog(new OnTerminalAndOtherDialog(loggerFactory));
            AddDialog(new OnUpExpressTrainDialog(loggerFactory));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskLostandFoundStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("LostandFoundDialog.AskLostandFoundStepAsync Begin");
            var msg = "Sorry to hear you've lost something! Let me ask you a few questions that will help to assist you better.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            _logger.LogTrace("LostandFoundDialog.AskLostandFoundStepAsync End");

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Is your lost item a Passport, Nexus Card or something else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                    Choices = new List<Choice>() {
                        new Choice() { Value = "Passport",
                           Synonyms = new List<string>(){ "passport","Passport","PASSPORT" } },

                        new Choice(){Value = "Nexus" ,
                            Synonyms = new List<string>(){ "Nexus","Nexus","NEXUS", "Nexus card", "NEXUS Card" } },

                        new Choice(){Value = "Something else" ,
                            Synonyms = new List<string>()
                            {
                                "Something Else","other","different","Something",
                                "Other Item","Other Items","Another Item", "Another Items","laptop","phone",
                                "ring","jacket","hat","ID","duty free","can","show","coat","PR card","wallet","purse",
                                "watch","ipad","tablet","camera","documents","charger","mobile"
                            }
                        }
                    }

                }, cancellationToken);

        }

        private async Task<DialogTurnResult> ChooseLostDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("LostandFoundDialog.ChooseLostDialogStepAsync Begin");

            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value == "Passport")
            {
                var msg = "All lost passports found at the airport or left on an aircraft are turned into Canada Border Services Agency (CBSA). You can reach them at \r\r Terminal 1: 905-676-3640  \r\r Terminal 3: 905-612-5416";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                _logger.LogTrace("LostandFoundDialog.ChooseLostDialogStepAsync.Passport End");
                return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));

            }

            else if (foundChoice.Value == "Nexus")
            {
                var msg = "All lost Nexus cards found at the airport are turned into the Nexus office. You may contact them directly at 905-676-3477";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                _logger.LogTrace("LostandFoundDialog.ChooseLostDialogStepAsync.Nexus End");
                return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
            }

            else
            {
                _logger.LogTrace("LostandFoundDialog.ChooseLostDialogStepAsync.Something Else End");

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please tell me where you think you may have left your item."),
                        RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                        Choices = new List<Choice>()
                        {

                            new Choice()
                            {
                                Value = "On aircraft",
                                Synonyms = new List<string>()
                                    {
                                        "Aircraft", "Aeroplane","Airbus","Airliner","Airship","jet", "airplane", "plane"
                                    }
                            },

                            new Choice()
                            {
                                Value = "Terminals, garage or LINK Train" ,
                                Synonyms = new List<string>()
                                    {
                                    "Terminals","garage","Train", "Link Train", "Terminal 1", "Terminal 3", "T1","T3", "Terminal",
                                    "gate", "departure gate", "lounge", "security", "at security", "restaurant", "bar", "customs",
                                    "Canada customs", "washroom", "check-in", "US customs", "store", "baggage hall", "baggage area","link","terminal train","taxi","limo","limousine","bathroom"
                                    }
                            },

                            new Choice(){Value = "UP Express" ,
                                Synonyms = new List<string>(){ "Express Train", "Train to City", "Up Express Train","UP Express","upexpress","up express","UP" } }

                        }
                    }, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> LostSomethingElseDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("LostandFoundDialog.LostSomethingElseDialogStepAsync Begin");
            string _OnAircraft = "On aircraft";
            string _TerminalAndOther = "Terminals, garage or LINK Train";

            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value.ToString() == _OnAircraft)
            {

                _logger.LogTrace("LostandFoundDialog.LostSomethingElseDialogStepAsync. onAircraft End");
                return await stepContext.BeginDialogAsync(nameof(OnAircraftDialog), cancellationToken);
            }

            else if (foundChoice.Value.ToString() == _TerminalAndOther)
            {

                _logger.LogTrace("LostandFoundDialog.LostSomethingElseDialogStepAsync.Terminal&Other End");
                return await stepContext.BeginDialogAsync(nameof(OnTerminalAndOtherDialog), cancellationToken);
            }

            else
            {

                _logger.LogTrace("LostandFoundDialog.LostSomethingElseDialogStepAsync.ExpressTrain End");
                return await stepContext.BeginDialogAsync(nameof(OnUpExpressTrainDialog), cancellationToken);
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
