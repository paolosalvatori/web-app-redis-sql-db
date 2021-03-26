using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.ApplicationInsights.Telemetry.Services;

namespace ProductStore
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ResolverConfig.ConfigureApi(GlobalConfiguration.Configuration);
            ServerAnalytics.Start("d48ea1d5-718f-4c10-940a-0f5ca339b4b5");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            ServerAnalytics.BeginRequest();
            ServerAnalytics.CurrentRequest.LogEvent(Request.Url.AbsolutePath);
        }
    }
}