using System.Runtime.InteropServices;

namespace ShowExchangeRateBot.Test
{
    public class Tests
    {
        [TestCase("USD", Currency.USD)]
        [TestCase("EUR", Currency.EUR)]
        [TestCase("SEK", Currency.SEK)]
        [TestCase("PLZ", Currency.PLZ)]
        [TestCase("test wrong input", null)]
        [TestCase("", null)]
        public void GetNeededCurrencyExpectTrue(string strCurrency, Currency? resultCurrency)
        {
            BotCommands botCommands = new BotCommands();
            Assert.AreEqual(resultCurrency, botCommands.GetCurrency(strCurrency));
        }

        [Test]
        public void GetNeededDateExpectTrue()
        {
            string strDate = "01.01.2024";
            DateOnly? resultDate = new DateOnly(2024, 1, 1);
            BotCommands botCommands = new BotCommands();
            Assert.AreEqual(resultDate, botCommands.GetDate(strDate));
        }

        [Test]
        public void GetWrongInputDateExpectNull()
        {
            string strDate = "test wrong input";
            DateOnly? resultDate = null;
            BotCommands botCommands = new BotCommands();
            Assert.AreEqual(resultDate, botCommands.GetDate(strDate));
        }
     
        [Test]
        public void GetEmptyStringDateExpectNull()
        {
            string strDate = "";
            DateOnly? resultDate = null;
            BotCommands botCommands = new BotCommands();
            Assert.AreEqual(resultDate, botCommands.GetDate(strDate));
        }
    }
}