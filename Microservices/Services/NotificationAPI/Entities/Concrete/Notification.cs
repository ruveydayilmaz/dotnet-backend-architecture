using Core.Entities;

namespace Entities.Concrete
{
    public class Notification : IEntity
    {
        public NotificationContent Content { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Type { get; set; }
    }

    public class NotificationContent
    {
        public TextContent Text { get; set; }
        public string Link { get; set; }
        public int Id { get; set; }
    }

    public class TextContent
    {
        public string Username { get; set; }
        public string Message { get; set; }
    }
}
