// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.PictureBot;
using PictureBot.Responses;

namespace PictureBot.Bots
{
    public class PictureBot : ActivityHandler
    {
        private readonly PictureBotAccessors _accessors;
        private DialogSet _dialogs;

        public PictureBot(PictureBotAccessors accessors)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            // The DialogSet needs a DialogState accessor, it will call it when it has a turn context.
            _dialogs = new DialogSet(_accessors.DialogStateAccessor);

            // This array defines how the Waterfall will execute.
            // We can define the different dialogs and their steps here
            // allowing for overlap as needed. In this case, it's fairly simple
            // but in more complex scenarios, you may want to separate out the different
            // dialogs into different files.
            var main_waterfallsteps = new WaterfallStep[]
            {
                GreetingAsync,
                MainMenuAsync,
            };
            var search_waterfallsteps = new WaterfallStep[]
            {
                // Add SearchDialog water fall steps

            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("mainDialog", main_waterfallsteps));
            _dialogs.Add(new WaterfallDialog("searchDialog", search_waterfallsteps));
            // The following line allows us to use a prompt within the dialogs
            _dialogs.Add(new TextPrompt("searchPrompt"));
        }


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is "message")
            {
                // Establish dialog context from the conversation state.
                var dc = await _dialogs.CreateContextAsync(turnContext);
                // Continue any current dialog.
                var results = await dc.ContinueDialogAsync(cancellationToken);

                // Every turn sends a response, so if no response was sent,
                // then there no dialog is currently active.
                if (!turnContext.Responded)
                {
                    // Start the main dialog
                    await dc.BeginDialogAsync("mainDialog", null, cancellationToken);
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> GreetingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the state for the current step in the conversation
            var state = await _accessors.PictureState.GetAsync(stepContext.Context, () => new PictureState());

            // If we haven't greeted the user
            if (state.Greeted == "not greeted")
            {
                // Greet the user
                await MainResponses.ReplyWithGreeting(stepContext.Context);
                // Update the GreetedState to greeted
                state.Greeted = "greeted";
                // Save the new greeted state into the conversation state
                // This is to ensure in future turns we do not greet the user again
                await _accessors.ConversationState.SaveChangesAsync(stepContext.Context);
                // Ask the user what they want to do next
                await MainResponses.ReplyWithHelp(stepContext.Context);
                // Since we aren't explicitly prompting the user in this step, we'll end the dialog
                // When the user replies, since state is maintained, the else clause will move them
                // to the next waterfall step
                return await stepContext.EndDialogAsync();
            }
            else // We've already greeted the user
            {
                // Move to the next waterfall step, which is MainMenuAsync
                return await stepContext.NextAsync();
            }

        }

        // This step routes the user to different dialogs
        // In this case, there's only one other dialog, so it is more simple,
        // but in more complex scenarios you can go off to other dialogs in a similar
        public async Task<DialogTurnResult> MainMenuAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Check if we are currently processing a user's search
            var state = await _accessors.PictureState.GetAsync(stepContext.Context);

            // If Regex picks up on anything, store it
            var recognizedIntents = stepContext.Context.TurnState.Get<IRecognizedIntents>();
            // Based on the recognized intent, direct the conversation
            switch (recognizedIntents.TopIntent?.Name)
            {
                case "search":
                    // switch to the search dialog
                    return await stepContext.BeginDialogAsync("searchDialog", null, cancellationToken);
                case "share":
                    // respond that you're sharing the photo
                    await MainResponses.ReplyWithShareConfirmation(stepContext.Context);
                    return await stepContext.EndDialogAsync();
                case "order":
                    // respond that you're ordering
                    await MainResponses.ReplyWithOrderConfirmation(stepContext.Context);
                    return await stepContext.EndDialogAsync();
                case "help":
                    // show help
                    await MainResponses.ReplyWithHelp(stepContext.Context);
                    return await stepContext.EndDialogAsync();
                default:
                    {
                        await MainResponses.ReplyWithConfused(stepContext.Context);
                        return await stepContext.EndDialogAsync();
                    }
            }
        }
    }
}
