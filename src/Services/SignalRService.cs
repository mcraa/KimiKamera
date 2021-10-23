using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

public class SignalRService : IStatusNotifier
{
    private HubConnection _connection;

    private string _url;
    private readonly ILogger _logger;

    public SignalRService(string url, ILogger logger)
    {
        _url = url;
        _logger = logger;
    }

    public SignalRService Connect()
    {
        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_url))
                .WithAutomaticReconnect()
                .Build();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return this;
    }

    public async Task SetAvailableAsync()
    {
        await SetStatus("Available", "green");
    }

    public async Task SetBusyAsync()
    {
        await SetStatus("Camera busy", "red");
    }

    public async Task SetInTransitionAsync()
    {
        await SetStatus("Changing state", "yellow");
    }

    public async Task SetStatus(string message, string color)
    {
        if (_connection.State == HubConnectionState.Disconnected) await _connection.StartAsync();
        await _connection.SendAsync("SendMessage", message, color);
    }

    public void Dispose()
    {
        if (_connection != null && _connection.State == HubConnectionState.Connected)
        {
            _connection.StopAsync();
            _connection.DisposeAsync();
        }
    }

    public async Task<bool> IsConnected()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State != HubConnectionState.Connected)
                {
                    await _connection.StartAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        return false;
    }
}