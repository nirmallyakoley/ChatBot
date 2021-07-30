using GTAADevChatBot.Dialogs.FlightStatus.ByCity;
using System;
using System.Collections.Generic;

namespace GTAADevChatBot.Dialogs.FlightStatus
{
    public class FlightStatusState
    {
        public string userInput { get; set; }

        public string AD { get; set; }

        public string TT { get; set; }

        public string FlightID { get; set; }

        public string FlightNumber { get; set; }

        public string City { get; set; }

        public string Airline { get; set; }

        public DateTime dateTime { get; set; }

        public string Time { get; set; }
        public int skip { get; set; } = 0;

        public int finish { get; set; } = 10;

        public List<FlightByCity> FlightsList { get; set; }

        public string al { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

    }

}

