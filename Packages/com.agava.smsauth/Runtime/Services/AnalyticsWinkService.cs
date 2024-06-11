using Io.AppMetrica;

namespace Agava.Wink
{
    public static class AnalyticsWinkService
    {
       /// <summary>
       /// Auditory
       /// </summary>
        public static void SendSanId(string sanId) => AppMetrica.ReportEvent("SanId", sanId);
        public static void SendSex(string sex) => AppMetrica.ReportEvent("Sex", sex);
        public static void SendAge(string age) => AppMetrica.ReportEvent("Age", age);
        public static void SendLocation(string location) => AppMetrica.ReportEvent("Location", location);
        public static void SendOsType(string osType) => AppMetrica.ReportEvent("Os Type", osType);
        public static void SendOsVersion(string osVersion) => AppMetrica.ReportEvent("Os Version", osVersion);
        public static void SendDevice(string device) => AppMetrica.ReportEvent("Device", device);
        public static void SendAppVersion(string appVersion) => AppMetrica.ReportEvent("App Version", appVersion);

        /// <summary>
        /// User data
        /// </summary>
        public static void SendHasActiveAccount(bool hasActiveAcc) => AppMetrica.ReportEvent("Has Active Account", hasActiveAcc.ToString());

        /// <summary>
        /// Retention
        /// </summary>
        public static void SendTimespent(int time) => AppMetrica.ReportEvent("Timespent", time.ToString());
        public static void SendSessionTimespent(int time) => AppMetrica.ReportEvent("Session Timespent", time.ToString());
        public static void SendAverageSessionLength(int time) => AppMetrica.ReportEvent("Average Session Length", time.ToString());
        public static void SendAverageAppsOnUser(int count) => AppMetrica.ReportEvent("Average App`s On User", count.ToString());

        /// <summary>
        /// First time events
        /// </summary>
        public static void SendFirstOpen() => AppMetrica.ReportEvent("First Open App");        
        public static void SendSubscribeOfferWindow() => AppMetrica.ReportEvent("Subscribe Offer Window (Unsigned user)");        
        public static void SendHelloWindow() => AppMetrica.ReportEvent("Hello Window (Signed user)");        
    }
}
