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

            //System.Web.Mvc.DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            //GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
            container.RegisterType<IExcelService, ExcelService>();
            container.RegisterType<IPeerCodeReviewService, PeerCodeReviewService>();
            container.RegisterType<ITeamFoundationService, TeamFoundationService>();
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            //RegisterTypes(container);
        }


        //public static void RegisterTypes(IUnityContainer container)
        //{
        //    ComponentLoader.LoadContainer(container, ".\\bin", "TheWork.dll");
        //    ComponentLoader.LoadContainer(container, ".\\bin", "BusinessServices.dll");
        //    ComponentLoader.LoadContainer(container, ".\\bin", "DataModel.dll");
        //}
    }
}