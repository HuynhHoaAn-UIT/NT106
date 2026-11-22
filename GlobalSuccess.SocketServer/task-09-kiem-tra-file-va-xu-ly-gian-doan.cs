using System.Text.Json;

internal sealed partial class FileUpload
{
    private static (string Name, long Size, byte[] Hash) ReadMetadata(JsonElement body)
    {
        string? name = body.GetProperty("fileName").GetString();
        long size = body.GetProperty("size").GetInt64();
        string? sha256 = body.GetProperty("sha256").GetString();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 128 || name != name.Trim() ||
            name is "." or ".." || name.Any(character => char.IsControl(character) ||
                "<>:\"/\\|?*".Contains(character)) ||
            size is < 0 or > FileProtocol.MaxFileSize || sha256?.Length != 64)
        {
            throw new FormatException("Invalid file metadata.");
        }

        return (name, size, Convert.FromHexString(sha256));
    }

    private static byte[] ReadChunk(JsonElement body, long remaining)
    {
        string? encoded = body.GetProperty("data").GetString();
        const int maxEncodedSize = ((FileProtocol.MaxChunkSize + 2) / 3) * 4;
        if (string.IsNullOrEmpty(encoded) || encoded.Length > maxEncodedSize)
        {
            throw new FormatException("Invalid file chunk.");
        }

        byte[] data = Convert.FromBase64String(encoded);
        if (data.Length == 0 || data.Length > FileProtocol.MaxChunkSize || data.Length > remaining)
        {
            throw new FormatException("Invalid file chunk size.");
        }

        return data;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (output is not null)
            {
                await output.DisposeAsync();
            }
        }
        catch (IOException error)
        {
            Console.WriteLine($"Upload close error: {error.Message}");
        }
        finally
        {
            output = null;
            hash?.Dispose();
            hash = null;
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    Console.WriteLine($"Upload cleanup error: {error.Message}");
                }

                temporaryPath = null;
            }
        }
    }
}
