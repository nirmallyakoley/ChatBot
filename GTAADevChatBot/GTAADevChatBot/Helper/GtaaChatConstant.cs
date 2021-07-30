namespace GTAADevChatBot.Helper
{
    public class GtaaChatConstant
    {
        public const string LUIS_CONFIGURATION = "GTAADevChatBotLuisAppProd";
        public const string LUIS_CONFIGURATIONAirline = "GTAADevChatBotLuisAppAirlineFlow";
        public const string HABANERO_API_BASE_ADDRESS = "https://gtaa-fl-uat.azureedge.net/";
        public const string ONT = "On Time";
        public const string DEL = "Delayed";
        public const string ARR = "Arrived";
        public const string ERL = "Early";
        public const string CAN = "Cancelled";
        public const string LND = "Landed";
        public const string DEP = "Departed";
        public const string DIV = "Diverted";
        public const string RETRY_PROMPT = "I’m sorry I did not understand you. Can you please choose from the options below. You may also type RESTART to start over or type SUPPORT for a live agent.";
    }
}
