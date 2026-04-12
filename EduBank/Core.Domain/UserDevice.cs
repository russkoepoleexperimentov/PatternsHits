
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public enum DeviceType
    {
        Customer = 0,
        Employee
    }

    // устройство пользователя
    public class UserDevice
    {
        [Key] public Guid Id { get; set; }  
        public Guid UserId { get; set; }
        public string? FcmToken { get; set; }
        public DeviceType Type { get; set; }
    }
}
