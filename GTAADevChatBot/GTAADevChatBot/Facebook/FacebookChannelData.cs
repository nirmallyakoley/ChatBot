using Newtonsoft.Json;
using System.Collections.Generic;

namespace GTAADevChatBot.Facebook
{
    public class FaceBookButton
    {
        [JsonProperty("title")]
        public string title { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("url")]
        public string url { get; set; }
    }

    public class FaceBookElement
    {
        [JsonProperty("title")]
        public string title { get; set; }
        [JsonProperty("subtitle")]
        public string subtitle { get; set; }
        [JsonProperty("image_url")]
        public string image_url { get; set; }
        [JsonProperty("buttons")]
        public List<FaceBookButton> buttons { get; set; }
    }

    public class FaceBookPayload
    {
        [JsonProperty("template_type")]
        public string template_type { get; set; }
        [JsonProperty("elements")]
        public List<FaceBookElement> elements { get; set; }
    }

    public class FacebookAttachment
    {
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("payload")]
        public FaceBookPayload payload { get; set; }
    }

    public class FacebookChannelData
    {
        [JsonProperty("notification_type")]
        public string notification_type { get; set; }
        [JsonProperty("attachment")]
        public FacebookAttachment attachment { get; set; }
    }
}
