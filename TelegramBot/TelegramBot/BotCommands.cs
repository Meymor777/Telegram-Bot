using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using static ShowExchangeRateBot.DeserializeExchangeRate;
using Telegram.Bot.Requests;
using System;

namespace ShowExchangeRateBot
{
    public class BotCommands : IBotCommands
    {
        public Logger? Logger { get; set; } = null;
        public Currency? ChosenCurrency { get; set; } = null;
        public DateOnly? ChosenDate { get; set; } = null;
        public DateOnly? FirstDayOfMonth { get; set; } = null;
        private bool _currencyInputAwaited { get; set; } = false;
        private bool _dateInputAwaited { get; set; } = false;

        private const string StartCommand = "/start";
        private const string HelpCommand = "/help";
        private const string ShowCommand = "/show";
        private const string PreviousMonthCommand = "Previous month";
        private const string TodayCommand = "Today";
        private const string NextMonthCommand = "Next month";

        public async void OnMessage(ITelegramBotClient client, Update update)
        {
            try
            {
                await ProcessMessage(client, update);
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
            }
        }

        private async Task ProcessMessage(ITelegramBotClient client, Update update)
        {
            Message message = update.Message;
            User user = message.From;
            Chat chat = message.Chat;
            switch (message.Type)
            {
                case MessageType.Text:
                    {
                        if (message.Text == StartCommand)
                        {
                            await client.SendTextMessageAsync(chat.Id, "Welcome!!!");
                            break;
                        }
                        else if (message.Text == HelpCommand)
                        {
                            await client.SendTextMessageAsync(chat.Id, "This bot is needed to display the foreign currency exchange" +
                                        " rate to the hryvnia on the desired date");
                            break;
                        }
                        else if (message.Text == ShowCommand)
                        {
                            await client.SendTextMessageAsync(chat.Id, "Enter the wanted currency", replyMarkup: GetReplyCurrencyKeyboardMarkup());
                            _currencyInputAwaited = true;
                            _dateInputAwaited = false;
                            break;
                        }
                        else
                        {
                            await ProcessNotCommandText(client, message, chat);
                            break;
                        }
                    }
                default:
                    {
                        _currencyInputAwaited = false;
                        _dateInputAwaited = false;
                        await Task.CompletedTask;
                        break;
                    }
            }
        }
        private async Task ProcessNotCommandText(ITelegramBotClient client, Message message, Chat chat)
        {
            if (_currencyInputAwaited)
            {
                await ProcessCurrencyInput(client, message, chat);
            }
            else if (_dateInputAwaited)
            {
                await ProcessDateInput(client, message, chat);
            }
        }
        private async Task ProcessCurrencyInput(ITelegramBotClient client, Message message, Chat chat)
        {
            _currencyInputAwaited = false;
            ChosenCurrency = GetCurrency(message.Text);
            if (ChosenCurrency != null)
            {
                _dateInputAwaited = true;
                DateTime dateTime = DateTime.Now;
                FirstDayOfMonth = new DateOnly(dateTime.Year, dateTime.Month, 1);
                await client.SendTextMessageAsync(chat.Id, "Enter the wanted date", replyMarkup: GetDateReplyKeyboardMarkup());
            }
            else
            {
                SendMessageRequest request = new SendMessageRequest(chat.Id, "The currency is incorrect")
                {
                    ReplyMarkup = new ReplyKeyboardRemove()
                };
                await client.MakeRequestAsync(request);
            }
        }
        private async Task ProcessDateInput(ITelegramBotClient client, Message message, Chat chat)
        {
            if (message.Text == PreviousMonthCommand || message.Text == NextMonthCommand)
            {
                await ProcessCalendarCommand(client, message, chat);
                return;
            }

            _dateInputAwaited = false;
            if (message.Text == TodayCommand)
            {
                ChosenDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                ChosenDate = GetDate(message.Text);
            }
            if (ChosenDate != null)
            {
                string result = await GetTextResultRequest();
                SendMessageRequest request = new SendMessageRequest(chat.Id, result)
                {
                    ReplyMarkup = new ReplyKeyboardRemove()
                };
                await client.MakeRequestAsync(request);
            }
            else
            {
                SendMessageRequest request = new SendMessageRequest(chat.Id, "The date is incorrect")
                {
                    ReplyMarkup = new ReplyKeyboardRemove()
                };
                await client.MakeRequestAsync(request);
            }
        }
        private async Task ProcessCalendarCommand(ITelegramBotClient client, Message message, Chat chat)
        {
            if (message.Text == PreviousMonthCommand)
            {
                FirstDayOfMonth = FirstDayOfMonth.Value.AddMonths(-1);
                int yr = FirstDayOfMonth.Value.Year;
                int mth = FirstDayOfMonth.Value.Month;
                DateTime firstDay = new DateTime(yr, mth, 1);
                DateTime lastDay = new DateTime(yr, mth, 1).AddMonths(1).AddDays(-1);
                await client.SendTextMessageAsync(chat.Id, $"{firstDay.ToShortDateString()}-{lastDay.ToShortDateString()}", replyMarkup: GetDateReplyKeyboardMarkup());
                return;
            }
            else if (message.Text == NextMonthCommand)
            {
                FirstDayOfMonth = FirstDayOfMonth.Value.AddMonths(1);
                int yr = FirstDayOfMonth.Value.Year;
                int mth = FirstDayOfMonth.Value.Month;
                DateTime firstDay = new DateTime(yr, mth, 1);
                DateTime lastDay = new DateTime(yr, mth, 1).AddMonths(1).AddDays(-1);
                await client.SendTextMessageAsync(chat.Id, $"{firstDay.ToShortDateString()}-{lastDay.ToShortDateString()}", replyMarkup: GetDateReplyKeyboardMarkup());
                return;
            }
            else
            {
                await Task.CompletedTask;
                return;
            }
        }

