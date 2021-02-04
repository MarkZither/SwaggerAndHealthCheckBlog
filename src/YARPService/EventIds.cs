using Microsoft.Extensions.Logging;

namespace YARPService
{
    public class EventIds
    {
        public static readonly EventId NoAvailableDestinations = new EventId(1, "NoAvailableDestinations");
        public static readonly EventId TestWarning = new EventId(2, "TestWarning");
    }
}
