using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages
{
    public class FoodAndBeverageDialog : CancelAndHelpDialog
    {
        public IStatePropertyAccessor<FoodRetailServiceBO> foodStatus { get; }
        private readonly ILogger _logger;
        private BotServices _botService;

        #region Constructor
        public FoodAndBeverageDialog(IStatePropertyAccessor<FoodRetailServiceBO> statePropertyAccessor, ILoggerFactory loggerFactory,BotServices botService) : base(nameof(FoodAndBeverageDialog))
        {
            _botService = botService;
            foodStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Food and Beverage Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskForTerminalStepAsync,
                AskForDepartureArea,
                AskForRestaurantStepAsync



            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new FaceBookChannelDisplayDialog(loggerFactory));
            AddDialog(new WebChannelDisplayDialog(loggerFactory));
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Initialize state for Food and Beverage
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("FoodAndBeverageDialog.InitializeStateStepAsync start");
            var foodAndBevStatusState = (FoodRetailServiceBO)stepContext.Options;
            if (foodAndBevStatusState == null)
            {
                var foodAndBevStatusStateOpt = stepContext.Options as FoodRetailServiceBO;
                if (foodAndBevStatusStateOpt != null)
                {
                    await foodStatus.SetAsync(stepContext.Context, foodAndBevStatusStateOpt);
                }
                else
                {
                    await foodStatus.SetAsync(stepContext.Context, new FoodRetailServiceBO());
                }
            }
            _logger.LogTrace("FoodAndBeverageDialog.InitializeStateStepAsync end");
            return await stepContext.NextAsync();
        }
        /// <summary>
        /// Render for the Terminal choice
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskForTerminalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("FoodAndBeverageDialog.AskForTerminalStepAsync start");
            //var foodAndBevStatusState = await foodStatus.GetAsync(stepContext.Context, () => null);
            _logger.LogTrace("FoodAndBeverageDialog.AskForTerminalStepAsync end");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Our restaurants offer everything from top Canadian chefs to family-friendly and everything in between!"),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Terminal 1", Synonyms = new List<string>(){ "terminal1","t1" } },
                        new Choice(){Value = "Terminal 3", Synonyms = new List<string>(){ "terminal3","t3" }  },
                        new Choice(){Value = "Show me everything"}
             }
                 }, cancellationToken);
        }

        /// <summary>
        /// Render choice fpr Departure area
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskForDepartureArea(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("FoodAndBeverageDialog.AskForDepartureArea start");
            var foodAndBevStatusState = await foodStatus.GetAsync(stepContext.Context);
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"FoodAndBeverageDialog.AskForDepartureArea choice: {foundChoice.Value.ToLower()}");
            if (foundChoice.Value.ToLower() == "terminal 1")
            {
                foodAndBevStatusState.Terminal = "T1";
                _logger.LogTrace("FoodAndBeverageDialog.AskForDepartureArea end");
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Thanks! I can show you options near your departing flight if you tell me where you're travelling to. I can also show you options before security that are available to everyone."),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Before security", Synonyms = new List<string>(){ "pre security","pre-security","public","at check in","arrivals","before check in" }  },
                        new Choice() {Value = "Within Canada", Synonyms = new List<string>(){ "in canada", "domestic","canada","" }  },
                        new Choice() { Value = "United States", Synonyms = new List<string>(){ "us","usa","america","transborder","to the us" } },
                        new Choice() { Value = "International" }
             }
                 }, cancellationToken);
            }
            else if (foundChoice.Value.ToLower() == "terminal 3")
            {
                foodAndBevStatusState.Terminal = "T3";
                _logger.LogTrace("FoodAndBeverageDialog.AskForDepartureArea end");
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Thanks! I can show you options near your departing flight if you tell me where you're travelling to. I can also show you options before security that are available to everyone."),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Before security", Synonyms = new List<string>(){ "pre security","pre-security","public","at check in","arrivals","before check in" }  },
                        new Choice() {Value = "Within Canada", Synonyms = new List<string>(){ "in canada", "domestic","canada","" }  },
                        new Choice() { Value = "United States", Synonyms = new List<string>(){ "us","usa","america","transborder","to the us" } },
                        new Choice() { Value = "International" }
             }
                 }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Visit our website for all your options at https://www.torontopearson.com/eatanddrink.", cancellationToken: cancellationToken);
                _logger.LogTrace("FoodAndBeverageDialog.AskForDepartureArea end");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                    cancellationToken: cancellationToken);
            }

        }
        /// <summary>
        /// Render for Resturant choice
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskForRestaurantStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("FoodAndBeverageDialog.AskForRestaurantStepAsync start");
                if (stepContext.Result is bool)
                {
                    if ((bool)stepContext.Result)
                    {

                        await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                        _logger.LogTrace("FoodAndBeverageDialog.AskForRestaurantStepAsync ends.");
                        return await stepContext.EndDialogAsync(cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks for chatting with me! If you have any other questions, just type **RESTART**"));
                        _logger.LogTrace("FoodAndBeverageDialog.AskForRestaurantStepAsync ends.");
                        return await stepContext.EndDialogAsync(cancellationToken);
                    }
                }
                else
                {
                    var foodAndBevStatusState = await foodStatus.GetAsync(stepContext.Context);
                    FoundChoice foundChoice = (FoundChoice)stepContext.Result;
                    _logger.LogTrace($"FoodAndBeverageDialog.AskForRestaurantStepAsync choice: {foundChoice.Value.ToLower()}");

                    var reply = stepContext.Context.Activity.CreateReply();

                    if (foundChoice.Value.ToLower() == "within canada")
                    {
                        foodAndBevStatusState.DepartureArea = "Domestic";

                    }
                    else if (foundChoice.Value.ToLower() == "before security")
                    {
                        foodAndBevStatusState.DepartureArea = "Pre-security";

                    }
                    else if (foundChoice.Value.ToLower() == "united states")
                    {
                        foodAndBevStatusState.DepartureArea = "USA";

                    }
                    else
                    {
                        foodAndBevStatusState.DepartureArea = "International";

                    }

                    //getting all the restaurants after filtering//
                    List<Restaurant> restaurants = FoodAndBeverageHelper.GetRestaurant(foodAndBevStatusState);


                    if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                    {
                        foodAndBevStatusState.FoodRetailServiceCollection = restaurants.ToList<IFoodRetailService>();
                        return await stepContext.ReplaceDialogAsync(nameof(WebChannelDisplayDialog), foodAndBevStatusState, cancellationToken);
                    }
                    else
                    {
                        foodAndBevStatusState.FoodRetailServiceCollection = restaurants.ToList<IFoodRetailService>();
                        return await stepContext.ReplaceDialogAsync(nameof(FaceBookChannelDisplayDialog), foodAndBevStatusState, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botService, ex, objDictionary, "FoodAndBeverageDialog", "AskForRestaurantStepAsync");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            
        }

    }
}
