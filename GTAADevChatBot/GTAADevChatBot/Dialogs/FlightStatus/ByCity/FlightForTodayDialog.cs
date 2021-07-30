using GTAADevChatBot.Dialogs.FlightStatus;
using GTAADevChatBot.Dialogs.FlightStatus.ByCity;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.ByCity
{
    public class FlightForTodayDialog : CancelAndHelpDialog
    {
        public FlightForTodayDialog() : base(nameof(FlightForTodayDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                    SummarystepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            //The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> SummarystepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var flightStatusState = (FlightStatusState)stepContext.Options;

            List<FlightByCity> flights = null;

            var esttimenow = Utility.GetESTCurrentTime();
            var reply = stepContext.Context.Activity.CreateReply();
            //flights = flightStatusState.FlightsList.Where(a => a.al.ToLower() == flightStatusState.al.ToLower() && Helper.GetESTCurrentTime(a.schTime) >= esttimenow.AddHours(-3) && Helper.GetESTCurrentTime(a.schTime) <= esttimenow.AddHours(3)).ToList();
            flights = (from item in flightStatusState.FlightsList where (item.al.ToLower() == flightStatusState.al.ToLower() || (from item1 in item.ids where item1.alName.ToLower() == flightStatusState.al.ToLower() select item1).ToList().Count > 0) && Utility.GetESTCurrentTime(item.schTime) >= esttimenow.AddHours(-3) && Utility.GetESTCurrentTime(item.schTime) <= esttimenow.AddHours(3) select item).ToList();

            if (flights.Count() > 0)
            {
                var resultString = flights.Count() > 1 ? "results" : "result";
                if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                {
                    reply.Text = $"All flights +/-3 hours from current time \r\r Here you go : {flights.Count()} { resultString} found";
                    foreach (var item in flights.Take(25))
                    {
                        reply.Attachments.Add(FlightCardUtility.CreateSearchFlightAdaptiveCardAttachment(item, flightStatusState));
                    }

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    await stepContext.Context.SendActivityAsync(reply);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"All flights +/-3 hours from current time \r\r Here you go : {flights.Count()} { resultString} found");
                    foreach(var item in flights)
                    {
                        reply.Attachments.Add(FlightCardUtility.CreateFacebookSearchFlightAdaptiveCardAttachment(item, flightStatusState));
                        await stepContext.Context.SendActivityAsync(reply);
                        reply.Attachments.Clear();
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No Flights are available from {esttimenow.AddHours(-3)} to {esttimenow.AddHours(3)} Hours."));
            }

            return await stepContext.EndDialogAsync(null, cancellationToken: cancellationToken);
        }

        
    }
}
