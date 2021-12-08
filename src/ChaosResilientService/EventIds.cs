using Microsoft.Extensions.Logging;

namespace ChaosResilientService
{
    public static class EventIds
    {
        public static readonly EventId DBReadFailure = new EventId(1, "DBReadFailure");
        public static readonly EventId TestWarning = new EventId(2, "TestWarning");
    }
}
