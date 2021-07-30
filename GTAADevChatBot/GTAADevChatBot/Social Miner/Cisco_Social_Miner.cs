using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using RestSharp;

namespace GTAADevChatBot.Social_Miner
{
    public class Cisco_Social_Miner
    {
        public async Task<IRestResponse> CreateSession()
        {
            try
            {
                #region RestClient Comment
                var client = new RestClient("https://www.ppcgtaa.org/customersupport/ccp/chat/100000/");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Host", "ppcgtaa.org");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("undefined", "title=CCX_Chat&extensionField_h_widgetName=TestStandardChat&extensionField_ccxqueuetag=Chat_Csq67&author=LaxmanCB", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                return response;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IRestResponse> GetResponsefromAgent(int eventId,string CookieID)
        {
            try
            {
                #region RestClient Comment
                string url = $"https://www.ppcgtaa.org/customersupport/ccp/chat?eventid={eventId}&all=false";
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("cookie", $"JSESSIONID={CookieID}");
                request.AddHeader("Host", "t1ebs008-gcsm1.voip.gtaa.com");
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


        public async Task<IRestResponse> PutResponseToAgent(string CookieID, string text)
        {
            try
            {
                #region RestClient Comment
                var client = new RestClient("https://www.ppcgtaa.org/customersupport/ccp/chat");
                var request = new RestRequest(Method.PUT);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("cookie", $"JSESSIONID={CookieID}");
                request.AddHeader("Host", "t1ebs008-gcsm1.voip.gtaa.com");
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Content-Type", "application/xml");
                request.AddParameter("undefined", $"<Message><body>{text}</body></Message>", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                return response;
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IRestResponse> DisconnectFromAgent(string CookieID)
        {
            try
            {
                #region RestClient Comment
                var client = new RestClient("https://www.ppcgtaa.org/customersupport/ccp/chat/leaveChat");
                var request = new RestRequest(Method.PUT);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("cookie", $"JSESSIONID={CookieID}");
                request.AddHeader("Host", "t1ebs008-gcsm1.voip.gtaa.com");
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
