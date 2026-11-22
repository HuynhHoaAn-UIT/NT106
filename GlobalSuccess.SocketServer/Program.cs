using System.Net;
using System.Net.Sockets;

const int port = 8085;
using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

var listener = new TcpListener(IPAddress.Loopback, port);
var router = new ChatRouter();
var clientTasks = new List<Task>();

try
{
    listener.Start();
    Console.WriteLine($"Listening on {listener.LocalEndpoint}");

    while (!shutdown.IsCancellationRequested)
    {
        TcpClient client = await listener.AcceptTcpClientAsync(shutdown.Token);
        clientTasks.RemoveAll(task => task.IsCompleted);
        clientTasks.Add(router.HandleClientAsync(client, shutdown.Token));
    }
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    // Ctrl+C yêu cầu dừng server.
}
finally
{
    listener.Stop();
    shutdown.Cancel();
    await Task.WhenAll(clientTasks);
}
