// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Linq;
using PictureBot.Models;
using PictureBot.Responses;
using Microsoft.Bot.Builder.AI.Luis;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.PictureBot;
using System.Collections.Generic;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace PictureBot.Bots
{
    public class PictureBot : ActivityHandler
    {
        private readonly PictureBotAccessors _accessors;
        // Initialize LUIS Recognizer

        private readonly ILogger _logger;
        private DialogSet _dialogs;
        private List<SearchClient> _azSearches;


        /// <summary>
        /// Every conversation turn for our PictureBot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response. Later, when we add Dialogs, we'll have to navigate through this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is "message")
            {
                var utterance = turnContext.Activity.Text;
                var state = await _accessors.PictureState.GetAsync(turnContext, () => new PictureState());
                state.UtteranceList.Add(utterance);
                await _accessors.ConversationState.SaveChangesAsync(turnContext);

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

        public PictureBot(PictureBotAccessors accessors, ILoggerFactory loggerFactory, List<SearchClient> azureSearchClients  /*, LuisRecognizer recognizer*/)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            if(azureSearchClients == null)
            {
                throw new System.ArgumentNullException(nameof(azureSearchClients));
            }

            if (azureSearchClients.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(azureSearchClients));
            }


            //add instance of Azure Search Client
            _azSearches = azureSearchClients;

            // Add instance of LUIS Recognizer

            _logger = loggerFactory.CreateLogger<PictureBot>();
            _logger.LogTrace("PictureBot turn start.");
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
                AskForSearchTermAsync,
                SearchAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("mainDialog", main_waterfallsteps));
            _dialogs.Add(new WaterfallDialog("searchDialog", search_waterfallsteps));
            // The following line allows us to use a prompt within the dialogs
            _dialogs.Add(new TextPrompt("searchPrompt"));
        }

        // Add MainDialog-related tasks

        // Add SearchDialog-related tasks

        // Add search related tasks
        private List<SearchHit> RunQueries(List<SearchClient> searches, string searchText)
        {
            var results = new List<SearchHit>();

            foreach (var searchClient in searches)
            {
                SearchOptions options;
                SearchResults<SearchHit> _results;

                options = new SearchOptions();
               // options.Select.Add(searchClient.IndexName);

                _results = searchClient.Search<SearchHit>(searchText, options);
               
                foreach (var _res in _results.GetResults())
                {
                    results.Add(_res.Document );
                }

                // can add filtering 
                //options = new SearchOptions()
                //{
                //    Filter = "Rooms/any(r: r/BaseRate lt 100)",
                //      Size = 2
                //};
                //options.Select.Add("HotelId");
                //options.Select.Add("Description");

                //results = searchClient.Search<Hotel>("*", options);

              
            }
            return results;
        }

        // If we haven't greeted a user yet, we want to do that first, but for the rest of the
        // conversation we want to remember that we've already greeted them.
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

        // Greet the user
        // Update the GreetedState to greeted

        private async Task<DialogTurnResult> AskForSearchTermAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the state for the current step in the conversation
            var state = await _accessors.PictureState.GetAsync(stepContext.Context, () => new PictureState());

            // Save the new greeted state into the conversation state
            // This is to ensure in future turns we do not greet the user again
            // Ask the user what they want to do next
            state.Searching = true;
            await _accessors.ConversationState.SaveChangesAsync(stepContext.Context);
            
            
           // await SearchResponses.ReplyWithGreeting(stepContext.Context);
            
           return await stepContext.PromptAsync("searchPrompt", new PromptOptions { Prompt = MessageFactory.Text("Ok, What would you like to search for?") }, cancellationToken);

            // Move to the next waterfall step, which is MainMenuAsync
          //  return await stepContext.NextAsync();

        }

        // If we haven't greeted a user yet, we want to do that first, but for the rest of the
        // conversation we want to remember that we've already greeted them.
        private async Task<DialogTurnResult> SearchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the state for the current step in the conversation
            var state = await _accessors.PictureState.GetAsync(stepContext.Context, () => new PictureState());

            // If we haven't asked the user for the search term ask now
            if (state.Searching)
            {
                var searchText = stepContext.Context.Activity.Text;
                await SearchResponses.ReplyWithSearchConfirmation(stepContext.Context, searchText);

                List<SearchHit> results = RunQueries(_azSearches, searchText);

                if(results.Count == 0)
                {
                    await SearchResponses.ReplyWithNoResults(stepContext.Context, searchText);
                }
                else
                {
                    await SearchResponses.ReplyWithResults(stepContext.Context, searchText, results);
                }

                 state.Searching = false;
                await _accessors.ConversationState.SaveChangesAsync(stepContext.Context);



            }
            // Since we aren't explicitly prompting the user in this step, we'll end the dialog
            // When the user replies, since state is maintained, the else clause will move them
            // to the next waterfall step
            return await stepContext.EndDialogAsync();

        }

       

    }
}
