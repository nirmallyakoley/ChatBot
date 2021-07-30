
using GTAADevChatBot.Dialogs.FlightStatus;
using GTAADevChatBot.Dialogs.FlightStatus.ByCity;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text.DateTime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Constants = Microsoft.Recognizers.Text.DataTypes.TimexExpression.Constants;

namespace GTAADevChatBot.Dialogs.ByCity
{
    public class FlightForTomorrowDialog : CancelAndHelpDialog
    {
        public FlightForTomorrowDialog() : base(nameof(FlightForTomorrowDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                    AskForAMorPMstepAsync,
                    SummarystepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt), DateTimePromptValidator));

            //The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForAMorPMstepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = (FlightStatusState)stepContext.Options;

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text($"There are a lot of flights! Please enter your flight time or a time range. For example, 6am to 12pm or 0600 to 1200.​"),
                    RetryPrompt = MessageFactory.Text($"Entered input did not recognized please enter in the below format in 24 hours Format (00:00 to 23:59) or 12 hours Format (12 AM to 11:59 PM)")
                });
        }

        private async Task<bool> DateTimePromptValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                try
                {
                    string text = promptContext.Context.Activity.Text;
                    string date = Utility.ConvertToTime(text);
                    var results = DateTimeRecognizer.RecognizeDateTime(date, Culture.English);
                    var resolutions = results[0].Resolution["values"] as List<Dictionary<string, string>>;

                    var result = resolutions[0];
                    var timex = result["timex"];
                    var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Time);

                    TimeSpan StartTime = new TimeSpan();
                    TimeSpan EndTime = new TimeSpan();

                    if (isDefinite)
                    {
                        if (result["start"] != null && result["end"] != null)
                        {
                            StartTime = TimeSpan.Parse(result["start"]);
                            EndTime = TimeSpan.Parse(result["end"]);
                        }
                        else
                        if (result["start"] == null && result["end"] == null)
                            StartTime = EndTime = TimeSpan.Parse(result["value"]);
                        else
                        if (result["start"] != null && result["end"] == null)
                        {
                            StartTime = TimeSpan.Parse(result["start"]);
                            EndTime = TimeSpan.Parse(result["start"]);
                        }
                        else
                        {
                            StartTime = TimeSpan.Parse(result["end"]);
                            EndTime = TimeSpan.Parse(result["end"]);
                        }

                        if (EndTime < StartTime)
                        {
                            await promptContext.Context.SendActivityAsync($"Entered time is incorrect : End Time {EndTime} should be greater than start time {StartTime}. Please re-enter the time / time range.");
                            return await Task.FromResult(false);
                        }
                    }

                    return await Task.FromResult(isDefinite);
                }
                catch (Exception)
                {
                    await promptContext.Context.SendActivityAsync($"Entered input did not recognized please enter in the below format in 24 hours Format (00:00 to 23:59) or 12 hours Format (12 AM to 11:59 PM)");

                    return await Task.FromResult(false);
                }
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = (FlightStatusState)stepContext.Options;
            string date = Utility.ConvertToTime(stepContext.Result.ToString());
            //var results = DateTimeRecognizer.RecognizeDateTime(stepContext.Result.ToString(), Culture.English);
            var results = DateTimeRecognizer.RecognizeDateTime(date, Culture.English);
            var resolutions = results[0].Resolution["values"] as List<Dictionary<string, string>>;

            var result = resolutions[0];
            var timex = result["timex"];
            var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Time);

            if (isDefinite)
            {
                if (result["start"] != null && result["end"] != null)
                {
                    flightStatusState.StartTime = TimeSpan.Parse(result["start"]);
                    flightStatusState.EndTime = TimeSpan.Parse(result["end"]);
                }
                else
                if (result["start"] == null && result["end"] == null)
                    flightStatusState.StartTime = flightStatusState.EndTime = TimeSpan.Parse(result["value"]);
                else
                if (result["start"] != null && result["end"] == null)
                {
                    flightStatusState.StartTime = TimeSpan.Parse(result["start"]);
                    flightStatusState.EndTime = TimeSpan.Parse(result["start"]);
                }
                else
                {
                    flightStatusState.StartTime = TimeSpan.Parse(result["end"]);
                    flightStatusState.EndTime = TimeSpan.Parse(result["end"]);
                }
            }

            var reply = stepContext.Context.Activity.CreateReply();

            List<FlightByCity> flights = null; //flights = flightStatusState.FlightsList.Where(a => Helper.GetESTCurrentTime(a.schTime).TimeOfDay >= flightStatusState.StartTime && Helper.GetESTCurrentTime(a.schTime).TimeOfDay <= flightStatusState.EndTime && a.al.ToLower() == flightStatusState.al.ToLower()).ToList();
            flights = (from item in flightStatusState.FlightsList where (item.al.ToLower() == flightStatusState.al.ToLower() || (from item1 in item.ids where item1.alName.ToLower() == flightStatusState.al.ToLower() select item1).ToList().Count > 0) && Utility.GetESTCurrentTime(item.schTime).TimeOfDay >= flightStatusState.StartTime && Utility.GetESTCurrentTime(item.schTime).TimeOfDay <= flightStatusState.EndTime select item).ToList();

            if (flights.Count() > 0 && flights != null)
            {
                var resultString = flights.Count() > 1 ? "results" : "result";
                if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                {
                    reply.Text = $"Here you go : {flights.Count()} { resultString} found";

                    foreach (var item in flights.Take(25))
                    {
                        reply.Attachments.Add(FlightCardUtility.CreateSearchFlightAdaptiveCardAttachment(item, flightStatusState));
                    }

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    await stepContext.Context.SendActivityAsync(reply);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Here you go : {flights.Count()} { resultString} found.");
                    foreach (var item in flights)
                    {
                        reply.Attachments.Add(FlightCardUtility.CreateFacebookSearchFlightAdaptiveCardAttachment(item, flightStatusState));
                        await stepContext.Context.SendActivityAsync(reply);
                        reply.Attachments.Clear();
                    }

                }
            }
            else
            {
                if (flightStatusState.StartTime == flightStatusState.EndTime)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No Flights are available at  {flightStatusState.StartTime}  Hours."));
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No Flights are available from {flightStatusState.StartTime} to {flightStatusState.EndTime} Hours."));

                }
            }
            return await stepContext.EndDialogAsync(null);
        }        
    }
}
