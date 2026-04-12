namespace Core.Application.Services.Interfaces
{
    using Core.Domain;

    public interface IPushService
    {
        Task RegisterDevice(Guid userId, string fcmToken, DeviceType type);
        Task SendPushToAllCustomerDevices(Guid toUser, string title, string description);
        Task SendPushToEmployee(string title, string description);

    }
}
