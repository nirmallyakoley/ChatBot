using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace GTAADevChatBot.Dialogs.AirlineInformation
{
    public class AirlineInformationDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public static readonly string LuisConfigurationAirline = GtaaChatConstant.LUIS_CONFIGURATIONAirline;
        public static readonly string LuisConfiguration = GtaaChatConstant.LUIS_CONFIGURATION;
        private BotServices _botServices = null;
        public IStatePropertyAccessor<AirlineInformationBO> airlineStatus { get; }
        private string topIntent = string.Empty;
        private const string BaggageIntent = "Baggage";
        private const string ReservationIntent = "Reservations";
        private const string NegativeResponseIntent = "Negative_Response";

        #region constructor
        public AirlineInformationDialog(IStatePropertyAccessor<AirlineInformationBO> statePropertyAccessor, ILoggerFactory loggerFactory, BotServices botService) : base(nameof(AirlineInformationDialog))
        {
            airlineStatus = statePropertyAccessor ?? throw new ArgumentNullException(nameof(statePropertyAccessor));
            _botServices = botService;
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Airline Information Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync,
                AskAirlineStepAsync,
                ProvideAirlineInfoStepAsync,
                ReservationAndBaggageInfoStepAsync,
                ConfirmStepAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt), CustomPromptValidatorAsync));
            AddDialog(new TextPrompt("baggagePrompt"));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });

            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Initialize state
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var airlineStatusState = (AirlineInformationBO)stepContext.Options;
            if (airlineStatusState == null)
            {
                var airlineStatusStateOpt = stepContext.Options as AirlineInformationBO;
                if (airlineStatusStateOpt != null)
                {
                    await airlineStatus.SetAsync(stepContext.Context, airlineStatusStateOpt);
                }
                else
                {
                    await airlineStatus.SetAsync(stepContext.Context, new AirlineInformationBO());
                }
            }

            return await stepContext.NextAsync();
        }


        /// <summary>
        /// Ask for airline name or code
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskAirlineStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AirlineInformationDialog.AskAirlineStepAsync start");
            var airlineStatusState = await airlineStatus.GetAsync(stepContext.Context, () => null);

            _logger.LogTrace("AirlineInformationDialog.AskAirlineStepAsync end");
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Great! Please type the airline name or code. For example, you can type WestJet or WS."),
                RetryPrompt = MessageFactory.Text($"I'm sorry, I can't seem to find that airline. Let's try again.  Please type either the full airline name or airline code. \r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent."),
            }, cancellationToken);
        }

        /// <summary>
        /// Provide airline info and render choice for baggage and resrevations.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideAirlineInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("AirlineInformationDialog.ProvideAirlineInfoStepAsync start");

                var airlineStatusState = await airlineStatus.GetAsync(stepContext.Context);
                var airlineNameOrCode = stepContext.Result.ToString();
                _logger.LogTrace($"AirlineInformationDialog.ProvideAirlineInfoStepAsync : Choice{airlineNameOrCode}");
                //airlineStatusState.Airline = airlineNameOrCode;
                RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(stepContext.Context, cancellationToken);
                if (recognizerResult.Entities.Count > 0)
                {
                    airlineStatusState.Airline = recognizerResult.Entities["airlinename"]?.FirstOrDefault()?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                }
                if (airlineStatusState.Airline != null)
                {
                    AirlineInformationBO result = AirlineInfo.GetAirlineInfo(airlineStatusState.Airline.Replace(" ", String.Empty).ToLower());
                    if (result != null)
                    {
                        airlineStatusState.Airline = result.Airline;
                        airlineStatusState.Baggage = result.Baggage;
                        airlineStatusState.Reservation = result.Reservation;
                        airlineStatusState.Terminal = result.Terminal;

                        string terminal = result.Terminal == "T3" ? "3" : "1";
                        var opts = new PromptOptions
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = $"{result.Airline} operates from Terminal {terminal}. \r\r I can help you connect with {result.Airline} for reservations, customer service or lost, delayed or damaged baggage.",
                            },
                        };
                        _logger.LogTrace("AirlineInformationDialog.ProvideAirlineInfoStepAsync end");
                        return await stepContext.PromptAsync("baggagePrompt", opts);
                    }
                    else
                    {
                        var msg = $"I'm sorry, I can't seem to find that airline. \r\r You can type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                        return await stepContext.EndDialogAsync();

                    }
                }
                else
                {
                    var msg = $"I'm sorry, I can't seem to find that airline. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync();

                }
            }
            catch(Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "AirlineInformationDialog", "ProvideAirlineInfoStepAsync");
                return await stepContext.EndDialogAsync();
            }
        }

        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            try {
                if (promptContext.Recognized.Succeeded)
                {
                    var airlineStatusState = await airlineStatus.GetAsync(promptContext.Context);
                    RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(promptContext.Context, cancellationToken);
                    if (recognizerResult.Entities.Count > 0)
                    {
                        airlineStatusState.Airline = recognizerResult.Entities["airlinename"]?.FirstOrDefault()?.ToString();
                        if (airlineStatusState.Airline != null)
                        {
                            return await Task.FromResult(true);
                        }
                        else
                        {
                            var msg = $"I'm sorry, I can't seem to find that airline. Let's try again.  Please type either the full airline name or airline code. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                            await promptContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                            return await Task.FromResult(false);
                        }
                    }

                    else
                    {
                        var msg = $"I'm sorry, I can't seem to find that airline. Let's try again.  Please type either the full airline name or airline code. \r\r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                        await promptContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                        return await Task.FromResult(false);
                    }
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
                TelemetryLog.LogException(_botServices, ex, objDictionary, "AirlineInformationDialog", "CustomPromptValidatorAsync");
                return await Task.FromResult(false);
            }
         }
        /// <summary>
        /// Provide to baggage or reservations info based on input
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ReservationAndBaggageInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync start");

                var airlineStatusState = await airlineStatus.GetAsync(stepContext.Context);
                string userInput = (string)stepContext.Result;

                _logger.LogTrace($"AirlineInformationDialog.ReservationAndBaggageInfoStepAsync : Choice= {userInput}");

                RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfigurationAirline].RecognizeAsync(stepContext.Context, cancellationToken);
                if (recognizerResult.GetTopScoringIntent().score > 0.70)
                {
                    var topScoringIntent = recognizerResult.GetTopScoringIntent().intent.ToString();
                    topIntent = topScoringIntent;

                    if (topIntent == ReservationIntent)
                    {
                        await stepContext.Context.SendActivityAsync($"You can reach {airlineStatusState?.Airline}'s customer service at \"{airlineStatusState?.Reservation}\"", cancellationToken: cancellationToken);
                        _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync end");
                        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                        {
                            Prompt = MessageFactory.Text("Can I help you with anything else?"),
                            RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                        },
                         cancellationToken: cancellationToken);
                    }
                    else if (topIntent == BaggageIntent)
                    {
                        await stepContext.Context.SendActivityAsync($"You can reach {airlineStatusState?.Airline}'s baggage department at \"{airlineStatusState?.Baggage}\"", cancellationToken: cancellationToken);
                        _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync end");
                        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                        {
                            Prompt = MessageFactory.Text("Can I help you with anything else?"),
                            RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                        },
                         cancellationToken: cancellationToken);
                    }
                    else if (recognizerResult.Entities["Response"]?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim() == "Negative")
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks for chatting with me! If you have any other questions, just type **RESTART**"));
                        _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync ends.");
                        return await stepContext.EndDialogAsync(cancellationToken);
                    }
                    else
                    {
                        var msg = $"I'm sorry, I can't seem to find that. \r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                        _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync end");
                        return await stepContext.EndDialogAsync();
                    }
                }
                else
                {
                    var msg = $"I'm sorry, I can't seem to find that. \r You can also type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                    _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync end");
                    return await stepContext.EndDialogAsync();
                }
            } catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "AirlineInformationDialog", "ReservationAndBaggageInfoStepAsync");
                return await stepContext.EndDialogAsync();
            }
        }


        /// <summary>
        /// restart or end the conversation.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync starts.");
            if ((bool)stepContext.Result)
            {

                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync ends.");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                _logger.LogTrace("AirlineInformationDialog.ReservationAndBaggageInfoStepAsync ends.");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }
    }
}
