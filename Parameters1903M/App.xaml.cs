using System;
using System.Reflection;
using System.Windows;
using log4net;

namespace Parameters1903M
{
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{sender.GetType()}{Environment.NewLine}{e.ExceptionObject}");
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Microsoft.Xaml.Behaviors"))
            {
                return Assembly.Load(AppResources.Microsoft_Xaml_Behaviors);
            }
            else if (args.Name.Contains("Newtonsoft.Json"))
            {
                return Assembly.Load(AppResources.Newtonsoft_Json);
            }
            else if (args.Name.Contains("OxyPlot.Wpf"))
            {
                return Assembly.Load(AppResources.OxyPlot_Wpf);
            }
            else if (args.Name.Contains("OxyPlot"))
            {
                return Assembly.Load(AppResources.OxyPlot);
            }
            else if (args.Name.Contains("PdfSharp-WPF"))
            {
                return Assembly.Load(AppResources.PdfSharp_WPF);
            }
            else if (args.Name.Contains("PdfSharp.Xps"))
            {
                return Assembly.Load(AppResources.PdfSharp_Xps);
            }
            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  Started Logging  =============        ");
            base.OnStartup(e);
        }
    }
}
