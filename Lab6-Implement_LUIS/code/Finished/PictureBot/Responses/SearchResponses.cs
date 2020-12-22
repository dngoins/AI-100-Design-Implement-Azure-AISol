using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using PictureBot.Models;
using System.Text;

namespace PictureBot.Responses
{
    public class SearchResponses
    {
        // add a task called "ReplyWithSearchRequest"
        // it should take in the context and ask the
        // user what they want to search for
        public static async Task ReplyWithSearchConfirmation(ITurnContext context, string utterance)
        {
            await context.SendActivityAsync($"Ok, searching for pictures of {utterance}");
        }
        public static async Task ReplyWithNoResults(ITurnContext context, string utterance)
        {
            await context.SendActivityAsync("There were no results found for \"" + utterance + "\".");
        }

        public static async Task ReplyWithResults(ITurnContext context, string utterance, List<SearchHit> results)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var res in results)
            {
                if (string.IsNullOrEmpty(res.Caption))
                {
                    sb.Append(res.merged_content);
                }
                else
                {
                    sb.Append(string.Format("\nItem: {0}\nDescription: {1}\nUrl: {2}\n\n", res.Content, res.Caption, res.BlobUri));
                }
            }

            await context.SendActivityAsync(string.Format("We found {0} results found for \"" + utterance + "\".\n\n{1}", results.Count.ToString(), sb.ToString()));
        }

        public static async Task ReplyWithGreeting(ITurnContext context)
        {
            // Add a greeting
            await context.SendActivityAsync($"Ok what type of pictures would you like to search for?");
        }
    }
}