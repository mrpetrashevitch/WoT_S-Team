using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UIClient.Infrastructure.Command;
using UIClient.Infrastructure.Command.Base;
using UIClient.Infrastructure.Controls;
using UIClient.Model.Client;
using UIClient.Model.Client.Api;
using UIClient.Model.Config;
using UIClient.Model.Server;
using UIClient.View.Pages;
using UIClient.ViewModel;

namespace UIClient.Model
{
    public enum WebActions
    {
        LOGIN = 1,
        LOGOUT = 2,
        MAP = 3,
        GAME_STATE = 4,
        GAME_ACTIONS = 5,
        TURN = 6,
        CHAT = 100,
        MOVE = 101,
        SHOOT = 102,
    }

    public enum Result : int
    {
        OKEY = 0,
        BAD_COMMAND = 1,
        ACCESS_DENIED = 2,
        INAPPROPRIATE_GAME_STATE = 3,
        TIMEOUT = 4,
        INTERNAL_SERVER_ERROR = 500,

        // for client
        ERROR_WSA_INIT,
        ERROR_SOCKET,
        ERROR_CONNECT,
        CONNECTED_FALSE,
        ERROR_SEND,
        ERROR_RECV,
        IVALID_PARAM,
    };

    public class Core : ViewModel.Base.ViewModelBase
    {
        IntPtr web = IntPtr.Zero;
        object locker = new object();
        public Core()
        {
            web = WebClientDll.create();
            Log("Web ядро создано");

            if (web == IntPtr.Zero) throw new Exception("Error create core!");

            Chat = new ObservableCollection<string>();
            Field = new HexField();
            SelectedCanMove = new List<Hex>();
            SelectedCanShoot = new List<Hex>();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += TimerTick;

            LogoutCommand = new LambdaCommand(OnLogoutCommandExecuted, CanLogoutCommandExecute);
            SendChatMessageCommand = new LambdaCommand(OnSendChatMessageCommandExecuted, CanSendChatMessageCommandExecute);
            TurnCommand = new LambdaCommand(OnTurnCommandExecuted, CanTurnCommandExecute);

            Log("UI ядро создано");
            Connect();
        }

