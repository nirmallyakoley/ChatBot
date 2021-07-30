
using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.Dialogs.ByCity;
using GTAADevChatBot.Dialogs.FlightStatus.ByFlightID;
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

namespace GTAADevChatBot.Dialogs.FlightStatus
{
    public class FlightStatusDialog : CancelAndHelpDialog
    {
        // Dialog IDs
        private const string FlightNumberPrompt = "FlightNumberPrompt";
        private const string DateTimePrompt = "DateTimePrompt";
        private const string ConfirmPrompt = "ConfirmPrompt";
        public static readonly string LuisConfiguration = GtaaChatConstant.LUIS_CONFIGURATION;
        private readonly ILogger _logger;
        public IStatePropertyAccessor<FlightStatusState> flightStatus { get; }
        private BotServices _botServices = null;
        private string _strFlightorCityText = "'flight/city'";
        WaterfallStepContext _stepContext = null;


        public FlightStatusDialog(IStatePropertyAccessor<FlightStatusState> statePropertyAccessor, BotServices botService, ILoggerFactory loggerFactory) : base(nameof(FlightStatusDialog))
        {
            flightStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            _botServices = botService;

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Flight Status Dialog.");


            //AddDialog(new FlightStatusByNumberDialog(flightStatus, loggerFactory));
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    AskforDayStepAsync,
                    AskforTypeStepAsync,
                    SearchCriteriaStepAsync,
                    SummarystepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new TextPrompt(nameof(TextPrompt), CustomPromptValidatorAsync));

        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = (FlightStatusState)stepContext.Options;
            if (flightStatusState == null)
            {
                var flightStatusStateOpt = stepContext.Options as FlightStatusState;
                if (flightStatusStateOpt != null)
                {
                    await flightStatus.SetAsync(stepContext.Context, flightStatusStateOpt);
                }
                else
                {
                    await flightStatus.SetAsync(stepContext.Context, new FlightStatusState());
                }
            }

            return await stepContext.NextAsync();
        }


        private async Task<DialogTurnResult> AskforDayStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = await flightStatus.GetAsync(stepContext.Context, () => null);

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Great! When is your flight?"),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                    Choices = new List<Choice>()
                    {
                        new Choice()
                        {
                            Value = "Today", Synonyms = new List<string>()
                            {
                                "present day", "present", "this day", "Now", "moment", "here and now",
                                "Today", "this morning", "this afternoon","tonight"
                            }
                        },
                        new Choice(){Value = "Tomorrow", Synonyms = new List<string>(){ "by-and-by", "day after", "future", "hereafter", "offing", "tmr" }  }
            }
                }, cancellationToken); ;
        }

        private async Task<DialogTurnResult> AskforTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var searchFlightState = await flightStatus.GetAsync(stepContext.Context);
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value.ToLower() == "today")
            {
                searchFlightState.dateTime = Utility.GetESTCurrentTime(DateTime.UtcNow).Date;
                searchFlightState.TT = foundChoice.Value;
            }
            else
            {            

                DateTime t = Utility.GetESTCurrentTime(DateTime.UtcNow.AddDays(1));
                searchFlightState.dateTime = t.Date;
                searchFlightState.TT = foundChoice.Value;
            }

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Is the flight arriving to or departing from Toronto?"),
                    Choices = new List<Choice>() {
                        new Choice() { Value = "Arriving",
                           Synonyms = new List<string>(){ "arr", "arrive" ,"arriving","Arrival" } },
                        new Choice(){Value = "Departing" ,
                            Synonyms = new List<string>(){ "dep", "depart" , "departure", "leaving" } }
            }
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> SearchCriteriaStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var searchFlightState = await flightStatus.GetAsync(stepContext.Context);
            FoundChoice foundChoice = (FoundChoice)stepContext.Result;

            if (foundChoice.Value.ToLower() == "arriving")
                searchFlightState.AD = "ARR";
            else
                searchFlightState.AD = "DEP";

            var arrOrdepCity = searchFlightState.AD == "ARR" ? "origin" : "destination";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Good to know!  To get flight details, please type either your flight number or {arrOrdepCity} city. As an example you can type AC1234 or Vancouver"),
                //RetryPrompt = MessageFactory.Text($"I'm sorry, I can't seem to find that {_strFlightorCityText}. Let's try again. Please type either your flight number, origin/destination city."),
            }, cancellationToken);
        }


        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var searchFlightState = await flightStatus.GetAsync(stepContext.Context);
                string input = (string)stepContext.Result;             //var searchFlightState = (FlightStatusState)stepContext.Options;
                if (input.StartsWith("#"))
                {
                    stepContext.Context.Activity.Text = searchFlightState.userInput = input.Replace("#", "MU");
                }

                RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(stepContext.Context, cancellationToken);

                if (recognizerResult.Entities.Count > 0)
                {
                    searchFlightState.FlightNumber = recognizerResult.Entities["flight#"]?.FirstOrDefault()?.ToString().Replace(" ", String.Empty);
                    searchFlightState.City = recognizerResult.Entities["city"]?.FirstOrDefault()?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                }

                if (searchFlightState.FlightNumber != null)
                    return await stepContext.BeginDialogAsync(nameof(FlightStatusByNumberDialog), searchFlightState, cancellationToken);
                else if (searchFlightState.City != null)
                    return await stepContext.BeginDialogAsync(nameof(FlightStatusByCityDialog), searchFlightState, cancellationToken);
                else
                {
                    string toOrfrom = searchFlightState.AD == "ARR" ? "from" : "to";
                    var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {input}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                }

                return await stepContext.ReplaceDialogAsync(nameof(FlightStatusDialog), null, cancellationToken);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusDialog", "SummarystepAsync");
                return await stepContext.EndDialogAsync();
            }
        }
        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            try
            {
                var searchFlightState = await flightStatus.GetAsync(promptContext.Context);
                if (promptContext.Recognized.Succeeded)
                {
                    _strFlightorCityText = promptContext.Context.Activity.Text;
                    if (_strFlightorCityText.StartsWith("#"))
                    {
                        promptContext.Context.Activity.Text = _strFlightorCityText = _strFlightorCityText.Replace("#", "MU");
                    }
                    RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(promptContext.Context, cancellationToken);
                    if (recognizerResult.Entities.Count > 0)
                    {
                        //var searchFlightState = (FlightStatusState)promptContext.State.Values;
                        string number = recognizerResult.Entities["flight#"]?.FirstOrDefault()?.ToString();
                        string city = recognizerResult.Entities["city"]?.FirstOrDefault()?.ToString();
                        if (number != null || city != null)
                            return await Task.FromResult(true);
                        else
                        {
                            string toOrfrom = searchFlightState.AD == "ARR" ? "from" : "to";
                            string flightOrCity = Utility.ModifyCityFlightNoByRegex(_strFlightorCityText);
                            if (flightOrCity.Any(char.IsDigit))
                            {
                                var msg = $"I'm sorry, I can't seem to find {flightOrCity}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                                await promptContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                                return await Task.FromResult(false);
                            }
                            else
                            {
                                var msg = $"I'm sorry, I can't seem to find a flight {toOrfrom} {flightOrCity}. Let's try again.  Please type either your flight number or origin/destination city. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                                await promptContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                                return await Task.FromResult(false);
                            }

                        }
                    }
                    else
                        return await Task.FromResult(false);
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {

                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "FlightStatusDialog", "CustomPromptValidatorAsync");
                return await Task.FromResult(false);
            }
        }


    }
}

