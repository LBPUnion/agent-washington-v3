using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;

namespace LBPunion.AgentWashington.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger.Log("Waking up...");

            DiscordBot.Bootstrap();
        }
    }
}