        public void Reset()
        {
            StepEnable = false;
            GameState = null;
            Map = null;
            Player = null;
            Field = new HexField();
            SelectedHex = null;
            ChatText = null;
            SelectedCanMove.Clear();
            SelectedCanShoot.Clear();
            Chat.Clear();
            MessageWait = "Ожидание хода...";
            TeamColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xf8, 0x57, 0x06));
        }

        #region bool Connected : подключен ли к серверу
        private bool _Connected;
        /// <summary>подключен ли к серверу</summary>
        public bool Connected
        {
            get { return _Connected; }
            set { Set(ref _Connected, value); }
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

        #region Timer
        #region int TotalTick : прошло секунд
        private int _TotalTick;
        /// <summary>прошло секунд</summary>
        public int TotalTick
        {
            get { return _TotalTick; }
            set { Set(ref _TotalTick, value); }
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
        private DispatcherTimer Timer = new DispatcherTimer();
        private async void TimerTick(object sender, EventArgs e)
        {
            TotalTick--;
            Log("Осталось: " + TotalTick + "c");
            if (TotalTick == 0)
            {
                Log("Время истекло");
                await TimerStop();
            }
        }
        public async Task TimerRun()
        {
            TotalTick = 9;
            TotalStep = 0;
            StepEnable = true;
            Timer.Start();
            await Task.FromResult(false);
        }
        public async Task TimerStop()
        {
            Timer.Stop();
            await SendTurnAsync();
            await SendGameStateAsync();
        }
        #endregion
        #region Hex SelectedHex : выбранная ячейка
        private Hex _SelectedHex;
        /// <summary>выбранная ячейка</summary>
        public Hex SelectedHex
        {
            get { return _SelectedHex; }
            set { Set(ref _SelectedHex, value); }
        }
        #endregion
        #region List<Hex> SelectedCanMove : лист гексов, куда можно ходить
        private List<Hex> _SelectedCanMove;
        /// <summary>лист гексов, куда можно ходить</summary>
        public List<Hex> SelectedCanMove
        {
            get { return _SelectedCanMove; }
            set { Set(ref _SelectedCanMove, value); }
        }
        #endregion
        #region List<Hex> SelectedCanShoot : лист гексов, куда можно стрелять
        private List<Hex> _SelectedCanShoot;
        /// <summary>лист гексов, куда можно стрелять</summary>
        public List<Hex> SelectedCanShoot
        {
            get { return _SelectedCanShoot; }
            set { Set(ref _SelectedCanShoot, value); }
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
        #region ObservableCollection<string> Logs : програмные логи
        private ObservableCollection<string> _Logs = new ObservableCollection<string>();
        /// <summary>програмные логи</summary>
        public ObservableCollection<string> Logs
        {
            get { return _Logs; }
            set { Set(ref _Logs, value); }
        }
        public void Log(string str)
        {
            Logs.Insert(0, str);
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

        #region LogoutCommand : выйти из сессии
        /// <summary>выйти из сессии</summary>
        public ICommand LogoutCommand { get; }
        private bool CanLogoutCommandExecute(object p) => Connected;
        private async void OnLogoutCommandExecuted(object p)
        {
            var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            main_page.SelectLoadPage();
            var res = await SendLogoutAsync();
            if (res != Result.OKEY)
                Log("Ошибка: " + res.ToString());
        }
        #endregion
        #region SendChatMessageCommand : отправить сообщение в чат
        /// <summary>отправить сообщение в чат</summary>
        public ICommand SendChatMessageCommand { get; }
        private bool CanSendChatMessageCommandExecute(object p) => Connected && ChatText?.Length > 1 && StepEnable;
        private async void OnSendChatMessageCommandExecuted(object p)
        {
            var res = await SendChatAsync(ChatText).ConfigureAwait(false);
            if (res == Result.OKEY)
            {
                //Chat.Add(ChatText);
                ChatText = "";
            }
            else
                Log("Ошибка: " + res.ToString());
        }
        #endregion
        #region TurnCommand : переключить ход
        /// <summary>переключить ход</summary>
        public ICommand TurnCommand { get; }
        private bool CanTurnCommandExecute(object p) => StepEnable;
        private async void OnTurnCommandExecuted(object p)
        {
            await TimerStop();
        }
        #endregion

        async Task<Result> SendPacket<T>(WebActions action, T data, bool scip_data = false)
        {
            string json_str = "";

            if (!scip_data)
                json_str = JsonConvert.SerializeObject(data);

            byte[] size_msg = System.Text.Encoding.UTF8.GetBytes(json_str);
            GCHandle pinned_msg = GCHandle.Alloc(size_msg, GCHandleType.Pinned);
            IntPtr pointer_msg = pinned_msg.AddrOfPinnedObject();
            byte[] buffer = new byte[4096];
            GCHandle pinned_buffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer_buffer = pinned_buffer.AddrOfPinnedObject();
            byte[] out_size = new byte[4];
            GCHandle pinned_out_size = GCHandle.Alloc(out_size, GCHandleType.Pinned);
            IntPtr pointer_out_size = pinned_out_size.AddrOfPinnedObject();

            Result res;
            string message = "";
            lock (locker)
            {
                if (scip_data)
                    res = WebClientDll.send_packet(web, action, 0, IntPtr.Zero, pointer_out_size, pointer_buffer);
                else
                    res = WebClientDll.send_packet(web, action, size_msg.Length, pointer_msg, pointer_out_size, pointer_buffer);

                message = Encoding.UTF8.GetString(buffer, 0, BitConverter.ToInt32(out_size, 0));
            }
            pinned_msg.Free();
            pinned_out_size.Free();
            pinned_buffer.Free();

            if (res == Result.OKEY)
            {
                switch (action)
                {
                    case WebActions.LOGIN:
                        {
                            Player = JsonConvert.DeserializeObject<Player>(message);
                        }
                        break;
                    case WebActions.LOGOUT:
                        {
                            GameState = null;
                        }
                        break;
                    case WebActions.MAP:
                        {
                            Map = JsonConvert.DeserializeObject<Map>(message);
                        }
                        break;
                    case WebActions.GAME_STATE:
                        {
                            GameState = JsonConvert.DeserializeObject<GameState>(message);

                            if (Field.Inited)
                            {
                                if (GameState.finished)
                                {
                                    if (GameState.winner != null)
                                        MessageWait = "Победитель: " + Field.players[GameState.winner.Value].player.name;
                                    else
                                        MessageWait = "Ничья";
                                }
                                else
                                {
                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        TimerRun().Wait();
                                        Field.UpdateContent(this.GameState);
                                        CommandBase.RaiseCanExecuteChanged();
                                    }));
                                    await SendActionsAsync().ConfigureAwait(false);

                                    if (AIEnable)
                                    {
                                        if (GameState.current_player_idx == Player.idx)
                                        {
                                            var actions = GetAIActions();
                                            int steps = 0;
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
                                                if (result == Result.OKEY) steps++;
                                            }
                                            if (steps != 5)
                                            {
                                                await TimerStop().ConfigureAwait(false);
                                            }
                                        }
                                        else
                                        {
                                            await TimerStop().ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case WebActions.GAME_ACTIONS:
                        {
                            var v = JsonConvert.DeserializeObject<Actions>(message);
                            if (v.actions != null)
                            {
                                foreach (var i in v.actions)
                                {
                                    if (i.action_type == WebActions.CHAT)
                                    {
                                        ChatMessage msg = JsonConvert.DeserializeObject<ChatMessage>(i.data.ToString());
                                        App.Current.Dispatcher.Invoke(() => { Chat.Add(msg.message); });
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return res;
        }

        public action_ret GetAIActions()
        {
            IntPtr player_s_ptr = IntPtr.Zero;
            GCHandle player_pipe = default(GCHandle);
            IntPtr vehicle_s_ptr = IntPtr.Zero;
            GCHandle vehicle_pipe = default(GCHandle);
            IntPtr winpoints_s_ptr = IntPtr.Zero;
            GCHandle winpoints_pipe = default(GCHandle);
            IntPtr attackmatrix_s_ptr = IntPtr.Zero;
            GCHandle attackmatrix_pipe = default(GCHandle);
            IntPtr base_s_ptr = IntPtr.Zero;
            GCHandle base_pipe = default(GCHandle);

            player_native[] player_s = null;
            if (this.GameState.players != null)
            {
                player_s = new player_native[this.GameState.players.Length];
                for (int i = 0; i < this.GameState.players.Length; i++)
                {
                    player_s[i].idx = this.GameState.players[i].idx;
                    player_s[i].is_observer = this.GameState.players[i].is_observer ? 1 : 0;
                }
            }
            vehicle_native[] vehicle_s = null;
            if (this.GameState.vehicles.Count > 0)
            {
                vehicle_s = new vehicle_native[this.GameState.vehicles.Count];
                int i = 0;
                foreach (var item in this.GameState.vehicles)
                {
                    vehicle_s[i].vehicle_id = item.Key;
                    vehicle_s[i].capture_points = item.Value.capture_points;
                    vehicle_s[i].health = item.Value.health;
                    vehicle_s[i].player_id = item.Value.player_id;
                    vehicle_s[i].position.x = item.Value.position.x;
                    vehicle_s[i].position.y = item.Value.position.y;
                    vehicle_s[i].position.z = item.Value.position.z;
                    vehicle_s[i].spawn_position.x = item.Value.spawn_position.x;
                    vehicle_s[i].spawn_position.y = item.Value.spawn_position.y;
                    vehicle_s[i].spawn_position.z = item.Value.spawn_position.z;
                    vehicle_s[i].vehicle_type = item.Value.vehicle_type;
                    i++;
                }
            }
            win_points_native[] winpoints_s = null;
            if (this.GameState.win_points.Count > 0)
            {
                winpoints_s = new win_points_native[this.GameState.win_points.Count];
                int i = 0;
                foreach (var item in this.GameState.win_points)
                {
                    winpoints_s[i].id = item.Key;
                    winpoints_s[i].capture = item.Value.capture;
                    winpoints_s[i].kill = item.Value.kill;
                    i++;
                }
            }
            attack_matrix_native[] attackmatrix_s = null;
            if (this.GameState.attack_matrix.Count > 0)
            {
                attackmatrix_s = new attack_matrix_native[this.GameState.attack_matrix.Count];
                int i = 0;
                unsafe
                {
                    foreach (var item in this.GameState.attack_matrix)
                    {
                        int j = 0;
                        attackmatrix_s[i].id = item.Key;
                        foreach (var ind in item.Value)
                        {
                            attackmatrix_s[i].attack[j] = ind;
                            j++;
                        }
                        i++;
                    }
                }
            }
            point[] base_s = null;
            if (this.Map.content._base != null)
            {
                base_s = new point[this.Map.content._base.Length];
                int i = 0;
                foreach (var item in this.Map.content._base)
                {
                    base_s[i].x = item.x;
                    base_s[i].y = item.y;
                    base_s[i].z = item.z;
                    i++;
                }
            }

            if (player_s != null)
            {
                player_pipe = GCHandle.Alloc(player_s, GCHandleType.Pinned);
                player_s_ptr = player_pipe.AddrOfPinnedObject();
            }
            if (vehicle_s != null)
            {
                vehicle_pipe = GCHandle.Alloc(vehicle_s, GCHandleType.Pinned);
                vehicle_s_ptr = vehicle_pipe.AddrOfPinnedObject();
            }
            if (winpoints_s != null)
            {
                winpoints_pipe = GCHandle.Alloc(winpoints_s, GCHandleType.Pinned);
                winpoints_s_ptr = winpoints_pipe.AddrOfPinnedObject();
            }
            if (attackmatrix_s != null)
            {
                attackmatrix_pipe = GCHandle.Alloc(attackmatrix_s, GCHandleType.Pinned);
                attackmatrix_s_ptr = attackmatrix_pipe.AddrOfPinnedObject();
            }
            if (base_s != null)
            {
                base_pipe = GCHandle.Alloc(base_s, GCHandleType.Pinned);
                base_s_ptr = base_pipe.AddrOfPinnedObject();
            }

            action_ret action_re;
            var act = WebClientDll.get_action(Player.idx,
                player_s_ptr, player_s?.Length ?? 0,
                vehicle_s_ptr, vehicle_s?.Length ?? 0,
                winpoints_s_ptr, winpoints_s?.Length ?? 0,
                attackmatrix_s_ptr, attackmatrix_s?.Length ?? 0,
                base_s_ptr, base_s?.Length ?? 0,
                out action_re);

            if (player_s != null) player_pipe.Free();
            if (vehicle_s != null) vehicle_pipe.Free();
            if (winpoints_s != null) winpoints_pipe.Free();
            if (attackmatrix_s != null) attackmatrix_pipe.Free();
            if (base_s != null) base_pipe.Free();

            return action_re;
        }

        public async Task<Result> MoveAsync(int id, Point3 point)
        {
            Result res = await SendMoveAsync(id, point).ConfigureAwait(false);

            if (res == Result.OKEY)
            {
                TotalStep++;
                App.Current.Dispatcher.Invoke(() => { Field.VehicleMove(id, point); });
                Log("Машина перемещена по x: " + point.x + ", y: " + point.y + ", z: " + point.z);

                if (TotalStep == 5)
                    await TimerStop();
            }
            else if (res == Result.BAD_COMMAND)
            {
                Log("Неверный ход");
            }
            else if (res == Result.INAPPROPRIATE_GAME_STATE)
            {
                Log("Ход перешел другому");
                await TimerStop();
            }
            else
            {
                Log(res.ToString());
                await TimerStop();
            }
            return res;
        }

        public async Task<Result> ShootAsync(int id, Point3 point)
        {
            Result res = await SendShootAsync(id, point).ConfigureAwait(false);

            if (res == Result.OKEY)
            {
                TotalStep++;
                App.Current.Dispatcher.Invoke(() => { Field.VehicleShoot(id, point); });
                Log("Выстрел по x: " + point.x + ", y: " + point.y + ", z: " + point.z);

                if (TotalStep == 5)
                    await TimerStop();
            }
            else if (res == Result.BAD_COMMAND)
            {
                Log("Неверный ход");
            }
            else if (res == Result.INAPPROPRIATE_GAME_STATE)
            {
                Log("Ход перешел другому");
                await TimerStop();
            }
            else
            {
                Log(res.ToString());
                await TimerStop();
            }
            return res;
        }

        public async Task<Result> SendLoginAsync(LoginCreate log)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.LOGIN, log)).ConfigureAwait(false);
        }

        public async Task<Result> SendLogoutAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.LOGOUT, 0, true)).ConfigureAwait(false);
        }

        public async Task<Result> SendMapAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.MAP, 0, true)).ConfigureAwait(false);
        }

        public async Task<Result> SendGameStateAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.GAME_STATE, 0, true)).ConfigureAwait(false);
        }

        public async Task<Result> SendActionsAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.GAME_ACTIONS, 0, true)).ConfigureAwait(false);
        }

        public async Task<Result> SendTurnAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            App.Current.Dispatcher.Invoke(() => { StepEnable = false; CommandBase.RaiseCanExecuteChanged(); });
            return await Task.Run(() => SendPacket(WebActions.TURN, 0, true)).ConfigureAwait(false);
        }

        public async Task<Result> SendMoveAsync(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionMove move = new ActionMove();
            move.target = point;
            move.vehicle_id = id;
            return await Task.Run(() => SendPacket(WebActions.MOVE, move)).ConfigureAwait(false);
        }

        public async Task<Result> SendChatAsync(string msg)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ChatMessage msgs = new ChatMessage();
            msgs.message = msg;
            return await SendPacket(WebActions.CHAT, msgs).ConfigureAwait(false);
        }

        public async Task<Result> SendShootAsync(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionShoot shoot = new ActionShoot();
            shoot.target = point;
            shoot.vehicle_id = id;
            return await Task.Run(() => SendPacket(WebActions.SHOOT, shoot)).ConfigureAwait(false);
        }

        public async void Connect()
        {
            try
            {
                Log("Подключение к серверу...");
                var config = App.Host.Services.GetRequiredService<AppConfig>();

                IPAddress[] addresslist = Dns.GetHostAddresses(config.NetConfig.HostName);
#pragma warning disable CS0618 // Тип или член устарел
                await Task.Run(() => { WebClientDll.connect_(web, (uint)addresslist[0].Address, config.NetConfig.Port); });
#pragma warning restore CS0618 // Тип или член устарел

                Connected = true;
                Log("Подключен к серверу");
            }
            catch
            {
                Connected = false;
                Log("Ошибка подключения к серверу");
            }
        }
    }
}