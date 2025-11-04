using System.Net;
using System.Net.Sockets;
using System.Text;
using EvoConnect.Common;

namespace EvoConnect.Server;

public class UdpServicePublisher : BackgroundService
{
    private const int Port = 11000; // Port for multicast
    private const string MulticastAddress = "224.0.0.1"; // Multicast address
    private UdpClient? _udpClient;
    private IPEndPoint? _multicastEndpoint;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _udpClient = new UdpClient();
        _udpClient.JoinMulticastGroup(IPAddress.Parse(MulticastAddress));
        _multicastEndpoint = new IPEndPoint(IPAddress.Parse(MulticastAddress), Port);
        
        var message = AppData.IsServer() ? "evoconnect-server" : "evoconnect-client";
        var data = Encoding.UTF8.GetBytes(message);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _udpClient.SendAsync(data, data.Length, _multicastEndpoint);
                await Task.Delay(100, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Normal cancellation, ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP broadcast error: {ex.Message}");
            }
        }
    }

    public override void Dispose()
    {
        _udpClient?.DropMulticastGroup(IPAddress.Parse(MulticastAddress));
        _udpClient?.Dispose();
        base.Dispose();
    }
}