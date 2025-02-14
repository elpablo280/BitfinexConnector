using BitfinexConnector.Interface;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using WebSocket = WebSocket4Net.WebSocket;

namespace BitfinexConnector.Clients
{
    public class WebSocketClient : IWebSocket
    {
        private WebSocket _webSocket;
        private const string BaseUrl = "wss://api.bitfinex.com/ws/2";

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public WebSocketClient()
        {
            _webSocket = new WebSocket(BaseUrl);
            _webSocket.Opened += WebSocket_Opened;
            _webSocket.MessageReceived += WebSocket_MessageReceived;
            _webSocket.Open();
        }

        private void WebSocket_Opened(object sender, EventArgs e)
        {
        }

        private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var message = JArray.Parse(e.Message);
            if (message.Count > 1 && message[1].Type == JTokenType.String)
            {
                var channel = message[1].ToString();
                if (channel.Contains("trade"))
                {
                    var trade = ParseTrade(message[2]);
                    if (trade.Side == "buy")
                    {
                        NewBuyTrade?.Invoke(trade);
                    }
                    else
                    {
                        NewSellTrade?.Invoke(trade);
                    }
                }
                else if (channel.Contains("candle"))
                {
                    var candle = ParseCandle(message[2]);
                    CandleSeriesProcessing?.Invoke(candle);
                }
            }
        }

        public Task SubscribeTradesAsync(string pair, int maxCount)
        {
            var message = new JArray { 0, "subscribe", "trades", $"t{pair}", maxCount };
            _webSocket.Send(message.ToString());
            return Task.CompletedTask;
        }

        public Task UnsubscribeTradesAsync(string pair)
        {
            var message = new JArray { 0, "unsubscribe", "trades", $"t{pair}" };
            _webSocket.Send(message.ToString());
            return Task.CompletedTask;
        }

        public Task SubscribeCandlesAsync(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            var message = new JArray { 0, "subscribe", "candles", $"trade:{periodInSec}:t{pair}" };
            _webSocket.Send(message.ToString());
            return Task.CompletedTask;
        }

        public Task UnsubscribeCandlesAsync(string pair)
        {
            var message = new JArray { 0, "unsubscribe", "candles", $"trade:t{pair}" };
            _webSocket.Send(message.ToString());
            return Task.CompletedTask;
        }

        private Trade ParseTrade(JToken token)
        {
            var trade = new Trade
            {
                Id = token[0].ToString(),
                Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(token[1].ToString())),
                Amount = decimal.Parse(token[2].ToString()),
                Price = decimal.Parse(token[3].ToString()),
                Side = token[4].ToString()
            };
            return trade;
        }

        private Candle ParseCandle(JToken token)
        {
            var candle = new Candle
            {
                OpenTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(token[0].ToString())),
                OpenPrice = decimal.Parse(token[1].ToString()),
                ClosePrice = decimal.Parse(token[2].ToString()),
                HighPrice = decimal.Parse(token[3].ToString()),
                LowPrice = decimal.Parse(token[4].ToString()),
                TotalVolume = decimal.Parse(token[5].ToString())
            };
            return candle;
        }
    }
}