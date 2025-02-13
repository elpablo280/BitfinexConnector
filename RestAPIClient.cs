namespace BitfinexConnector
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using TestHQ;

    public class RestAPIClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.bitfinex.com/v2/";

        public RestAPIClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int maxCount)
        {
            var response = await _httpClient.GetAsync($"trades/{pair}/hist?limit={maxCount}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Trade>>(json);
        }

        public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to, long? count)
        {
            var query = $"candles/trade:{periodInSec}:{pair}/hist?limit={count}";
            if (from.HasValue) query += $"&start={from.Value.ToUnixTimeSeconds()}";
            if (to.HasValue) query += $"&end={to.Value.ToUnixTimeSeconds()}";

            var response = await _httpClient.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Candle>>(json);
        }

        //public async Task<Ticker> GetTickerAsync(string pair)
        //{
        //    var response = await _httpClient.GetAsync($"ticker/{pair}");
        //    response.EnsureSuccessStatusCode();
        //    var json = await response.Content.ReadAsStringAsync();
        //    // Десериализация JSON в объект Ticker
        //    return JsonConvert.DeserializeObject<Ticker>(json);
        //}
    }
}