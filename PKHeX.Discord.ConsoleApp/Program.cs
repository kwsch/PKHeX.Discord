using System;
using System.IO;
using System.Threading.Tasks;
using PKHeX.Discord.Axew;

namespace PKHeX.Discord.ConsoleApp
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            foreach (var line in args)
                Console.WriteLine(line);
            // Call the Program constructor, followed by the 
            // MainAsync method and wait until it finishes (which should be never).
            var token = File.ReadAllText("token.txt").Trim();
            await new AxewBot().MainAsync(token).ConfigureAwait(false);
        }
    }
}
