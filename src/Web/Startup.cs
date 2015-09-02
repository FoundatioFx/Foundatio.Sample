using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Samples.Core;
using Samples.Core.Dependency;
using Samples.Core.Extensions;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

[assembly: OwinStartup(typeof(Samples.Web.Startup))]
namespace Samples.Web {
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);

            var container = CreateContainer();
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            VerifyContainer(container);

            app.UseWebApi(GlobalConfiguration.Configuration);

            var resolver = new SimpleInjectorSignalRDependencyResolver(container);
            app.MapSignalR(new HubConfiguration { Resolver = resolver });
        }

        public static Container CreateContainer(bool includeInsulation = true) {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            container.RegisterPackage<Bootstrapper>();

            if (!includeInsulation)
                return container;

            Assembly insulationAssembly = null;
            try {
                insulationAssembly = Assembly.Load("Samples.Insulation");
            } catch (Exception ex) {
                //Log.Error().Message("Unable to load the insulation assembly.").Exception(ex).Write();
            }

            if (insulationAssembly != null)
                container.RegisterPackages(new[] { insulationAssembly });

            return container;
        }

       private static void VerifyContainer(Container container) {
            try {
                container.Verify();
            } catch (Exception ex) {
                var tempEx = ex;
                while (!(tempEx is ReflectionTypeLoadException)) {
                    if (tempEx.InnerException == null)
                        break;
                    tempEx = tempEx.InnerException;
                }

                var typeLoadException = tempEx as ReflectionTypeLoadException;
                if (typeLoadException != null) {
                    foreach (var loaderEx in typeLoadException.LoaderExceptions)
                        Debug.WriteLine(loaderEx.Message);
                }

                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}