using GTAADevChatBot.CustomLogging;
using GTAADevChatBot.Dialogs.AirlineInformation;
using GTAADevChatBot.Dialogs.YesNoDialog;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.LostAndFound.AirCraft
{
    public class OnAircraftDialog : CancelAndHelpDialog
    {

        private readonly ILogger _logger;
        private BotServices _botServices = null;
        public static readonly string LuisConfiguration = GtaaChatConstant.LUIS_CONFIGURATION;
        // public IStatePropertyAccessor<AirlineInformationBO> airlineStatus { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        public OnAircraftDialog(BotServices botServices, ILoggerFactory loggerFactory) : base(nameof(OnAircraftDialog))
        {
            _botServices = botServices;
            if (loggerFactory == null)

            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("onAirCraft Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                LostandFoundonAircroft,
                LostAirlineDialogStepAsync

            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new YesAndNoStatusDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt), CustomPromptValidatorAsync));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> LostandFoundonAircroft(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("onAirCraft Dialog.LostandFoundonAircroft Begin");
            var msg = "Anything left on an aircraft (including lost luggage) would remain with your airline's lost and found office.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            _logger.LogTrace("onAirCraft Dialog.LostandFoundonAircroft End");
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Let me help you to get in touch with them. Which airline did you travel with? You can type the name or airline code (i.e. Westjet or WS)"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> LostAirlineDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("OnAircraft.LostAilineDialogStepAsync Begin");
                // var airlineStatusState = await airlineStatus.GetAsync(stepContext.Context);
                var airlineNameOrCode = "";

                _logger.LogTrace($"OnAircraft.LostAilineDialogStepAsync : Choice{airlineNameOrCode}");

                RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(stepContext.Context, cancellationToken);
                if (recognizerResult.Entities.Count > 0)
                {
                    airlineNameOrCode = recognizerResult.Entities["airlinename"]?.FirstOrDefault()?.ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                }

                if (airlineNameOrCode != null)
                {
                    AirlineInformationBO result = AirlineInfo.GetAirlineInfo(airlineNameOrCode.Replace(" ", String.Empty).ToLower());
                    if (result != null)
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here's a link to {result.Airline}'s baggage and lost & found information.\r {result.Baggage}"));
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
                    var msg = $"I'm sorry, I can't seem to find that airline. \r\r You can type **RESTART** to go back to the first menu or **SUPPORT** to speak to a live agent.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync();

                }

                _logger.LogTrace("OnAircraft.LostAilineDialogStepAsync End");
                return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "OnAircraftDialog", "LostAilineDialogStepAsync");
                return await stepContext.EndDialogAsync();
            }
        }

        public async Task<bool> CustomPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            try
            {
                if (promptContext.Recognized.Succeeded)
                {
                    string airlineNameOrCode = null;
                    //var airlineStatusState = await airlineStatus.GetAsync(promptContext.Context);
                    //var airlineNameOrCode = promptContext.Context.Activity.Text.ToString().Replace(" ", String.Empty).ToLower();
                    RecognizerResult recognizerResult = await _botServices.LuisServices[LuisConfiguration].RecognizeAsync(promptContext.Context, cancellationToken);
                    if (recognizerResult.Entities.Count > 0)
                    {
                        airlineNameOrCode = recognizerResult.Entities["airlinename"]?.FirstOrDefault()?.ToString();
                        if (airlineNameOrCode != null)
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
                TelemetryLog.LogException(_botServices, ex, objDictionary, "OnAircraftDialog", "CustomPromptValidatorAsync");
                return await Task.FromResult(false);
            }
        }


       }
}













