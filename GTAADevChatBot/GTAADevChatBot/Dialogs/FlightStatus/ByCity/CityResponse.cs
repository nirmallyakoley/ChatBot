using System;

namespace GTAADevChatBot.Dialogs.FlightStatus.ByCity
{
    public class FlightStatusSearchAPIResponseJson
    {
        public DateTime lastUpdate { get; set; }
        public DateTime serverTime { get; set; }
        public FlightByCity[] list { get; set; }
        public string today { get; set; }
        public string tomorrow { get; set; }
        public string yesterday { get; set; }
    }

    public class FlightByCity
    {
        public string key { get; set; }
        public string id { get; set; }
        public string id2 { get; set; }
        public string type { get; set; }
        public DateTime schTime { get; set; }
        public DateTime latestTm { get; set; }
        public string gate { get; set; }
        public string status { get; set; }
        public string term { get; set; }
        public string al { get; set; }
        public string alCode { get; set; }
        public Id[] ids { get; set; }
        public Route[] routes { get; set; }
        public string carousel { get; set; }
        public string termzone { get; set; }
    }

    public class Id
    {
        public string id { get; set; }
        public string id2 { get; set; }
        public string alName { get; set; }
    }

    public class Route
    {
        public string code { get; set; }
        public string name { get; set; }
        public string _short { get; set; }
        public string city { get; set; }
        public string cnty { get; set; }
        public string region { get; set; }
    }

}
