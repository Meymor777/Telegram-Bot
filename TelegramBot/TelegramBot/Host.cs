using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShowExchangeRateBot
{
    public class Host
    {
        public Logger? Logger { get; set; } = null;
        public Action<ITelegramBotClient, Update>? OnMessage { get; set; }
        private TelegramBotClient _bot { get; set; }

        public Host(string token)
        {
            _bot = new TelegramBotClient(token);
        }
        public void Start()
        {
            _bot.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine($"{DateTime.Now} : Show exchange rate bot is running!");
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            LogErrorMessage(exception);
            await Task.CompletedTask;
        }
        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            try
            {
                Message message = update.Message;
                User user = message.From;
                Chat chat = message.Chat;
                LogInfoMessage(user, chat, message);
                OnMessage?.Invoke(client, update);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                await Task.CompletedTask;
            }
        }

        private void LogErrorMessage(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now} Error: {exception.Message}");
            Console.ResetColor();
            if (Logger != null)
            {
                Logger.Log($"{DateTime.Now} Error: {exception.Message}");
            }
        }
        private void LogInfoMessage(User user, Chat chat, Message message)
        {
            string messageText = "";
            Console.ForegroundColor = ConsoleColor.Yellow;
            switch (message.Type)
            {
                case MessageType.Text:
                    {
                        messageText = $"{DateTime.Now} : The user {chat.Id} {user.FirstName} {user.LastName} " +
                            $"{user.Username} sent {message.Text}";
                        Console.WriteLine(messageText);
                        break;
                    }
                default:
                    {
                        messageText = $"{DateTime.Now} : The user {chat.Id} {user.FirstName} {user.LastName} " +
                            $"{user.Username} sent [not a text]";
                        Console.WriteLine(messageText);
                        break;
                    }
            }
            Console.ResetColor();
            if (Logger != null)
            {
                Logger.Log(messageText);
            }
        }
    }
}