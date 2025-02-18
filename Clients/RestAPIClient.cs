namespace BitfinexConnector.Clients
{
    using System;
    using System.Globalization;
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

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, string period, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            string url = $"candles/trade:{period}:{pair}/hist";
            if (to is not null)
            {
                url += $"?limit={count}&start={from?.ToUnixTimeSeconds()}&end={to?.ToUnixTimeSeconds()}";
            }
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
                if (long.TryParse(item[1].ToString(), out long time) &&
                    decimal.TryParse(item[2].ToString(), out decimal amount) &&
                    decimal.TryParse(item[3].ToString(), out decimal price))
                {
                    var trade = new Trade
                    {
                        Id = item[0].ToString(),
                        Time = DateTimeOffset.FromUnixTimeSeconds(time / 1000),
                        Amount = amount,
                        Price = price
                    };
                    trades.Add(trade);
                }
            }
            return trades;
        }

        private IEnumerable<Candle> ParseCandles(string json)
        {
            var candles = new List<Candle>();
            var array = JArray.Parse(json);
            foreach (var item in array)
            {
                if (long.TryParse(item[0].ToString(), out long openTime) &&
                    decimal.TryParse(item[1].ToString(), out decimal openPrice) &&
                    decimal.TryParse(item[2].ToString(), out decimal closePrice) &&
                    decimal.TryParse(item[3].ToString(), out decimal highPrice) &&
                    decimal.TryParse(item[4].ToString(), out decimal lowPrice) &&
                    decimal.TryParse(item[5].ToString(), out decimal totalVolume))
                {
                    var candle = new Candle
                    {
                        OpenTime = DateTimeOffset.FromUnixTimeSeconds(openTime / 1000),
                        OpenPrice = openPrice,
                        ClosePrice = closePrice,
                        HighPrice = highPrice,
                        LowPrice = lowPrice,
                        TotalVolume = totalVolume
                    };
                    candles.Add(candle);
                }
            }
            return candles;
        }
    }
}