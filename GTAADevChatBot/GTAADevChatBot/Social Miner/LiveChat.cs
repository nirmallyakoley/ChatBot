using GTAADevChatBot.CustomLogging;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace GTAADevChatBot.Social_Miner
{
    public class LiveChat
    {
        private string Cookiee { get; set; }
        private Timer timer;
        ITurnContext _turnContext;
        IStatePropertyAccessor<CounterState> _socilaMinerStateAccessor;
        UserState _userState;
        ConversationState _conversationState;
        CSMState _CSMState;
        BotServices _botServices;
        public string SocialMinerUrl = null;
        public string transcriptContainerName = null;
        int JoinState = 0;
        bool boolJoinMessage = false;
        bool boolLeftMessage = false;

        public async Task CallCustomerCareAsync(IStatePropertyAccessor<CounterState> _socilaMinerStateAccessor, ITurnContext turnContext, UserState _userState, ConversationState _conversationState, CSMState cSMState, BotServices botServices, string SocialMinerUrl,string _transcriptContainerName)
        {
            transcriptContainerName = _transcriptContainerName;
            this._socilaMinerStateAccessor = _socilaMinerStateAccessor;
            this._userState = _userState;
            this._conversationState = _conversationState;
            this._CSMState = cSMState;
            this._botServices = botServices;
            this.SocialMinerUrl = SocialMinerUrl;
            _turnContext = turnContext;
            try
            {
                CounterState objCounterState = await _socilaMinerStateAccessor.GetAsync(turnContext, () => new CounterState());

                if (objCounterState != null)
                {

                    if (!objCounterState.RequestedtoAgent)
                    {
                        try
                        {
                            IRestResponse restResponse = new SocialMiner().CreateSession(turnContext, _CSMState, SocialMinerUrl);
                            if (restResponse.StatusCode == System.Net.HttpStatusCode.Created)
                            {
                                objCounterState.CookieID = restResponse.Cookies[0].Value;
                                objCounterState.LastChatID = 0;
                                Cookiee = restResponse.Cookies[0].Value;
                                objCounterState.RequestedtoAgent = true;
                                await turnContext.SendActivityAsync(SocialMinerVerbiages.REQUEST_TO_AGENT);
                                await _socilaMinerStateAccessor.SetAsync(turnContext, objCounterState);
                                await _conversationState.SaveChangesAsync(turnContext);
                                await _userState.SaveChangesAsync(turnContext);
                                timer = new Timer(new TimerCallback(timerEvent));
                                timer.Change(2000, Timeout.Infinite);
                            }
                            else
                            {
                                await turnContext.SendActivityAsync(SocialMinerVerbiages.AGENT_NOTAVAILABLE);
                                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                                objDictionary.Add("Social Miner ResponseCode", restResponse.StatusCode.ToString());
                                objDictionary.Add("SocialMinerURL", SocialMinerUrl);
                                objDictionary.Add("Social Miner ResponseStatus", restResponse.ResponseStatus.ToString());
                                botServices.TelemetryClient.TrackTrace("Social Miner Request not established successfully", objDictionary);
                            }

                        }
                        catch (Exception ex)
                        {
                            await turnContext.SendActivityAsync(SocialMinerVerbiages.AGENT_NOTAVAILABLE);
                            Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                            objDictionary.Add("Message", ex.Message);
                            objDictionary.Add("InternalException", ex.InnerException.ToString());
                            objDictionary.Add("StackTrace", ex.StackTrace);
                            TelemetryLog.LogException(botServices, ex, objDictionary, "LiveChat", "CallCustomerCareAsync");
                        }
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(SocialMinerVerbiages.REQUEST_TO_AGENT_ALREADY_SENT);
                    }

                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(botServices, ex, objDictionary, "LiveChat", "CallCustomerCareAsync");
            }
         }

        private void timerEvent(object state)
        {
            try
            {
                timer.Dispose();
                CounterState objCounterState = _socilaMinerStateAccessor.GetAsync(_turnContext).Result;
                chatEvents chat = null;
                IRestResponse restResponse = null;
                while (objCounterState.RequestedtoAgent)
                {

                    try
                    {
                        Thread.Sleep(2000);
                        objCounterState = _socilaMinerStateAccessor.GetAsync(_turnContext, () => new CounterState()).Result;
                        restResponse = new SocialMiner().GetResponsefromAgent(objCounterState.LastChatID, Cookiee, SocialMinerUrl);

                        //retry again if response is null or empty//
                        if (string.IsNullOrEmpty(restResponse.Content?.Trim()))
                        {
                            for (int i = 1; i < 3; i++)
                            {
                                Thread.Sleep(2000 * i);
                                restResponse = new SocialMiner().GetResponsefromAgent(objCounterState.LastChatID, Cookiee, SocialMinerUrl);
                                if (string.IsNullOrEmpty(restResponse.Content?.Trim()))
                                {
                                    break;
                                }
                            }
                        }
                        XmlSerializer serializer = new XmlSerializer(typeof(chatEvents));
                        using (TextReader reader = new StringReader(restResponse.Content))
                        {
                            chat = (chatEvents)serializer.Deserialize(reader);
                            reader.Close();
                        }
                        if (chat.Items != null)
                        {

                            foreach (var item in chat.Items)
                            {
                                int id = PrintMessageFromAgent(_turnContext, item, _botServices, transcriptContainerName).Result;
                                if (objCounterState.LastChatID < id)
                                    objCounterState.LastChatID = id;
                            }
                            _socilaMinerStateAccessor.SetAsync(_turnContext, objCounterState);
                            _userState.SaveChangesAsync(_turnContext);
                            _conversationState.SaveChangesAsync(_turnContext);
                        }
                    }
                    catch (System.Net.WebException ex)
                    {
                        Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                        objDictionary.Add("Message", ex.Message);
                        objDictionary.Add("InternalException", ex.InnerException.ToString());
                        objDictionary.Add("StackTrace", ex.StackTrace);
                        objDictionary.Add("CiscoMinerResponse", string.IsNullOrEmpty(restResponse.Content?.Trim()) ? "N/A" : restResponse.Content.Trim());
                        TelemetryLog.LogException(_botServices, ex, objDictionary, "LiveChat", "timerEvent");

                        _turnContext.SendActivityAsync(SocialMinerVerbiages.CONNECTION_ENDS);
                        objCounterState.RequestAccepted = false;
                        objCounterState.RequestedtoAgent = false;
                        objCounterState.CookieID = string.Empty;
                        objCounterState.LastChatID = 0;
                        _socilaMinerStateAccessor.SetAsync(_turnContext, objCounterState);
                        _userState.SaveChangesAsync(_turnContext);
                        _conversationState.SaveChangesAsync(_turnContext);
                    }
                    catch (System.Exception ex)
                    {
                        Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                        objDictionary.Add("Message", ex.Message);
                        objDictionary.Add("InternalException", ex.InnerException.ToString());
                        objDictionary.Add("StackTrace", ex.StackTrace);
                        objDictionary.Add("CiscoMinerResponse", string.IsNullOrEmpty(restResponse.Content?.Trim()) ? "N/A" : restResponse.Content.Trim());
                        TelemetryLog.LogException(_botServices, ex, objDictionary, "LiveChat", "timerEvent");

                        _turnContext.SendActivityAsync(SocialMinerVerbiages.CONNECTION_ENDS);
                        objCounterState.RequestAccepted = false;
                        objCounterState.RequestedtoAgent = false;
                        objCounterState.CookieID = string.Empty;
                        objCounterState.LastChatID = 0;
                        _socilaMinerStateAccessor.SetAsync(_turnContext, objCounterState);
                        _userState.SaveChangesAsync(_turnContext);
                        _conversationState.SaveChangesAsync(_turnContext);
                    }
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);               
                TelemetryLog.LogException(_botServices, ex, objDictionary, "LiveChat", "timerEvent");
            }
         }


        public async Task<int> PrintMessageFromAgent(ITurnContext turnContext, object obj, BotServices botServices,string transcriptContainerName)
{
            int id = 0;
            try
            {
                if (obj.GetType().Name == typeof(chatEventsMessageEvent).Name)
                {
                    chatEventsMessageEvent chatEventsMessage = (chatEventsMessageEvent)obj;
                    await turnContext.SendActivityAsync(HttpUtility.UrlDecode(chatEventsMessage.body.Replace("+", " ").ToString()));
                    id = chatEventsMessage.id;
                }
                else if (obj.GetType().Name == typeof(chatEventsPresenceEvent).Name)
                {
                    chatEventsPresenceEvent chatEventsPresence = (chatEventsPresenceEvent)obj;

                    id = chatEventsPresence.id;

                    if (chatEventsPresence.status == "joined")
                    {
                        JoinState++;
                        _CSMState.didAgentAcceptRequest = true;
                        TelemetryLog.LogCustomEvents(_botServices, _CSMState);
                        var objCounterState = await _socilaMinerStateAccessor.GetAsync(_turnContext);                       
                        objCounterState.RequestAccepted = true;
                        if (!boolJoinMessage)//show message once//
                        {
                            await turnContext.SendActivityAsync(SocialMinerVerbiages.CONNECTION_ESTABLISHED);
                            boolJoinMessage = true;
                        }
                        string chatTranscriptForLiveAgent = ChatTranscript.ReadChatTranscript(turnContext.Activity.ChannelId, turnContext.Activity.Conversation.Id, botServices,transcriptContainerName);

                        IRestResponse res = new SocialMiner().PutResponseToAgent(Cookiee, chatTranscriptForLiveAgent, SocialMinerUrl);
                      
                        await _socilaMinerStateAccessor.SetAsync(turnContext, objCounterState);
                        await _userState.SaveChangesAsync(turnContext);
                        await _conversationState.SaveChangesAsync(turnContext);
                    }
                    else if (chatEventsPresence.status == "left" || chatEventsPresence.status == "multiPartyChatEnded")
                    {
                        var objCounterState = await _socilaMinerStateAccessor.GetAsync(turnContext);
                        JoinState--;
                        if (JoinState == 0 || chatEventsPresence.status == "multiPartyChatEnded")
                        {
                            if (!boolLeftMessage)//show the message one time//
                            {
                                await turnContext.SendActivityAsync(SocialMinerVerbiages.CONNECTION_ENDS);
                                boolLeftMessage = true;
                            }
                            objCounterState.RequestAccepted = false;
                            objCounterState.RequestedtoAgent = false;
                            objCounterState.CookieID = string.Empty;
                            objCounterState.LastChatID = 0;
                        }
                        await _socilaMinerStateAccessor.SetAsync(turnContext, objCounterState);
                        await _userState.SaveChangesAsync(turnContext);
                        await _conversationState.SaveChangesAsync(turnContext);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(chatEventsPresence.status);                       
                    }
                }
                else if (obj.GetType().Name == typeof(chatEventsStatusEvent).Name)
                {
                    chatEventsStatusEvent chatEventsPresence = (chatEventsStatusEvent)obj;

                    id = chatEventsPresence.id;

                    //if (chatEventsPresence.status == "chat_ok")
                    //{
                    //    await turnContext.SendActivityAsync("Connect established with SCM");
                    //}
                }
                else
                {

                    chatEventsTypingEvent chatEventsTyping = (chatEventsTypingEvent)obj;
                    id = chatEventsTyping.id;

                }
            }
            catch (Exception ex) {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);              
                TelemetryLog.LogException(botServices, ex, objDictionary, "LiveChat", "PrintMessageFromAgent");
            }
                return id;
        }

        public async Task SendChatToAgentAsync(IStatePropertyAccessor<CounterState> _socilaMinerStateAccessor, ITurnContext turnContext, string text, BotServices botServices, string SocialMinerUrl)
        {
            try
            {
                var objCounterState = await _socilaMinerStateAccessor.GetAsync(turnContext, () => new CounterState());
                new SocialMiner().PutResponseToAgent(objCounterState.CookieID, text, SocialMinerUrl);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(botServices, ex, objDictionary, "LiveChat", "SendChatToAgentAsync");
            }
        }
        public async Task DisconnectAgentAsync(IStatePropertyAccessor<CounterState> _socilaMinerStateAccessor, ITurnContext turnContext, UserState _userState, ConversationState _conversationState, BotServices botServices,string SocialMinerUrl)
        {
            try
            {       
                var objCounterState = await _socilaMinerStateAccessor.GetAsync(turnContext, () => new CounterState());
                if (!(objCounterState.RequestAccepted) && objCounterState.RequestedtoAgent == true)
                {
                    await _socilaMinerStateAccessor.DeleteAsync(turnContext);
                    IRestResponse restResponse = new SocialMiner().DisconnectFromAgent(objCounterState.CookieID, SocialMinerUrl);
                    await turnContext.SendActivityAsync(SocialMinerVerbiages.USER_LEFT);
                    Thread.Sleep(2000);
                    /*
                    var objCounterState1 = await _socilaMinerStateAccessor.GetAsync(turnContext, () => new CounterState());
                    objCounterState1.RequestAccepted = false;
                    objCounterState1.RequestedtoAgent = false;
                    objCounterState1.CookieID = string.Empty;
                    objCounterState1.LastChatID = 0;                    
                    await _socilaMinerStateAccessor.SetAsync(turnContext, objCounterState1);
                    */
                    await _userState.SaveChangesAsync(turnContext);
                    await _conversationState.SaveChangesAsync(turnContext);
                    
                }
                else
                {
                    IRestResponse restResponse = new SocialMiner().DisconnectFromAgent(objCounterState.CookieID, SocialMinerUrl);
                    objCounterState.RequestAccepted = false;
                    objCounterState.RequestedtoAgent = false;
                    objCounterState.CookieID = string.Empty;
                    objCounterState.LastChatID = 0;
                    await turnContext.SendActivityAsync(SocialMinerVerbiages.USER_LEFT);
                    await _socilaMinerStateAccessor.SetAsync(turnContext, objCounterState);
                    await _userState.SaveChangesAsync(turnContext);
                    await _conversationState.SaveChangesAsync(turnContext);
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> objDictionary = new Dictionary<string, string>();
                objDictionary.Add("Message", ex.Message);
                objDictionary.Add("InternalException", ex.InnerException.ToString());
                objDictionary.Add("StackTrace", ex.StackTrace);
                TelemetryLog.LogException(botServices, ex, objDictionary, "LiveChat", "DisconnectAgentAsync");

            }
        }
    }
}

