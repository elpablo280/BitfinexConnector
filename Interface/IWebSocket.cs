namespace BitfinexConnector.Interface
{
    // разбил изначальный интерфейс на 2 + поменял void на Task + поменял period на string

    public interface IWebSocket
    {
        event Action<Trade> NewBuyTrade;
        event Action<Trade> NewSellTrade;
        Task SubscribeTradesAsync(string pair, int maxCount = 100);
        Task UnsubscribeTradesAsync(string pair);

        event Action<Candle> CandleSeriesProcessing;

        /// <summary>
        /// Available values: "1m", "5m", "15m", "30m", "1h", "3h", "6h", "12h", "1D", "1W", "14D", "1M" (https://docs.bitfinex.com/reference/rest-public-candles)
        /// </summary>

        Task SubscribeCandlesAsync(string pair, string period, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        Task UnsubscribeCandlesAsync(string pair);
    }
}
