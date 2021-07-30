using GTAADevChatBot.DialogEnd;
using GTAADevChatBot.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GTAADevChatBot.Dialogs.CustomsAndImmigrations.ForCanada
{
    public class CanadaCustomsDialog : CancelAndHelpDialog
    {
        private readonly ILogger _logger;
        #region constructor
        public CanadaCustomsDialog(ILoggerFactory loggerFactory) : base(nameof(CanadaCustomsDialog))
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<GTAADevChatBot>();
            _logger.LogTrace("Canada Customs Dialog.");
            var waterfallSteps = new WaterfallStep[]
           {
                    ProvideInformationStepAsync,
                    ProvideCBSAInforamtionStepAsync,
                    ConfirmStepAsync
           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)) { Style = ListStyle.SuggestedAction });
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)) { Style = ListStyle.SuggestedAction });

            //The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        #endregion

        /// <summary>
        /// Rendering choice for Lost passport/General Info/Contact Info
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideInformationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CanadaCustomsDialog.ProvideInformationStepAsync starts.");

            var msg = "Okay! Passengers arriving from the US and other international origins may need to confirm their identity with Canada Border Services Agency (CBSA) and declare any goods that they are bringing into the country.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            _logger.LogTrace("CanadaCustomsDialog.ProvideInformationStepAsync ends.");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = MessageFactory.Text("Do you need any other information about CBSA?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT),
                Choices = new List<Choice>()
                {
                    new Choice(){Value="Lost passport",Synonyms=new List<string>(){"lost passport","passport lost","passport","lost" } },
                    new Choice(){Value="General Info",Synonyms=new List<string>(){"general info","general information","general","other information","Other info","Other","General","information", "travel documents", "visa" } },
                    new Choice(){Value="Contact Info",Synonyms=new List<string>(){"contact info","contact information","contact","contact details","Communication details","Communication", "phone number","Contact Number" } }
                }
            }, cancellationToken);


        }
        /// <summary>
        /// Provide CBSA information based on user choice and render if anything else is needed. 
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProvideCBSAInforamtionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CanadaCustomsDialog.ProvideCBSAInforamtionStepAsync starts.");
            FoundChoice choice = (FoundChoice)stepContext.Result;

            _logger.LogTrace($"CanadaCustomsDialog.ProvideCBSAInforamtionStepAsync : Choice Value :{choice.Value.ToLower()}");

            if (choice.Value.ToLower() == "lost passport")
            {
                await stepContext.Context.SendActivityAsync("All lost passports found at the airport, or left on an aircraft, are turned into Canada Border Services Agency (CBSA). You can reach them directly as follows:\r\rTerminal T1: 905-676-3640 \r\r Terminal T3: 905-612-5416", cancellationToken: cancellationToken);
            }
            else if (choice.Value.ToLower() == "general info")
            {
                await stepContext.Context.SendActivityAsync($"Please visit our website for more information on Canadian customs at torontopearson.com/customs.");
            }
            else
            {

                if (stepContext.Context.Activity.ChannelId.ToLower() != "facebook")
                {
                    await stepContext.Context.SendActivityAsync($"Please visit CBSA's website at https://www.cbsa-asfc.gc.ca/contact/menu-eng.html.");
                }
                else
                {

                   
                    var reply = stepContext.Context.Activity.CreateReply();
                    string str = @"{
                                    'attachment':{
                                      'type':'template',
                                      'payload':{
                                        'template_type':'button',
                                        'text':'Please visit CBSA\u0027s website at',
                                        'buttons':[
                                          {
                                            'type':'web_url',
                                            'url':'https://www.cbsa-asfc.gc.ca/contact/menu-eng.html',
                                            'title':'Click here'
                                          }
                                        ]
                                      }
                                    }
                                  }";

                    reply.ChannelData = JsonConvert.DeserializeObject(str);
                    await stepContext.Context.SendActivityAsync(reply);
                    
                }
               
              
            }
            _logger.LogTrace("CanadaCustomsDialog.ProvideCBSAInforamtionStepAsync ends.");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Can I help you with anything else?"),
                RetryPrompt = MessageFactory.Text(GtaaChatConstant.RETRY_PROMPT)
            },
                cancellationToken: cancellationToken);

        }
        /// <summary>
        ///  restart or end the conversation.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("CanadaCustomsDialog.ConfirmStepAsync starts.");
            if ((bool)stepContext.Result)
            {
                _logger.LogTrace("CanadaCustomsDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.RestartConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                _logger.LogTrace("CanadaCustomsDialog.ConfirmStepAsync ends.");
                await AllDialogEnd.EndConversationAsync(stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

    }
}