        private ReplyKeyboardMarkup GetReplyCurrencyKeyboardMarkup()
        {
            return new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    GetArrayCurrencyKeyboardButtonByRange(1, 4),
                    GetArrayCurrencyKeyboardButtonByRange(5, 8)
                })
            {
                ResizeKeyboard = true,
            };
        }
        private KeyboardButton[] GetArrayCurrencyKeyboardButtonByRange(int startRange, int endRange)
        {
            List<KeyboardButton> currencies = new List<KeyboardButton>();
            for (int i = startRange; i <= endRange; i++)
            {
                currencies.Add(Enum.GetName(typeof(Currency), i - 1));
            }
            return currencies.ToArray();
        }
        private ReplyKeyboardMarkup GetDateReplyKeyboardMarkup()
        {
            return new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton(PreviousMonthCommand),
                        new KeyboardButton(TodayCommand),
                        new KeyboardButton(NextMonthCommand)
                    },
                    GetArrayDateKeyboardButtonByRange(1, 5),
                    GetArrayDateKeyboardButtonByRange(6, 10),
                    GetArrayDateKeyboardButtonByRange(11, 15),
                    GetArrayDateKeyboardButtonByRange(16, 20),
                    GetArrayDateKeyboardButtonByRange(21, 25),
                    GetArrayDateKeyboardButtonByRange(26, 31)
                })
            {
                ResizeKeyboard = true,
            };
        }
        private KeyboardButton[] GetArrayDateKeyboardButtonByRange(int startRange, int endRange)
        {
            int daysInMonth = DateTime.DaysInMonth(FirstDayOfMonth.Value.Year, FirstDayOfMonth.Value.Month);
            List<KeyboardButton> daysMonth = new List<KeyboardButton>();
            for (int i = startRange; i <= endRange; i++)
            {
                if (daysInMonth >= i)
                {
                    DateTime dateOfDay = new DateTime(FirstDayOfMonth.Value.Year, FirstDayOfMonth.Value.Month, i);
                    if (dateOfDay <= DateTime.Today)
                    {
                        daysMonth.Add(GetNameForDateKeyboardButton(i));
                    }
                }
            }
            return daysMonth.ToArray();
        }
        private string GetNameForDateKeyboardButton(int i)
        {
            return $"{i.ToString().PadLeft(2, '0')}." +
                   $"{FirstDayOfMonth.Value.Month.ToString().PadLeft(2, '0')}." +
                    $"{FirstDayOfMonth.Value.Year.ToString().PadLeft(4, '0')}";
        }

        public Currency? GetCurrency(string text)
        {
            if (Enum.TryParse(typeof(Currency), text, out object? value))
            {
                return (Currency)value;
            }
            else
            {
                return null;
            }
        }
        public DateOnly? GetDate(string text)
        {
            if (DateOnly.TryParse(text, out DateOnly date))
            {
                return date;
            }
            else
            {
                return null;
            }
        }

        public async Task<(DeserializeExchangeRate allExchangeRateData, ExchangeRateData neededExchangeRateData)> GetRequest()
        {
            HttpService httpService = new HttpService();
            string jsonString = await httpService.GetAsync($"https://api.privatbank.ua/p24api/exchange_rates?date={ChosenDate}");
            DeserializeExchangeRate allExchangeRateData = JsonConvert.DeserializeObject<DeserializeExchangeRate>(jsonString);
            ExchangeRateData neededExchangeRateData = allExchangeRateData.exchangeRate.Where(x => x.currency == ChosenCurrency.ToString()).FirstOrDefault();
            return (allExchangeRateData, neededExchangeRateData);
        }
        public async Task<string> GetTextResultRequest()
        {
            var result = await GetRequest();

            decimal purchaseRate = result.neededExchangeRateData?.purchaseRate ?? 0;
            decimal saleRate = result.neededExchangeRateData?.saleRate ?? 0;
            decimal purchaseRateNB = result.neededExchangeRateData?.purchaseRateNB ?? 0;
            decimal saleRateNB = result.neededExchangeRateData?.saleRateNB ?? 0;

            return $"Currency - {ChosenCurrency}\n" +
                   $"Date - {ChosenDate}\n" +
                   $"Purchase rate - {purchaseRate}\n" +
                   $"Sale rate - {saleRate}\n" +
                   $"Purchase rate NB - {purchaseRateNB}\n" +
                   $"Sale rate NB - {saleRateNB}\n";
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
    }
}
