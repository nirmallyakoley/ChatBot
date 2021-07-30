using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTAADevChatBot.Dialogs.AirportInformation.TaxiAndLimosOrRideShare
{
    public class TaxiLimosRideShareDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        public TaxiLimosRideShareDialog(ILoggerFactory loggerFactory) : base(nameof(TaxiLimosRideShareDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("TaxiLimosRideShareDialog Dialog.");

            var waterfallSteps = new WaterfallStep[]
            {
                ProvideTaxiLimosDetailStepAsync,
                ProvideInfoBasedOnCarServiceStepAsync,
                ConfirmStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });
            InitialDialogId = nameof(WaterfallDialog);
        }
        /// <summary>
        /// Render user for car service choices
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideTaxiLimosDetailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("TaxiLimosRideShareDialog.ProvideTaxiLimosDetailStepAsync starts.");

            _logger.LogTrace("TaxiLimosRideShareDialog.ProvideTaxiLimosDetailStepAsync end.");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = MessageFactory.Text("We've got various car services available for our passengers to choose from. Let us know which service you would like information on. "),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                Choices = new List<Choice>()
                {
                    new Choice(){Value="Taxi & Limousines",Synonyms=new List<string>(){ "Airport Taxi and Limousines","airport","taxi", "Limousines","airport taxi","limo", "Limousine" } },
                    new Choice(){Value="Ride Share", Synonyms=new List<string>(){"rideshare","ride","share","share ride","ride share","uber","lyft" } },
                    new Choice(){Value="Pre-arranged", Synonyms=new List<string>(){ "Pre arranged" } }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Provide info based on user chpice
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideInfoBasedOnCarServiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("TaxiLimosRideShareDialog.ProvideInfoBasedOnCarServiceStepAsync starts.");
            FoundChoice choice = (FoundChoice)stepContext.Result;
            _logger.LogTrace($"TaxiLimosRideShareDialog.ProvideInfoBasedOnCarServiceStepAsync : Choice {choice.Value.ToLower()}.");
            if (choice.Value.ToLower() == "taxi & limousines")
            {
                await stepContext.Context.SendActivityAsync("Our authorized taxis and limousines are available 24/7 without the need to pre-book for service. We offer flat rate pricing and our drivers accept cash as well as major credit cards. To check the rate to your destination please click here:  [https://www.torontopearson.com/en/transportation/taxis-and-limos](https://www.torontopearson.com/en/transportation/taxis-and-limos)");
                _logger.LogTrace("TaxiLimosRideShareDialog.ProvideInfoBasedOnCarServiceStepAsync end.");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else? "),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                    cancellationToken: cancellationToken);
            }

            else if (choice.Value.ToLower() == "ride share")
            {
                await stepContext.Context.SendActivityAsync("Uber and Lyft are able to pick up and drop off passengers at Toronto Pearson. For ride share pick up locations please click here:  [https://www.torontopearson.com/en/transportation/ride-sharing](https://www.torontopearson.com/en/transportation/ride-sharing)");
                _logger.LogTrace("TaxiLimosRideShareDialog.ProvideInfoBasedOnCarServiceStepAsync end.");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else? "),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                    cancellationToken: cancellationToken);
            }

            else
            {
                await stepContext.Context.SendActivityAsync("If you prefer to plan ahead and pre-book your taxi or limo for your arrival you may! We offer passengers the option to book the operator of their choice to provide pre-arranged service from Toronto Pearson. Simply contact the company directly and when you arrive at the airport go to the designated area and let our dispatcher know you are waiting for your ride.");
                _logger.LogTrace("TaxiLimosRideShareDialog.ProvideInfoBasedOnCarServiceStepAsync end.");
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Can I help you with anything else? "),
                    RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
                },
                    cancellationToken: cancellationToken);
            }

        }
        /// <summary>
        ///restart or end the conversation.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("TaxiLimosRideShareDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("TaxiLimosRideShareDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("TaxiLimosRideShareDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }
    }
}
