using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Dialogs.AirportInformation.FoodAndBeverages;
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

namespace GTAADevChatBot.Dialogs.AirportInformation.ServiceAmenities
{
    public class ServiceAmenitiesDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public IStatePropertyAccessor<FoodRetailServiceBO> foodStatus { get; }
        BotServices _botServices;
        #region Constructor
        public ServiceAmenitiesDialog(IStatePropertyAccessor<FoodRetailServiceBO> statePropertyAccessor, ILoggerFactory loggerFactory, BotServices botServices) : base(nameof(ServiceAmenitiesDialog))
        {
            _botServices = botServices;
            foodStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("ServiceAmenities Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskForTerminalStepAsync,
                AskForDepartureArea,
                ProvideServiceAmenitiesInfoStepAsync
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
        /// Initialize state for Service and Amenities
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("ServiceAmenitiesDialog.InitializeStateStepAsync start");
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
            _logger.LogTrace("ServiceAmenitiesDialog.InitializeStateStepAsync end");
            return await stepContext.NextAsync();
        }

        /// <summary>
        /// Render choice for the Terminal choice
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskForTerminalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("ServiceAmenitiesDialog.AskForTerminalStepAsync start");
            var foodAndBevStatusState = await foodStatus.GetAsync(stepContext.Context, () => null);
            _logger.LogTrace("ServiceAmenitiesDialog.AskForTerminalStepAsync end");          
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("We offer a range of helpful services, including lounges, banking and currency exchange and VIP services. Choose a terminal to see options near you."),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Terminal 1", Synonyms = new List<string>(){ "terminal1","t1"} },
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
            _logger.LogTrace("ServiceAmenitiesDialog.AskForDepartureArea start");
            var foodAndBevStatusState = await foodStatus.GetAsync(stepContext.Context);
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"ServiceAmenitiesDialog.AskForDepartureArea choice: {foundChoice.Value.ToLower()}");
            if (foundChoice.Value.ToLower() == "terminal 1" || foundChoice.Value.ToLower() == "terminal 3")
            {
                if (foundChoice.Value.ToLower() == "terminal 1")
                    foodAndBevStatusState.Terminal = "T1";

                if (foundChoice.Value.ToLower() == "terminal 3")
                    foodAndBevStatusState.Terminal = "T3";

                _logger.LogTrace("ServiceAmenitiesDialog.AskForDepartureArea end");

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Thanks! I can show you options near your departing flight if you tell me where you're travelling to. I can also show you options before security that are available to everyone."),
                     RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                     Choices = new List<Choice>() {
                        new Choice() { Value = "Before security", Synonyms = new List<string>(){ "After Arrival", "Before security", "before check in", "public", "at check in ", "Arrivals" }  },
                         new Choice() {Value = "Within Canada", Synonyms = new List<string>(){ "in canada","domestic","canada" }  },
                        new Choice() { Value = "United States", Synonyms = new List<string>(){ "us","usa","transborder","to the US","america" } },
                        new Choice() { Value = "International" , Synonyms = new List<string>(){ "intl", "overseas" } },
                 }
                 }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Visit our website for all your options at https://www.torontopearson.com/services.", cancellationToken: cancellationToken);
                _logger.LogTrace("ServiceAmenitiesDialog.AskForDepartureArea end");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                    cancellationToken: cancellationToken);
            }

        }
        /// <summary>
        /// Provide Service Amenities Information based on user selection
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideServiceAmenitiesInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("ServiceAmenitiesDialog.AskForRestaurantStepAsync start");
                if (stepContext.Result is bool)
                {
                    if ((bool)stepContext.Result)
                    {
                        _logger.LogTrace("ServiceAmenitiesDialog.ProvideServiceAmenitiesInfoStepAsync ends.");
                        await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                        return await stepContext.EndDialogAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogTrace("ServiceAmenitiesDialog.ProvideServiceAmenitiesInfoStepAsync ends.");
                        await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                        return await stepContext.EndDialogAsync(cancellationToken);
                    }
                }
                else
                {
                    var foodAndBevStatusState = await foodStatus.GetAsync(stepContext.Context);
                    FoundChoice foundChoice = (FoundChoice)stepContext.Result;
                    _logger.LogTrace($"ServiceAmenitiesDialog.ProvideServiceAmenitiesInfoStepAsync choice: {foundChoice.Value.ToLower()}");

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
                    List<ServiceAmenities> serAmenities = ServiceAmenitiesHelper.GetServiceAmenities(foodAndBevStatusState);
                    if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                    {
                        foodAndBevStatusState.FoodRetailServiceCollection = serAmenities.ToList<IFoodRetailService>();
                        return await stepContext.ReplaceDialogAsync(nameof(WebChannelDisplayDialog), foodAndBevStatusState, cancellationToken);
                    }
                    else
                    {
                        foodAndBevStatusState.Skip = 0;
                        foodAndBevStatusState.FoodRetailServiceCollection = serAmenities.ToList<IFoodRetailService>();
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
                TelemetryLog.LogException(_botServices, ex, objDictionary, "ServiceAmenitiesDialog", "ProvideServiceAmenitiesInfoStepAsync");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }
    }
}
