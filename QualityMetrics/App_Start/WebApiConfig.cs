using ExternalServices.Interfaces;
using ExternalServices.Implementations;
using BusinessLayer.Implementations;
using BusinessLayer.Interfaces;
using System.Web.Http;
using Unity;
using System.Web.Mvc;
using QualityMetrics.Controllers;

namespace QualityMetrics
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
