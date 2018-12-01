using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
namespace Nop.Plugin.Payments.PaySubs
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Callback
            routeBuilder.MapRoute("Plugin.Payments.PaySubs.Callback", "Plugin/PaySubsPayment/Callback",
                new { controller = "PaySubsPayment", action = "Callback" });

            routeBuilder.MapRoute("Plugin.Payments.PaySubs.CancelCallback", "Plugin/PaySubsPayment/CancelCallback",
                new { controller = "PaySubsPayment", action = "CancelCallback" });

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
