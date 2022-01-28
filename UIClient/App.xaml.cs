using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UIClient.Model;
using UIClient.Model.Config;
using UIClient.View.Pages;
using UIClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static UIClient.Model.Core;

namespace UIClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IHost __Host;
        public static IHost Host => __Host ??= EntryPoint.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        private static Core __Core;
        public static Core Core => __Core ??= new Core();

        internal static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<AppConfigJson>(host.Configuration.GetSection(nameof(AppConfigJson)));
            services.AddSingleton<AppConfig>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LoadPageViewModel>();
            services.AddSingleton<LoadPage>();
            services.AddSingleton<GamePageViewModel>();
            services.AddSingleton<GamePage>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var host = Host;
            await host.StartAsync().ConfigureAwait(false);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var host = Host;
            await host.StopAsync().ConfigureAwait(false);
            host.Dispose();
            __Host = null;
        }
    }
}

