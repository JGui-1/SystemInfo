using System.Collections.Generic;

namespace SystemInfo.Configuration
{
    public static class UselessServices
    {
        // Lista conservadora (varia por edição do Windows). Revise antes de aplicar.
        public static readonly IEnumerable<string> Names = new []
        {
            "Fax",
            "XblGameSave",
            "XboxGipSvc",
            "XblAuthManager",
            "XboxNetApiSvc",
            "RemoteRegistry",
            "WpcMonSvc", // Parental Controls
            "MapsBroker", // Download de mapas offline
            "SharedAccess", // ICS - Internet Connection Sharing (se não usar)
            "DiagTrack" // Connected User Experiences and Telemetry (telemetria)
        };
    }
}
