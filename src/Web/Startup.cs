using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using Foundatio.Jobs;
using Foundatio.Logging;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Samples.Core;
using Samples.Core.Dependency;
using Samples.Core.Extensions;
using Samples.Core.Jobs;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

[assembly: OwinStartup(typeof(Samples.Web.Startup))]
namespace Samples.Web {
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);

            var container = CreateContainer();
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            var resolver = new SimpleInjectorSignalRDependencyResolver(container);
            app.MapSignalR(new HubConfiguration { Resolver = resolver, EnableDetailedErrors = true });

            VerifyContainer(container);

            app.UseWebApi(GlobalConfiguration.Configuration);

            
            JobRunner.RunContinuousAsync<ValuesPostJob>();
            JobRunner.RunContinuousAsync<WorkItemJob>(instanceCount: 2);
        }

        public static Container CreateContainer(bool includeInsulation = true) {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.Options.ResolveUnregisteredCollections = true;

            container.RegisterPackage<Bootstrapper>();

            if (!includeInsulation)
                return container;

            Assembly insulationAssembly = null;
            try {
                insulationAssembly = Assembly.Load("Samples.Insulation");
            } catch (Exception ex) {
                Logger.Error().Message("Unable to load the insulation assembly.").Exception(ex).Write();
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