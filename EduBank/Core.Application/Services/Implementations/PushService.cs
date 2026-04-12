
using Core.Application.Services.Interfaces;
using Core.Domain;
using Core.Infrastructure;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services.Implementations
{
    public class PushService : IPushService
    {
        private readonly ILogger<PushService> _logger;
        private readonly CoreDbContext _context;

        public PushService(CoreDbContext context, ILogger<PushService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegisterDevice(Guid userId, string fcmToken, DeviceType type)
        {
            var existingDevice = await _context.Devices.FirstOrDefaultAsync(x => x.FcmToken == fcmToken);

            if (existingDevice != null)
            {
                existingDevice.Type = type;
                existingDevice.UserId = userId;
                _context.Devices.Update(existingDevice);
                await _context.SaveChangesAsync();
            }
            else
            {

                var device = new UserDevice()
                {
                    UserId = userId,
                    FcmToken = fcmToken,
                    Type = type
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SendPushToAllCustomerDevices(Guid toUser, string title, string description)
        {
            var devices = await _context.Devices.Where(x => x.UserId == toUser && x.Type == DeviceType.Customer).ToListAsync();

            foreach (var device in devices)
            {
                await Send(device, title, description);
            }
        }

        public async Task SendPushToEmployee(string title, string description)
        {
            var devices = await _context.Devices.Where(x => x.Type == DeviceType.Employee).ToListAsync();

            foreach (var device in devices)
            {
                await Send(device, title, description);
            }
        }

        private async Task Send(UserDevice device, string title, string description)
        {
            try
            {
                // Создаем сообщение
                var message = new Message()
                {
                    Token = device.FcmToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = description
                    },
                    // (Опционально) Для Android можно задать дополнительные параметры
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    }
                };

                // Отправляем сообщение
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Отправлено сообщение {device.UserId} ({device.Type}): {response}");
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления");
            }
        }
    }
}
