using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketAnalysisPlugin;
public class UniversalisClient : IDisposable
{
    private ClientWebSocket webSocket;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private const string UniversalisWebSocketUrl = "wss://universalis.app/api/ws";

    public event EventHandler<MarketDataEventArgs> OnMarketDataReceived;
    public event EventHandler<EventArgs> OnConnected;
    public event EventHandler<EventArgs> OnDisconnected;

    public bool IsConnected => webSocket?.State == WebSocketState.Open;

    public UniversalisClient()
    {
        webSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync()
    {
        try
        {
            if (webSocket.State != WebSocketState.None && webSocket.State != WebSocketState.Closed)
            {
                await DisconnectAsync();
                webSocket = new ClientWebSocket();
            }

            await webSocket.ConnectAsync(new Uri(UniversalisWebSocketUrl), cancellationTokenSource.Token);
            OnConnected?.Invoke(this, EventArgs.Empty);

            // Start listening for messages
            _ = Task.Run(ReceiveMessagesAsync);
        }
        catch (Exception)
        {
            // Error handling without logging
        }
    }

    public async Task DisconnectAsync()
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                // Error handling without logging
            }
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[8192];

        try
        {
            while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                var messageBuffer = new List<byte>();

                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                    messageBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                    ProcessMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }
        catch (WebSocketException)
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
        catch (TaskCanceledException)
        {
            // Normal cancellation, no need to log
        }
        catch (Exception)
        {
            // Error handling without logging
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            // Just trigger the event with the message data
            OnMarketDataReceived?.Invoke(this, new MarketDataEventArgs
            {
                RawMessage = message
            });
        }
        catch (Exception)
        {
            // Error handling without logging
        }
    }

    public void Dispose()
    {
        cancellationTokenSource.Cancel();

        if (webSocket.State == WebSocketState.Open)
        {
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing client", CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        webSocket.Dispose();
        cancellationTokenSource.Dispose();
    }

    public async Task SubscribeToItemAsync(uint itemId)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        // Format subscription message for Universalis
        string message = $"{{ \"event\": \"subscribe\", \"channel\": \"listings/add/{itemId}\" }}";

        // Send the message
        await SendMessageAsync(message);
    }

    public async Task UnsubscribeFromItemAsync(uint itemId)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        // Format unsubscription message
        string message = $"{{ \"event\": \"unsubscribe\", \"channel\": \"listings/add/{itemId}\" }}";

        // Send the message
        await SendMessageAsync(message);
    }

    public async Task SendMessageAsync(string message)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        var messageBytes = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(
            new ArraySegment<byte>(messageBytes),
            WebSocketMessageType.Text,
            true,
            cancellationTokenSource.Token);
    }
}

// Simple event args for market data
public class MarketDataEventArgs : EventArgs
{
    public string RawMessage { get; set; }
}
