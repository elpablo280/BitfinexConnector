namespace BitfinexConnector.Interface
{
    // разбил изначальный интерфейс на 2 + поменял period на string

    public interface IRestApi
    {
        Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);

        /// <summary>
        /// Available values: "1m", "5m", "15m", "30m", "1h", "3h", "6h", "12h", "1D", "1W", "14D", "1M" (https://docs.bitfinex.com/reference/rest-public-candles)
        /// </summary>
        Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, string period, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);
    }
}
