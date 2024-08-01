using Telegram.Bot.Types;
using Telegram.Bot;

namespace ShowExchangeRateBot
{
    public interface IBotCommands
    {
        public void OnMessage(ITelegramBotClient client, Update update);
    }
}
