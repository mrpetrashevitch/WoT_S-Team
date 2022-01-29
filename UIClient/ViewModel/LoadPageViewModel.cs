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
using System.Threading;
using UIClient.View.Pages;

namespace UIClient.ViewModel
{
    class LoadPageViewModel : Base.ViewModelBase
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

        #region string UserName : имя пользователя
        private string _UserName;
        /// <summary>имя пользователя</summary>
        public string UserName
        {
            get { return _UserName; }
            set { Set(ref _UserName, value); }
        }
        #endregion

        #region string Pass : пароль
        private string _Pass = "";
        /// <summary>пароль</summary>
        public string Pass
        {
            get { return _Pass; }
            set { Set(ref _Pass, value); }
        }
        #endregion

        #region string GameName : имя игры
        private string _GameName;
        /// <summary>имя игры</summary>
        public string GameName
        {
            get { return _GameName; }
            set { Set(ref _GameName, value); }
        }
        #endregion

        #region int PlayersMax : максимальное количество игроков
        private int _PlayersMax;
        /// <summary>максимальное количество игроков</summary>
        public int PlayersMax
        {
            get { return _PlayersMax; }
            set { Set(ref _PlayersMax, value); }
        }
        #endregion

        #region int? TurnMax : максимальное количество раундов
        private int? _TurnMax;
        /// <summary>максимальное количество раундов</summary>
        public int? TurnMax
        {
            get { return _TurnMax; }
            set { Set(ref _TurnMax, value); }
        }
        #endregion

        #region bool IsObserver : наблюдать
        private bool _IsObserver;
        /// <summary>наблюдать</summary>
        public bool IsObserver
        {
            get { return _IsObserver; }
            set { Set(ref _IsObserver, value); }
        }
        #endregion

        #region Visibility IsLoadVisible : summury
        private Visibility _IsLoadVisible;
        /// <summary>summury</summary>
        public Visibility IsLoadVisible
        {
            get { return _IsLoadVisible; }
            set { Set(ref _IsLoadVisible, value); }
        }
        #endregion

        #region Visibility IsControlVisible : summury
        private Visibility _IsControlVisible;
        /// <summary>summury</summary>
        public Visibility IsControlVisible
        {
            get { return _IsControlVisible; }
            set { Set(ref _IsControlVisible, value); }
        }
        #endregion

        //..
        #endregion

        #region Commands
        #region LoginCommand : установить имя пользователя
        /// <summary>установить имя пользователя</summary>
        public ICommand LoginCommand { get; }
        private bool CanLoginCommandExecute(object p)
        {
            if (!Core.Connected) return false;
            if (UserName == null || UserName.Length < 4) return false;
            if (GameName != null && GameName.Length == 0) return false;
            if (PlayersMax < 1 || PlayersMax > 3) return false;
            if (TurnMax != null && TurnMax < 1) return false;
            return true;
        }
        private async void OnLoginCommandExecuted(object p)
        {
            LoginCreate log = new LoginCreate();
            log.name = UserName;
            log.password = Pass;
            log.game = GameName;
            log.num_turns = TurnMax;
            log.num_players = PlayersMax;
            log.is_observer = IsObserver;

            var res = await Core.SendLoginAsync(log).ConfigureAwait(false);
            if (res != Result.OKEY)
            {
                Core.Log("Ошибка: " + res.ToString());
                return;
            }
            Core.Log("Авторизация выполнена");

            res = await Core.SendMapAsync().ConfigureAwait(false);
            if (res != Result.OKEY)
            {
                Core.Log("Ошибка: " + res.ToString());
                return;
            }

            Core.Log("Карта загружена");
            Core.MessageWait = "Ожидание хода...";

            App.Current.Dispatcher.Invoke(() =>
            {
                Core.Field.CreateField(Core.Map);
                var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
                main_page.SelectGamePage();
            });

            //wait players
            while (true)
            {
                res = await Core.SendGameStateAsync().ConfigureAwait(false);
                if (res == Result.OKEY)
                {
                    if (!Core.Field.Inited)
                    {
                        App.Current.Dispatcher.Invoke(() => {  Core.Field.CreateContent(Core.GameState); });

                        if (Core.Field.Inited)
                        {
                            PlayerEx curr_player;
                            if (Core.Field.players.TryGetValue(Core.Player.idx, out curr_player))
                                Core.TeamColor = curr_player.color;

                            res = await Core.SendGameStateAsync().ConfigureAwait(false);
                            Core.Log("Все игроки подключены");
                            break;
                        }
                    }
                }
                await Task.Delay(1000);
            }
        }
        #endregion

        //..
        #endregion

        public LoadPageViewModel()
        {
            #region Properties
            IsLoadVisible = Visibility.Hidden;
            Core = App.Core;
            PlayersMax = 2;
            //..
            #endregion

            #region Commands
            LoginCommand = new LambdaCommand(OnLoginCommandExecuted, CanLoginCommandExecute);
            //..
            #endregion
        }
    }
}
