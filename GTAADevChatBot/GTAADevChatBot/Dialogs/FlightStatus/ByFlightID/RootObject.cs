using System;
using System.Collections.Generic;

namespace GTAADevChatBot.Dialogs.FlightStatus.ByFlightID
{
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
        public string @short { get; set; }
        public string city { get; set; }
        public string cnty { get; set; }
        public string region { get; set; }
    }

    public class Flight
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
        public List<Id> ids { get; set; }
        public List<Route> routes { get; set; }
        public object carousel { get; set; }
        public string termzone { get; set; }
    }
}
