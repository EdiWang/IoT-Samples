using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.NotificationHubs;

namespace AzureFireAlarm.Web.Controllers
{
    public class FireAlarmController : Controller
    {
        public string HubName => "RPi-FireAlarm-Notification";

        public string ConnectionString => "Endpoint=sb://rpi-firealarm-notification-namespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=M3b+/EU3SplIwdbWQjkczv7eJyiWlAvWu4AIbTbiJjE=";

        public async Task<ActionResult> SendAlarm(bool isFire)
        {
            string msg = isFire ? "Oh! There's a fire!" : "Fire is out.";
            await SendNotificationAsync(msg);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private async Task SendNotificationAsync(string message)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(ConnectionString, HubName);
            var toast = $@"<toast><visual><binding template=""ToastText01""><text id=""1"">{message}</text></binding></visual></toast>";
            await hub.SendWindowsNativeNotificationAsync(toast);
        }
    }
}