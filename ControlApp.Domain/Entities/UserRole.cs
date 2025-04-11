

namespace ControlApp.Domain.Entities
{
    public class UserToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? DeviceInfo { get; set; } 
    }
}