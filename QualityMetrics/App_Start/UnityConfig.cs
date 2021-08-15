using BusinessLayer.Implementations;
using BusinessLayer.Interfaces;
using ExternalServices.Implementations;
using ExternalServices.Interfaces;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace QualityMetrics
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
            container.RegisterType<IExcelService, ExcelService>();
            container.RegisterType<IPeerCodeReviewService, PeerCodeReviewService>();
            container.RegisterType<ITeamFoundationService, TeamFoundationService>();
        }
    }
}