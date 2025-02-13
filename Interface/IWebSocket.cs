namespace BitfinexConnector.Interface
{
    // разбил изначальный интерфейс на 2 + поменял void на Task

    interface IWebSocket
    {
        event Action<Trade> NewBuyTrade;
        event Action<Trade> NewSellTrade;
        Task SubscribeTradesAsync(string pair, int maxCount = 100);
        Task UnsubscribeTradesAsync(string pair);

        event Action<Candle> CandleSeriesProcessing;
        Task SubscribeCandlesAsync(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        Task UnsubscribeCandlesAsync(string pair);
    }
}
