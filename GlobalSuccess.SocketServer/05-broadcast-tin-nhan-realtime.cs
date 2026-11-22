using System.Net.Sockets;
using System.Threading.Channels;

internal sealed class ChatRooms
{
    private readonly Dictionary<ChatClient, string> members = new();

    public void Join(ChatClient client, string roomId, byte[] reply)
    {
        lock (members)
        {
            members[client] = roomId;
            client.Send(reply);
        }
    }

    public bool Broadcast(ChatClient sender, string roomId, byte[] message)
    {
        lock (members)
        {
            if (!members.TryGetValue(sender, out string? joinedRoom) || joinedRoom != roomId)
            {
                return false;
            }

            foreach (var (client, currentRoom) in members)
            {
                if (currentRoom == roomId)
                {
                    client.Send(message);
                }
            }

            return true;
        }
    }

    public void Leave(ChatClient client)
    {
        lock (members)
        {
            members.Remove(client);
        }
    }
}

internal sealed class ChatClient(TcpClient socket)
{
    private readonly Channel<byte[]> outgoing = Channel.CreateBounded<byte[]>(
        new BoundedChannelOptions(32) { SingleReader = true });

    // Một tác vụ ghi cho mỗi client để các frame không bị xen byte.
    public void Send(byte[] frame)
    {
        if (!outgoing.Writer.TryWrite(frame))
        {
            socket.Dispose();
        }
    }

    public async Task WriteAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (byte[] frame in outgoing.Reader.ReadAllAsync(cancellationToken))
            {
                await stream.WriteAsync(frame, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Phiên đang đóng.
        }
        catch (IOException)
        {
            socket.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Socket đã đóng.
        }
    }
}
