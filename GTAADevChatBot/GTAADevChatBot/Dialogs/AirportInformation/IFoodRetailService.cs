using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GTAADevChatBot.Dialogs.AirportInformation
{
    public class IFoodRetailService
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Hours { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Image { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Terminal { get; set; }


        [JsonProperty(Required = Required.Always)]
        public string Location { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string[] Country { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Security { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string URL { get; set; }
    }
}
