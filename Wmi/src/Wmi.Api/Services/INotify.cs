using Wmi.Api.Models;

namespace Wmi.Api.Services;

public interface INotify
{
    void Notify(string buyerId, string message);
}