using System.Configuration;

namespace ShowExchangeRateBot
{
    public class Program
    {
        const string ExitCommand = "exit";

        public static void Main(string[] args)
        {
            Logger logger = new Logger();
            Host showExchangeRateBot = new Host(ConfigurationManager.ConnectionStrings["TelegramApi"].ConnectionString);
            BotCommands botCommands = new BotCommands();
            showExchangeRateBot.Logger = logger;
            botCommands.Logger = logger;
            showExchangeRateBot.Start();
            showExchangeRateBot.OnMessage += botCommands.OnMessage;
            while (true)
            {
                string text = Console.ReadLine();
                if (text == ExitCommand)
                {
                    break;
                }
            }
        }
    }
}
