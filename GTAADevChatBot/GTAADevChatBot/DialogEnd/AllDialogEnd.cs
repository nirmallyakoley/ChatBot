using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.DialogEnd
{
    public class AllDialogEnd
    {
        public static async Task RestartConversationAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply("Great! How can I help?");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Markdown;

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "Flight & airline info", Type=ActionTypes.ImBack, Value="Flight & airline info" },
                    new CardAction(){ Title = "Airport information", Type=ActionTypes.ImBack, Value="Airport information" },
                    new CardAction(){ Title = "Lost & found", Type=ActionTypes.ImBack, Value="Lost & found" },
                    new CardAction(){ Title = "Customs & immigration", Type=ActionTypes.ImBack, Value="Customs & immigration" },
                    new CardAction(){ Title = "Accessibility​", Type=ActionTypes.ImBack, Value="Accessibility​" }
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        public static async Task EndConversationAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Thanks for chatting with me!", cancellationToken: cancellationToken);
            var reply = turnContext.Activity.CreateReply($"If you have any other questions, just type **RESTART**. \r Would you like to give your feedback?");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Markdown;
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value="yes-positive-feedback" },
                    new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value="no-negative-feedback" }
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
