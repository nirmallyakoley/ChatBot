// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace GTAADevChatBot.Social_Miner
{
    /// <summary>
    /// Stores counter state for the conversation.
    /// Stored in <see cref="Microsoft.Bot.Builder.ConversationState"/> and
    /// backed by <see cref="Microsoft.Bot.Builder.MemoryStorage"/>.
    /// </summary>
    public class CounterState
    {
        /// <summary>
        /// Gets or sets the number of turns in the conversation.
        /// </summary>
        /// <value>The number of turns in the conversation.</value>
        public int TurnCount { get; set; } = 0;

        public int LastChatID { get; set; } = 0;

        public string CookieID { get; set; }

        public bool RequestedtoAgent { get; set; } = false;

        public bool RequestAccepted { get; set; } = false;
        
       
    }
}
