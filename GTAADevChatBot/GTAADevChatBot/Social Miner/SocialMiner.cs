using Microsoft.Bot.Builder;
using RestSharp;
using System;

namespace GTAADevChatBot.Social_Miner
{
    public class SocialMiner
    {
        public IRestResponse CreateSession(ITurnContext turnContext, CSMState cSMState, string SocialMinerUrl)
        {
            try
            {
                //string url = "https://www.ppcgtaa.org/ccp/chat/100000/";
                string url = $"https://{SocialMinerUrl}/ccp/chat/100000/";
                #region RestClient Comment  
                //var SocialMinerUrl=_Configuration.GetSection("SocialMinerUrl")?.Value;
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Host", SocialMinerUrl);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                if (turnContext.Activity.ChannelId == "facebook")
                {
                    request.AddParameter("undefined", $"title=CCX_Chat&extensionField_h_widgetName=TestStandardChat&extensionField_ccxqueuetag=Chat_Csq{cSMState.ID}&author={turnContext.Activity.From.Name}", ParameterType.RequestBody);
                }
                else
                {
                    //if (turnContext.Activity.ChannelId == "directline")
                    //request.AddParameter("undefined", $"title=CCX_Chat&extensionField_h_widgetName=TestStandardChat&extensionField_ccxqueuetag=Chat_Csq{cSMState.ID}&author=Web-User-{turnContext.Activity.From.Id}", ParameterType.RequestBody);
                    request.AddParameter("undefined", $"title=CCX_Chat&extensionField_h_widgetName=TestStandardChat&extensionField_ccxqueuetag=Chat_Csq{cSMState.ID}&author=Web User", ParameterType.RequestBody);
                }
               
                    

                IRestResponse response = client.Execute(request);
                return response;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IRestResponse GetResponsefromAgent(int eventId, string CookieID, string SocialMinerUrl)
        {
            try
            {
                #region RestClient Comment
                // string url = $"https://www.ppcgtaa.org/ccp/chat?eventid={eventId}&all=false";
                string url = $"https://{SocialMinerUrl}/ccp/chat?eventid={eventId}&all=false";
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("cookie", $"JSESSIONID={CookieID}");
                request.AddHeader("Host", SocialMinerUrl);
                request.AddHeader("Accept", "*/*");
                IRestResponse response = client.Execute(request);
                return response;
                #endregion
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        public IRestResponse PutResponseToAgent(string CookieID, string text, string SocialMinerUrl)
        {
            try
            {
                #region RestClient Comment
                // string url = $"https://www.ppcgtaa.org/ccp/chat";
                string url = $"https://{SocialMinerUrl}/ccp/chat";
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("cookie", $"JSESSIONID={CookieID}");
                request.AddHeader("Host", SocialMinerUrl);
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Content-Type", "application/xml");
                request.AddParameter("undefined", $"<Message><body>{text.Replace("&","&amp;")}</body></Message>", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                return response;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IRestResponse DisconnectFromAgent(string CookieID, string SocialMinerUrl)
        {
            try
            {
                //string url= "https://www.ppcgtaa.org/ccp/chat/leaveChat";
                string url = $"https://{SocialMinerUrl}/ccp/chat/leaveChat";
                #region RestClient Comment
                var client = new RestClient(url);
                var request = new RestRequest(Method.PUT);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("cookie", $"JSESSIONID={CookieID}");
                request.AddHeader("Host", SocialMinerUrl);
                request.AddHeader("Accept", "*/*");
                IRestResponse response = client.Execute(request);
                return response;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
