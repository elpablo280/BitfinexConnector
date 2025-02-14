namespace BitfinexConnector.Clients
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BitfinexConnector.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RestApiClient : IRestApi
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.bitfinex.com/v2/";

        public RestApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            var response = await _httpClient.GetAsync($"trades/{pair}/hist?limit={maxCount}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var trades = ParseTrades(json);
            return trades;
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            var url = $"candles/trade:30m:tBTCUSD/hist";       // todo
            //var url = $"candles/trade:{periodInSec}:{pair}/hist?limit={count}&start={from?.ToUnixTimeSeconds()}&end={to?.ToUnixTimeSeconds()}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var candles = ParseCandles(json);
            return candles;
        }

        private IEnumerable<Trade> ParseTrades(string json)
        {
            var trades = new List<Trade>();
            var array = JArray.Parse(json);
            foreach (var item in array)
            {
                var trade = new Trade
                {
                    Id = item[0].ToString(),
                    Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(item[1].ToString()) / 1000),   // делим на тысячу, т.к. возвращается из апи в миллисекундах
                    Amount = decimal.Parse(item[2].ToString()),
                    Price = decimal.Parse(item[3].ToString())
                };
                trades.Add(trade);
            }
            return trades;
        }

        private IEnumerable<Candle> ParseCandles(string json)
        {
            var candles = new List<Candle>();
            var array = JArray.Parse(json);
            foreach (var item in array)
            {
                var candle = new Candle
                {
                    OpenTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(item[0].ToString()) / 1000),   // делим на тысячу, т.к. возвращается из апи в миллисекундах
                    OpenPrice = decimal.Parse(item[1].ToString()),
                    ClosePrice = decimal.Parse(item[2].ToString()),
                    HighPrice = decimal.Parse(item[3].ToString()),
                    LowPrice = decimal.Parse(item[4].ToString()),
                    TotalVolume = decimal.Parse(item[5].ToString())
                };
                candles.Add(candle);
            }
            return candles;
        }
    }
}