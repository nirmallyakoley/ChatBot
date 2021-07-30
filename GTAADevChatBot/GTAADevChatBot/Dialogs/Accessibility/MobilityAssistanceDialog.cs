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


namespace GTAADevChatBot.Dialogs.Accessibility
{
    public class MobilityAssistanceDialog : CancelAndHelpDialog

    {
        private readonly ILogger _logger;
        private BotServices _botServices = null;
        public static readonly string LuisConfiguration = GtaaChatConstant.LUIS_CONFIGURATION;
        public IStatePropertyAccessor<AirlineInformationBO> airlineStatus { get; }

        public MobilityAssistanceDialog(BotServices botServices, ILoggerFactory loggerFactory) : base(nameof(MobilityAssistanceDialog))
        {
            _botServices = botServices;
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("MobilityAssistanceDialog.");

            var waterfallSteps = new WaterfallStep[]
           {

                MobilityAssitanceDialogStepAsync,
                AirlineDialogStepAsync

           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt), CustomPromptValidatorAsync));
            AddDialog(new YesAndNoStatusDialog());
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> MobilityAssitanceDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Mobility Assistance.MobilityAssitanceDialogStepAsync Begin");

            var msg = "Mobility and special assistance is provided by your airline at Pearson Airport. Arrangements should be made directly with your airline at least 48 hours in advance of travel. \r\r For passengers travelling within the next 48 hours please type support.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            _logger.LogTrace("Mobility Assistance.MobilityAssitanceDialogStepAsync End");

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("If your flight is not within the next 48 hours let me know which airline you are travelling with. You may type the airline name or airline code. For example, you may type Westjet or WS."),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> AirlineDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("Mobilityassistance.AirlineDialogStepAsync Begin");
                var airlineNameOrCode = "";
                _logger.LogTrace($"Mobilityassistance.AirlineDialogStepAsync : Choice{airlineNameOrCode}");


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
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thank you. You can reach {result.Airline} at {result.Reservation} or  {result.ContactNumber} "));
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

                _logger.LogTrace("Mobilityassistance.AirlineDialogStepAsyncc End");

                return await stepContext.ReplaceDialogAsync(nameof(YesAndNoStatusDialog));
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(_botServices, ex, objDictionary, "MobilityAssistanceDialog", "AirlineDialogStepAsyncc");
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
                TelemetryLog.LogException(_botServices, ex, objDictionary, "MobilityAssistanceDialog", "CustomPromptValidatorAsync");
                return await Task.FromResult(false);
            }

        }
    }
}
