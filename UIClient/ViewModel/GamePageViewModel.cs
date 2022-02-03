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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using UIClient.Model.Server;
using System.Windows.Media;
using UIClient.Model.Client;
using UIClient.Model.Client.Api;

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
        #region string ChatText : вписанный текст
        private string _ChatText;
        /// <summary>вписанный текст</summary>
        public string ChatText
        {
            get { return _ChatText; }
            set { Set(ref _ChatText, value); }
        }
        #endregion
        #region ObservableCollection<string> Chat : логи чата
        private ObservableCollection<string> _Chat;
        /// <summary>логи чата</summary>
        public ObservableCollection<string> Chat
        {
            get { return _Chat; }
            set { Set(ref _Chat, value); }
        }
        #endregion

        #region bool StepEnable : есть ли возможность ходить
        private bool _StepEnable;
        /// <summary>есть ли возможность ходить</summary>
        public bool StepEnable
        {
            get { return _StepEnable; }
            set
            {
                if (Set(ref _StepEnable, value))
                {
                    if (value) MessageWaitVisible = Visibility.Collapsed;
                    else MessageWaitVisible = Visibility.Visible;
                }
            }
        }
        #endregion
        #region string MessageWait : сообщение ожидания
        private string _MessageWait;
        /// <summary>сообщение ожидания</summary>
        public string MessageWait
        {
            get { return _MessageWait; }
            set { Set(ref _MessageWait, value); }
        }
        #endregion
        #region Visibility MessageWaitVisible : надпись ожидания
        private Visibility _MessageWaitVisible;
        /// <summary>надпись ожидания</summary>
        public Visibility MessageWaitVisible
        {
            get { return _MessageWaitVisible; }
            set { Set(ref _MessageWaitVisible, value); }
        }
        #endregion

        #region bool AIEnable : включить автоматическую игру
        private bool _AIEnable;
        /// <summary>включить автоматическую игру</summary>
        public bool AIEnable
        {
            get { return _AIEnable; }
            set { Set(ref _AIEnable, value); }
        }
        #endregion
        #region int TotalStep : сделано шагов
        private int _TotalStep;
        /// <summary>сделано шагов</summary>
        public int TotalStep
        {
            get { return _TotalStep; }
            set { Set(ref _TotalStep, value); }
        }
        #endregion    
        #region TaskCompletionSource<bool> EventWait : ожидание события от пользоваеля
        private TaskCompletionSource<bool> _EventWait;
        /// <summary>ожидание события от пользоваеля</summary>
        public TaskCompletionSource<bool> EventWait
        {
            get { return _EventWait; }
            set { _EventWait = value; }
        }
        #endregion

        #region Player Player : текущий игрок
        private Player _Player;
        /// <summary>текущий игрок</summary>
        public Player Player
        {
            get { return _Player; }
            set { Set(ref _Player, value); }
        }
        #endregion
        #region Brush TeamColor : цвет команды игрока
        private Brush _TeamColor;
        /// <summary>цвет команды игрока</summary>
        public Brush TeamColor
        {
            get { return _TeamColor; }
            set { Set(ref _TeamColor, value); }
        }
        #endregion
        #region Map Map : карта
        private Map _Map;
        /// <summary>карта</summary>
        public Map Map
        {
            get { return _Map; }
            set { Set(ref _Map, value); }
        }
        #endregion
        #region HexField Field : игорвое поле
        private HexField _Field;
        /// <summary>игорвое поле</summary>
        public HexField Field
        {
            get { return _Field; }
            set { Set(ref _Field, value); }
        }
        #endregion
        #region GameState GameState : статус игры
        private GameState _GameState;
        /// <summary>статус игры</summary>
        public GameState GameState
        {
            get { return _GameState; }
            set { Set(ref _GameState, value); }
        }
        #endregion
        #endregion

        #region Commands
        #region LogoutCommand : выйти из сессии
        /// <summary>выйти из сессии</summary>
        public ICommand LogoutCommand { get; }
        private bool CanLogoutCommandExecute(object p) => Core.Connected;
        private async void OnLogoutCommandExecuted(object p)
        {
            var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            main_page.SelectLoadPage();
            var res = await Core.SendLogoutAsync();
            if (res != Result.OKEY)
                Core.Log("Ошибка: " + res.ToString());
        }
        #endregion
        #region SendChatMessageCommand : отправить сообщение в чат
        /// <summary>отправить сообщение в чат</summary>
        public ICommand SendChatMessageCommand { get; }
        private bool CanSendChatMessageCommandExecute(object p) => Core.Connected && ChatText?.Length > 1 && StepEnable;
        private async void OnSendChatMessageCommandExecuted(object p)
        {
            var res = await Core.SendChatAsync(ChatText).ConfigureAwait(false);
            if (res == Result.OKEY)
            {
                //Chat.Add(ChatText);
                ChatText = "";
            }
            else
                Core.Log("Ошибка: " + res.ToString());
        }
        #endregion
        #region TurnCommand : переключить ход
        /// <summary>переключить ход</summary>
        public ICommand TurnCommand { get; }
        private bool CanTurnCommandExecute(object p) => StepEnable;
        private void OnTurnCommandExecuted(object p)
        {
            EventWait?.TrySetResult(true);
        }
        #endregion
        #endregion

        public GamePageViewModel()
        {
            #region Properties
            Core = App.Core;
            Chat = new ObservableCollection<string>();
            Field = new HexField();
            #endregion

            #region Commands
            LogoutCommand = new LambdaCommand(OnLogoutCommandExecuted, CanLogoutCommandExecute);
            SendChatMessageCommand = new LambdaCommand(OnSendChatMessageCommandExecuted, CanSendChatMessageCommandExecute);
            TurnCommand = new LambdaCommand(OnTurnCommandExecuted, CanTurnCommandExecute);
            #endregion
        }

        public void Reset()
        {
            ChatText = null;
            Chat.Clear();
            StepEnable = false;
            MessageWait = "Ожидание хода...";
            GameState = null;
            Map = null;
            Player = null;
            Field = new HexField();
            TeamColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xf8, 0x57, 0x06));
        }

        public async Task<Result> MoveAsync(int id, Point3 point)
        {
            Result res = await Core.SendMoveAsync(id, point).ConfigureAwait(false);

            if (res == Result.OKEY)
            {
                TotalStep++;
                App.Current.Dispatcher.Invoke(() => { Field.VehicleMove(id, point); });
                Core.Log("Машина перемещена по x: " + point.x + ", y: " + point.y + ", z: " + point.z);
            }
            else if (res == Result.BAD_COMMAND)
            {
                Core.Log("MoveAsync: Неверный ход");
            }
            return res;
        }

        public async Task<Result> ShootAsync(int id, Point3 point)
        {
            Result res = await Core.SendShootAsync(id, point).ConfigureAwait(false);
            if (res == Result.OKEY)
            {
                TotalStep++;
                App.Current.Dispatcher.Invoke(() => { Field.VehicleShoot(id, point); });
                Core.Log("Выстрел по x: " + point.x + ", y: " + point.y + ", z: " + point.z);
            }
            else if (res == Result.BAD_COMMAND)
            {
                Core.Log("ShootAsync: Неверный ход");
            }
            return res;
        }

        public async Task<Result> TurnAsync()
        {
            App.Current.Dispatcher.Invoke(() => { StepEnable = false; CommandBase.RaiseCanExecuteChanged(); });
            return await Core.SendTurnAsync().ConfigureAwait(false);
        }

        public async Task RunGame()
        {
            // get game map
            var res_map = await Core.SendMapAsync().ConfigureAwait(false);
            if (res_map.Item1 != Result.OKEY)
            {
                Core.Log("Ошибка: " + res_map.Item1.ToString());
                return;
            }
            Map = res_map.Item2;
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageWait = "Ожидание хода...";
                Field.CreateField(_Map);
                var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
                main_page.SelectGamePage();
            });
            Core.Log("Карта загружена");

            //wait players
            while (true)
            {
                var res = await Core.SendGameStateAsync().ConfigureAwait(false);
                if (res.Item1 == Result.OKEY)
                {
                    GameState = res.Item2;
                    App.Current.Dispatcher.Invoke(() => { Field.CreateContent(res.Item2); });
                }

                if (!Field.Inited)
                    await Task.Delay(500);
                else break;
            }

            PlayerEx curr_player;
            if (Field.players.TryGetValue(Player.idx, out curr_player))
                TeamColor = curr_player.color;

            Core.Log("Все игроки подключены");

            //game loop
            while (true)
            {
                var res = await Core.SendGameStateAsync().ConfigureAwait(false);
                GameState = res.Item2;
                if (GameState == null) break;
                if (GameState.finished)
                {
                    if (GameState.winner != null) MessageWait = "Победитель: " + Field.players[GameState.winner.Value].player.name;
                    else MessageWait = "Ничья";
                    break;
                }

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    StepEnable = true;
                    EventWait = new TaskCompletionSource<bool>();
                    Field.UpdateContent(GameState);
                    CommandBase.RaiseCanExecuteChanged();
                }));

                var act = await Core.SendActionsAsync().ConfigureAwait(false);
                if (act.Item2 != null)
                {
                    foreach (var i in act.Item2.actions)
                    {
                        if (i.action_type == Model.WebAction.CHAT)
                        {
                            ChatMessage msg = JsonConvert.DeserializeObject<ChatMessage>(i.data.ToString());
                            App.Current.Dispatcher.Invoke(() => { Chat.Add(msg.message); });
                        }
                    }
                }

                if (AIEnable)
                {
                    if (GameState.current_player_idx == Player.idx)
                    {
                        var actions = Core.GetAIActions(Player.idx, GameState, Map);
                        foreach (var i in actions.actions)
                        {
                            Result result = Result.OKEY;
                            if (i.action_type == action_type.move)
                            {
                                result = await MoveAsync(i.vec_id, new Point3() { x = i.point.x, y = i.point.y, z = i.point.z }).ConfigureAwait(false);
                            }
                            else if (i.action_type == action_type.shoot)
                            {
                                result = await ShootAsync(i.vec_id, new Point3() { x = i.point.x, y = i.point.y, z = i.point.z }).ConfigureAwait(false);
                            }

                            if (result != Result.OKEY && result != Result.BAD_COMMAND)
                            {
                                Core.Log(result.ToString());
                                break;
                            }
                        }
                    }
                    await TurnAsync();
                }
                else
                {
                    if (GameState.current_player_idx == Player.idx)
                        await Task.WhenAny(EventWait.Task, Task.Delay(9000));
                    await TurnAsync();
                }
            }
        }
    }
}
