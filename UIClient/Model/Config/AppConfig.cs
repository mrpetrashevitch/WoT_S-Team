using Microsoft.Extensions.Options;
using System.Collections.Generic;
using static UIClient.Model.Core;

namespace UIClient.Model.Config
{
    public class AppConfig
    {
        public AppConfig(IOptionsMonitor<AppConfigJson> settings)
        {
            Update(settings.CurrentValue);
            settings.OnChange(OnUpdate);
        }

        private void OnUpdate(AppConfigJson settings) => Update(settings);

        private void Update(AppConfigJson settings)
        {
            NetConfig = settings;
        }

        public AppConfigJson NetConfig { get; set; }
    }
}
