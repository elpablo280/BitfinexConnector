namespace BitfinexConnector.Interface
{
    // разбил изначальный интерфейс на 2

    public interface IRestApi
    {
        Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
        Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);
    }
}
