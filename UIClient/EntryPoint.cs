using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using UIClient.Model;
using UIClient.Model.Config;
using static UIClient.Model.Core;

namespace UIClient
{
    public class EntryPoint
    {
        public const string config_path = "AppConfig.json";

        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host_builder = Host.CreateDefaultBuilder(args);
            host_builder.UseContentRoot(Environment.CurrentDirectory);
            host_builder.ConfigureAppConfiguration((host, cfg) =>
            {
                cfg.SetBasePath(Environment.CurrentDirectory);
                cfg.AddJsonFile(config_path, false, true);
                cfg.AddEnvironmentVariables();
                if (args != null) cfg.AddCommandLine(args);
                cfg.Build();
            });

            host_builder.ConfigureServices(App.ConfigureServices);
            return host_builder;
        }
    }
}
