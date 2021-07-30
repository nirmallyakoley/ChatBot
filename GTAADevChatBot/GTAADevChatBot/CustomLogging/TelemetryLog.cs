using GTAADevChatBot.Social_Miner;
using System.Collections.Generic;

namespace GTAADevChatBot.CustomLogging
{
    public class TelemetryLog
    {

        public static void LogCustomEvents(BotServices _BotServices, CSMState cSMState)
        {
            BotServices BotServices = _BotServices;
            Dictionary<string, string> CSMDictionary = new Dictionary<string, string>();

            CSMDictionary.Add("CSQ", cSMState.CSQ);

            if (!cSMState.didAgentAcceptRequest)
            {
                BotServices.TelemetryClient.TrackTrace("Support Initiated", CSMDictionary);
            }
            else
            {
                BotServices.TelemetryClient.TrackTrace("Support Request Accepted.", CSMDictionary);
            }
        }

        public static void LogCustomTrace(BotServices _BotServices, CSMState cSMState, string strMessage = "Chat Ongoing")
        {
            Dictionary<string, string> CSMDictionary = new Dictionary<string, string>();
            BotServices BotServices = _BotServices;
            if (cSMState != null)
            {
                CSMDictionary.Add("ChatFlow", cSMState.CSQ);
                BotServices.TelemetryClient.TrackTrace(strMessage, CSMDictionary);
            }
            else
                BotServices.TelemetryClient.TrackTrace(strMessage);

        }

        public static void LogCustomerFeedback(BotServices _BotServices, string feedback)
        {
            Dictionary<string, string> dictFeedback = new Dictionary<string, string>();
            dictFeedback.Add("Vote", feedback);
            _BotServices.TelemetryClient.TrackTrace("Customer Feedback ", dictFeedback);
        }

        public static void LogException(BotServices _BotServices, System.Exception ex, Dictionary<string,string> objDictionary,string strclass,string strMethod)
        {
            if(null!= objDictionary)
            {
                objDictionary.Add("Class",strclass);
                objDictionary.Add("Method", strMethod);
            }
           
            _BotServices.TelemetryClient.TrackException(ex, objDictionary);
        }

    }
}
