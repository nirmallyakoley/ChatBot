using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Welcome
{
    public class WelcomeMessage
    {
        public static async Task SendWelcomeMsgForStartAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply("Hello! I'm your virtual assistant at Toronto Pearson. Here's how I can help you. \r\r If I can't answer your question, or if you need further assistance, you can always type SUPPORT to reach a live agent. To start over, please type RESTART");
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
                }
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
            //HeroCard heroCard = new HeroCard()
            //{
            //    Buttons = new List<CardAction>() {
            //                    new CardAction() { DisplayText = "Flight & Airline Information", Value = "Flight & Airline Information", Text = "Flight & Airline Information", Title = "Flight & Airline Information", Type = "imBack" },
            //                    new CardAction() { DisplayText = "Airport Information​", Value = "Airport Information", Text = "Airport Information", Title = "Airport Information​", Type = "imBack" },
            //                    new CardAction() { DisplayText="Lost and Found", Value="Lost and Found",Text="Lost and Found",Title="Lost and Found​​",Type="imBack"},
            //                    new CardAction() { DisplayText="Customs & Immigrations​", Value="Customs & Immigrations",Text="Customs & Immigrations",Title="Customs & Immigrations​​​",Type="imBack"},
            //                    new CardAction() { DisplayText="Accessibility​", Value="Accessibility",Text="Accessibility",Title="Accessibility​​​",Type="imBack"}
            //                 },
            //    Text = "Hello!  I'm your virtual assistant at Toronto Pearson.  Please let me know how I can help? Here are some things that I can easily assist with.​ \r\r If I get stuck, you can always type **SUPPORT** to reach a live agent. If you want to start over please type **RESTART​**"
            //};
            //var reply = turnContext.Activity.CreateReply();
            //reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            //reply.Attachments = new List<Attachment>() { heroCard.ToAttachment() };
            //await turnContext.SendActivityAsync(reply);
        }
    }
}
