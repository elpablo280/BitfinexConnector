using BitfinexConnector;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitfinexConnector.Interface;

public class WebSocketClient : IWebSocket
{
    private readonly ClientWebSocket _webSocket;
    private readonly Uri _uri = new Uri("wss://api.bitfinex.com/ws/2");

    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    public event Action<Candle> CandleSeriesProcessing;

    public WebSocketClient()
    {
        _webSocket = new ClientWebSocket();
        ConnectAsync();
    }

    public async Task ConnectAsync()
    {
        await _webSocket.ConnectAsync(_uri, CancellationToken.None);
        _ = ReceiveMessagesAsync();
    }

    private async Task SendMessageAsync(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            HandleMessage(message);
        }
    }

    private void HandleMessage(string message)
    {
        var jsonObject = JObject.Parse(message);
        if (jsonObject.ContainsKey("event"))
        {
            var eventType = jsonObject["event"].ToString();
            if (eventType == "te") // "te" - trade event
            {
                var tradeData = jsonObject[1]; // Данные о трейде находятся во втором элементе массива
                var trade = JsonConvert.DeserializeObject<Trade>(tradeData.ToString());
                if (trade is not null && trade.Amount > 0m)
                {
                    NewBuyTrade?.Invoke(trade);
                }
                else if (trade is not null && trade.Amount < 0m)
                {
                    NewSellTrade?.Invoke(trade);
                }
            }
            else if (eventType == "candle")
            {
                var candleData = jsonObject[1]; // Данные о свече находятся во втором элементе массива
                var candle = JsonConvert.DeserializeObject<Candle>(candleData.ToString());
                CandleSeriesProcessing?.Invoke(candle);
            }
        }
    }

    public async Task SubscribeTradesAsync(string pair, int maxCount = 100)
    {
        var subscribeMessage = $"{{\"event\":\"subscribe\",\"channel\":\"trades\",\"symbol\":\"{pair}\",\"len\":\"{maxCount}\"}}";
        await SendMessageAsync(subscribeMessage);
    }

    public async Task SubscribeCandlesAsync(string pair, string period, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
    {
        string subscribeMessage = string.Empty;
        if (to is not null)
        {
            subscribeMessage += $"{{\"event\":\"subscribe\",\"channel\":\"candles\",\"key\":\"trade:{period}:{pair}a{count}:p{from?.ToUnixTimeSeconds()}:p{to?.ToUnixTimeSeconds()}\"}}";
        }
        else
        {
            subscribeMessage = $"{{\"event\":\"subscribe\",\"channel\":\"candles\",\"key\":\"trade:{period}:{pair}\"}}";
        }
        await SendMessageAsync(subscribeMessage);
    }

    public async Task UnsubscribeTradesAsync(string pair)
    {
        var unsubscribeMessage = $"{{\"event\":\"unsubscribe\",\"channel\":\"trades\",\"symbol\":\"{pair}\"}}";
        await SendMessageAsync(unsubscribeMessage);
    }

    public async Task UnsubscribeCandlesAsync(string pair)
    {
        var unsubscribeMessage = $"{{\"event\":\"unsubscribe\",\"channel\":\"candles\",\"key\":\"trade:{pair}\"}}";
        await SendMessageAsync(unsubscribeMessage);
    }
}