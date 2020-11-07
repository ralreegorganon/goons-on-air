using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.ViewModels;
using OfficeOpenXml;
using Serilog;

namespace GoonsOnAir
{
    public class AppBootstrapper : AutofacBootstrapper
    {
        public AppBootstrapper()
        {
            Initialize();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt")
                .CreateLogger();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .PropertiesAutowired();

            builder.RegisterType<GlobalCredentials>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<OnAirClient>()
                .AsImplementedInterfaces()
                .SingleInstance();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var windowSettings = new Dictionary<string, object> {
                {
                    "WindowStartupLocation", WindowStartupLocation.CenterScreen
                }
            };

            DisplayRootViewFor<ShellViewModel>(windowSettings);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An unhandled exception occurred");
            e.Handled = true;
            MessageBox.Show(e.Exception.InnerException?.Message ?? e.Exception.Message);
        }
    }
}