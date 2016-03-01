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
            var loggerFactory = Settings.Current.GetLoggerFactory();
            var logger = loggerFactory.CreateLogger("AppBuilder");

            ConfigureAuth(app);

            var container = CreateContainer(loggerFactory, logger);
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            var resolver = new SimpleInjectorSignalRDependencyResolver(container);

            if (Settings.Current.EnableRedis)
                resolver.UseRedis(new RedisScaleoutConfiguration(Settings.Current.RedisConnectionString, "sample.signalr"));

            app.MapSignalR(new HubConfiguration {
                Resolver = resolver,
                EnableDetailedErrors = true
            });

            VerifyContainer(container);

            app.UseWebApi(GlobalConfiguration.Configuration);

            RunJobs(loggerFactory);
        }

        public static Container CreateContainer(ILoggerFactory loggerFactory, ILogger logger, bool includeInsulation = true) {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            Bootstrapper.RegisterServices(container, loggerFactory);

            if (!includeInsulation)
                return container;

            Assembly insulationAssembly = null;
            try {
                insulationAssembly = Assembly.Load("Samples.Insulation");
            } catch (Exception ex) {
                logger.Error().Message("Unable to load the insulation assembly.").Exception(ex).Write();
            }

            if (insulationAssembly != null) {
                var bootstrapperType = insulationAssembly.GetType("Samples.Insulation.Bootstrapper");
                if (bootstrapperType == null)
                    return container;

                bootstrapperType.GetMethod("RegisterServices", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { container, loggerFactory });
            }

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

        private static void RunJobs(LoggerFactory loggerFactory) {
            var jobRunner = new JobRunner(loggerFactory);
            jobRunner.RunContinuousAsync<ValuesPostJob>();
            jobRunner.RunContinuousAsync<WorkItemJob>(instanceCount: 2);
        }
    }
}