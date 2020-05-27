using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace PictureBot.Responses
{
    public class MainResponses
    {
        static Random rnd;
        static string[] RandomPhrases = new string[]
        {
            "Elephant in the Room", "Drawing a Blank","Read 'Em and Weep", "Happy as a Clam","Mountain Out of a Molehill","It's Not All It's Cracked Up To Be so ","You Hit Below The Belt so","Right Off the Bat ","There's no Beating Around the Bush","Birds of a Feather Flock Together and you're not with me ","Up In Arms huh ", "Easy As Pie but not this time ","Not your Cup of Tea huh","You Can't Judge a Book By Its Cover but I can judge by your statement you don't know what you're talking about","I hate to Rain on Your Parade, but...","Back To the Drawing Board","Being faced with difficult choices huh"
        };

        static MainResponses()
        {
            rnd = new Random();
        }

        public static async Task ReplyWithGreeting(ITurnContext context)
        {
            await context.SendActivityAsync($"Greetings");
        }
        public static async Task ReplyWithHelp(ITurnContext context)
        {
            await context.SendActivityAsync($"I can search for pictures, share pictures and order prints of pictures.");
        }
        public static async Task ReplyWithResumeTopic(ITurnContext context)
        {
            await context.SendActivityAsync($"What can I do for you?");
        }
        public static async Task ReplyWithConfused(ITurnContext context)
        {
            // Add a response for the user if Regex or LUIS doesn't know
            // What the user is trying to communicate
            var i = rnd.Next(0, RandomPhrases.Length - 1);
            string randomPhrase = RandomPhrases[i];
            await context.SendActivityAsync(randomPhrase + "... could you try again?");

        }
        public static async Task ReplyWithLuisScore(ITurnContext context, string key, double score)
        {
            await context.SendActivityAsync($"Intent: {key} ({score}).");
        }
        public static async Task ReplyWithShareConfirmation(ITurnContext context)
        {
            await context.SendActivityAsync($"Posting your picture(s) on twitter...");
        }
        public static async Task ReplyWithOrderConfirmation(ITurnContext context)
        {
            await context.SendActivityAsync($"Ordering standard prints of your picture(s)...");
        }
    }
}