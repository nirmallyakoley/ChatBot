// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace GTAADevChatBot.Dialogs
{
    public class CancelAndHelpDialog : ComponentDialog
    {
        public CancelAndHelpDialog(string id) : base(id)
        {
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "help":
                    case "?":
                    case "exit":
                    case "cancel":
                    case "quit":
                        //await innerDc.Context.SendActivityAsync($"Is there anything else I can help you with? ", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();

                    case "restart":
                        //await AllDialogEnd.RestartConversationAsync(innerDc.Context, cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();

                    case var restart when new Regex(@"^[\s$&+,:;=?@#|'<>.-^*()%!]*restart[\s$&+,:;=?@#|'<>.-^*()%!]*$").IsMatch(text):
                        return await innerDc.CancelAllDialogsAsync();
                    case "support":
                        //await innerDc.Context.SendActivityAsync($"we are in development mode, I will be soon", cancellationToken: cancellationToken);
                        //await AllDialogEnd.RestartConversationAsync(innerDc.Context, cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();
                }
            }

            return null;
        }
    }
}
