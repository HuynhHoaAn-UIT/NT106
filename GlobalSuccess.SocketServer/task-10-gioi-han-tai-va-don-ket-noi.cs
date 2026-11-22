using System.Net.Sockets;

internal static class SocketLimits
{
    public const int MaxClients = 64;
    public static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(30);

    public static async ValueTask<int> ReadFrameStartAsync(
        NetworkStream stream, byte[] length, bool uploading, CancellationToken cancellationToken)
    {
        if (!uploading)
        {
            return await stream.ReadAsync(length.AsMemory(0, 1), cancellationToken);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(FrameTimeout);
        return await stream.ReadAsync(length.AsMemory(0, 1), timeout.Token);
    }
}
