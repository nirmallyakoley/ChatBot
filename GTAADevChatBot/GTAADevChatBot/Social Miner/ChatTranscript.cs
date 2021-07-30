using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Schema;
using System;


namespace GTAADevChatBot.Social_Miner
{
    public class ChatTranscript
    {
        public static string ReadChatTranscript(string channelId, string conversationID, BotServices botServices,string transcriptContainerName)
        {            
            System.DateTime UtcTime = System.DateTime.UtcNow.Date;
            DateTimeOffset dtoUtc = new DateTimeOffset(UtcTime, TimeSpan.Zero);
            string chatTranscript = string.Empty;
            PagedResult<IActivity> pagedActivities = null;
            string blobConnectionString = botServices.BlobStorageService.ConnectionString;
            string containerName = transcriptContainerName;
           
            AzureBlobTranscriptStore azureBlobTranscriptStore = new AzureBlobTranscriptStore(blobConnectionString, containerName);
            pagedActivities = azureBlobTranscriptStore.GetTranscriptActivitiesAsync(channelId, conversationID,null, dtoUtc).Result;

            foreach (var item in pagedActivities.Items)
            {
                chatTranscript = chatTranscript + item.AsMessageActivity()?.Text + System.Environment.NewLine;
            }

            return chatTranscript;
        }
    }
}
