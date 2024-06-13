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
        public static void SendSubscribeOfferWindow() => AppMetrica.ReportEvent("Subscribe Offer Window (Unsigned user)");        
        public static void SendHelloWindow() => AppMetrica.ReportEvent("Hello Window (Signed user)");        
        public static void SendEnterPhoneWindow() => AppMetrica.ReportEvent("Enter Phone Window");        
        public static void SendOnEnteredPhoneWindow() => AppMetrica.ReportEvent("On Entered Phone");        
        public static void SendEnterOtpCodeWindow() => AppMetrica.ReportEvent("Enter Otp Code Window");        
        public static void SendOnEnteredOtpCodeWindow() => AppMetrica.ReportEvent("On Entered Otp Code");        
        public static void SendPayWallWindow() => AppMetrica.ReportEvent("PayWall Window");        
        public static void SendPayWallRedirect() => AppMetrica.ReportEvent("PayWall Redirect");        
        public static void SendFirstOpen() => AppMetrica.ReportEvent("First Open Game");        
        public static void SendSubscribeProfileWindow() => AppMetrica.ReportEvent("Subscribe Profile Window");        
        public static void SendSubscribeProfileRemote() => AppMetrica.ReportEvent("Subscribe Profile Remote");        
    }
}
