using UIClient.Infrastructure.Command;
using UIClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UIClient.Model.Config;
using UIClient.Infrastructure.Command.Base;
using System.Runtime.InteropServices;
using UIClient.Infrastructure.Controls;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Net;

namespace UIClient.ViewModel
{
    public class GamePageViewModel : Base.ViewModelBase
    {
        #region Properties
        #region Core Core : ядро
        private Core _Core;
        /// <summary>ядро</summary>
        public Core Core
        {
            get { return _Core; }
            set { Set(ref _Core, value); }
        }
        #endregion
        #endregion

        #region Commands
        #region UpdateConfigCommand : обновить конфигурацию
        /// <summary>обновить конфигурацию</summary>
        public ICommand UpdateConfigCommand { get; }
        private bool CanUpdateConfigCommandExecute(object p) => true;
        private void OnUpdateConfigCommandExecuted(object p)
        {
            //var config = App.Host.Services.GetRequiredService<AppConfig>();
            //ConditionsText = GetConditionsText(config);
            //Task.Run(new Action(() =>
            //{
            //    unsafe
            //    {
            //        Core.ICore.set_sleep(config.SleepMin, config.SleepMax);

            //        GCHandle handle_items = GCHandle.Alloc(config.ConditionsItem?.ToArray(), GCHandleType.Pinned);
            //        var _ptr_items = (Core.condition_item*)handle_items.AddrOfPinnedObject();
            //        Core.ICore.set_item_find(_ptr_items, config.ConditionsItem?.Count ?? 0);
            //        handle_items.Free();

            //        GCHandle handle_single = GCHandle.Alloc(config.ConditionsSingle?.ToArray(), GCHandleType.Pinned);
            //        GCHandle handle_double = GCHandle.Alloc(config.ConditionsDouble?.ToArray(), GCHandleType.Pinned);
            //        var _ptr_single = (Core.condition_single*)handle_single.AddrOfPinnedObject();
            //        var _ptr_double = (Core.condition_double*)handle_double.AddrOfPinnedObject();
            //        Core.ICore.set_stat_find(_ptr_single, config.ConditionsSingle?.Count ?? 0, _ptr_double, config.ConditionsDouble?.Count ?? 0);
            //        handle_single.Free();
            //        handle_double.Free();
            //    }
            //})).Wait();
        }
        #endregion
        //..
        #endregion

        public GamePageViewModel()
        {
            #region Properties
            Core = App.Core;
            //OnUpdateConfigCommandExecuted(null);

            #endregion

            #region Commands
            UpdateConfigCommand = new LambdaCommand(OnUpdateConfigCommandExecuted, CanUpdateConfigCommandExecute);
            //..
            #endregion
        }

    }
}
