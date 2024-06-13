﻿using Io.AppMetrica;

namespace Agava.Wink
{
    public static class AnalyticsWinkService
    {
       /// <summary>
       /// Auditory
       /// </summary>
        public static void SendSanId(string sanId) => AppMetrica.ReportEvent("SanId", sanId);
        public static void SendSex(string sex) => AppMetrica.ReportEvent("Sex", sex);//N/A
        public static void SendAge(string age) => AppMetrica.ReportEvent("Age", age);//N/A

        /// <summary>
        /// User data
        /// </summary>
        public static void SendHasActiveAccountNewUser(bool hasActiveAcc) => AppMetrica.ReportEvent("Has Active Account New User", hasActiveAcc.ToString());
        public static void SendHasActiveAccountUser(bool hasActiveAcc) => AppMetrica.ReportEvent("Has Active Account Regular User", hasActiveAcc.ToString());

        /// <summary>
        /// Retention
        /// </summary>
        public static void SendAverageSessionLength(int time) => AppMetrica.ReportEvent("Average Session Length(Minute)", time.ToString());

        /// <summary>
        /// First time events
        /// </summary>
        public static void SendFirstOpen() => AppMetrica.ReportEvent("First Open App");        
        public static void SendSubscribeOfferWindow() => AppMetrica.ReportEvent("Subscribe Offer Window (Unsigned user)");        
        public static void SendHelloWindow() => AppMetrica.ReportEvent("Hello Window (Signed user)");        
    }
}
