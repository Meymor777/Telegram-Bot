namespace ShowExchangeRateBot
{
    public class DeserializeExchangeRate
    {
        public string? date { get; set; }
        public string? bank { get; set; }
        public int baseCurrency { get; set; }
        public string? baseCurrencyLit { get; set; }
        public List<ExchangeRateData>? exchangeRate { get; set; }

        public class ExchangeRateData
        {
            public string? baseCurrency { get; set; }
            public string? currency { get; set; }
            public decimal saleRateNB { get; set; }
            public decimal purchaseRateNB { get; set; }
            public decimal saleRate { get; set; }
            public decimal purchaseRate { get; set; }

        }
    }
}
