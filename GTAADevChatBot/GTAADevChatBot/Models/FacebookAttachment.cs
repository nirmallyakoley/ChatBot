namespace GTAADevChatBot.Models
{
    public class FacebookAttachment
    {
        public string notification_type { get; set; }
        public Attachment attachment { get; set; }
    }

    public class Attachment
    {
        public string type { get; set; }
        public Payload payload { get; set; }
    }

    public class Payload
    {
        public string template_type { get; set; }
        public string intro_message { get; set; }
        public string update_type { get; set; }
        public string locale { get; set; }
        public string pnr_number { get; set; }
        public Update_Flight_Info update_flight_info { get; set; }
    }

    public class Update_Flight_Info
    {
        public string flight_number { get; set; }
        public Departure_Airport departure_airport { get; set; }
        public Arrival_Airport arrival_airport { get; set; }
        public Flight_Schedule flight_schedule { get; set; }
    }

    public class Departure_Airport
    {
        public string airport_code { get; set; }
        public string city { get; set; }
        public string terminal { get; set; }
        public string gate { get; set; }
    }

    public class Arrival_Airport
    {
        public string airport_code { get; set; }
        public string city { get; set; }
        public string terminal { get; set; }
        public string gate { get; set; }
    }

    public class Flight_Schedule
    {
        public string boarding_time { get; set; }
        public string departure_time { get; set; }
        public string arrival_time { get; set; }
    }

}
