using System.Diagnostics;

namespace Wmi.Api.Services;

public class MockNotify(ILogger<MockNotify> logger):INotify
{
    public void Notify(string buyerId, string message)
    {
        logger.LogInformation($"Sending message...{buyerId} - {message}");
    }
}