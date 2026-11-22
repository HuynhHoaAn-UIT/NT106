using Azure;
using Azure.Communication.Email;

namespace GlobalSuccess.Api.Services;

public class EmailService
{
    private readonly EmailClient _client;
    private readonly string _sender;

    public EmailService(IConfiguration config)
    {
        var connectionString = config["AzureEmail:ConnectionString"]
            ?? throw new InvalidOperationException("Missing Azure Email connection string");

        _sender = config["AzureEmail:SenderAddress"]
            ?? throw new InvalidOperationException("Missing sender address");

        _client = new EmailClient(connectionString);
    }

    public async Task SendOtpAsync(string recipient, string otp)
    {
        var operation = await _client.SendAsync(
            WaitUntil.Completed,
            _sender,
            recipient,
            "GlobalSuccess - Mã xác thực OTP",
            $"Mã OTP của bạn là: {otp}. Mã có hiệu lực trong 5 phút.");

        if (!operation.HasCompleted)
            throw new InvalidOperationException("Email operation incomplete");
    }
}
