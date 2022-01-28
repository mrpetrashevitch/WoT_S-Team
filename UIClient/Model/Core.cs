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
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UIClient.Infrastructure.Command;
using UIClient.Infrastructure.Command.Base;
using UIClient.Infrastructure.Controls;
using UIClient.Model.Config;
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

    class LoginCreate
    {
        public string name { get; set; }
        public string password { get; set; }
        public string game { get; set; }
        public int? num_turns { get; set; }
        public int num_players { get; set; }
        public bool is_observer { get; set; }
    }

    class Message
    {
        public string message { get; set; }
    }

    public class Core : ViewModel.Base.ViewModelBase
    {
        const string _PathCoreDll = "WebClient.dll";
        IntPtr web = IntPtr.Zero;

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static IntPtr create();

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static Result destroy(IntPtr web);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static Result connect_(IntPtr web, uint addr, ushort port);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static Result send_packet(IntPtr web, WebActions action, int size, IntPtr data, IntPtr out_size, IntPtr out_data);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static action get_action(int curr_player,
                                        IntPtr players, int players_size,
                                        IntPtr vehicle, int vehicle_size,
                                        IntPtr win_points, int win_points_size,
                                        IntPtr attack_matrix, int attack_matrix_size);


        public Core()
        {
            web = create();
            if (web == IntPtr.Zero) throw new Exception("");


            Logs = new ObservableCollection<string>();
            Chat = new ObservableCollection<string>();
            Field = new HexField();
            PlayersMax = 2;


            SetUserNameCommand = new LambdaCommand(OnSetUserNameCommandExecuted, CanSetUserNameCommandExecute);
            LogoutCommand = new LambdaCommand(OnLogoutCommandExecuted, CanLogoutCommandExecute);
            ChatSendMsgCommand = new LambdaCommand(OnChatSendMsgCommandExecuted, CanChatSendMsgCommandExecute);
            GetActionsCommand = new LambdaCommand(OnGetActionsCommandExecuted, CanGetActionsCommandExecute);
            GetGameStateCommand = new LambdaCommand(OnGetGameStateCommandExecuted, CanGetGameStateCommandExecute);
            // init core
            Log("Ядро создано");
            Connect();
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

        #region string MessageWait : сообщение ожидания
        private string _MessageWait;
        /// <summary>сообщение ожидания</summary>
        public string MessageWait
        {
            get { return _MessageWait; }
            set { Set(ref _MessageWait, value); }
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

        #region bool VecEnable : summury
        private bool _VecEnable = false;
        /// <summary>summury</summary>
        public bool VecEnable
        {
            get { return _VecEnable; }
            set
            {
                if (Set(ref _VecEnable, value))
                {
                    if (value) WaitMsgVisible = Visibility.Collapsed;
                    else WaitMsgVisible = Visibility.Visible;
                }
            }
        }
        #endregion

        #region Visibility WaitMsgVisible : надпись ожидания
        private Visibility _WaitMsgVisible;
        /// <summary>надпись ожидания</summary>
        public Visibility WaitMsgVisible
        {
            get { return _WaitMsgVisible; }
            set { Set(ref _WaitMsgVisible, value); }
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

        #region Player Player : текущий игрок
        private Player _Player;
        /// <summary>текущий игрок</summary>
        public Player Player
        {
            get { return _Player; }
            set { Set(ref _Player, value); }
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

        #region string ChatText : вписанный текст
        private string _ChatText;
        /// <summary>вписанный текст</summary>
        public string ChatText
        {
            get { return _ChatText; }
            set { Set(ref _ChatText, value); }
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

        #region ObservableCollection<string> Logs : програмные логи
        private ObservableCollection<string> _Logs;
        /// <summary>програмные логи</summary>
        public ObservableCollection<string> Logs
        {
            get { return _Logs; }
            set { Set(ref _Logs, value); }
        }
        void Log(string str)
        {
            Logs.Insert(0, str);
        }
        #endregion

        #region SetUserNameCommand : установить имя пользователя
        /// <summary>установить имя пользователя</summary>
        public ICommand SetUserNameCommand { get; }
        private bool CanSetUserNameCommandExecute(object p)
        {
            if (!Connected) return false;
            if (UserName == null || UserName.Length < 4) return false;
            if (GameName != null && GameName.Length == 0) return false;
            if (PlayersMax < 1 || PlayersMax > 3) return false;
            if (TurnMax != null && TurnMax < 1) return false;
            return true;
        }
        private async void OnSetUserNameCommandExecuted(object p)
        {
            var res = await LoginAsync();
            if (res != Result.OKEY)
            {
                Log("Ошибка: " + res.ToString());
                return;
            }
            Log("Авторизация выполнена");

            res = await GetMapAsync();
            if (res != Result.OKEY)
            {
                Log("Ошибка: " + res.ToString());
                return;
            }
            Log("Карта загружена");
            MessageWait = "Ожидание хода...";

            var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            main_page.SelectGamePage();

            //wait players
            while (true)
            {
                res = await GetGameStateAsync();
                if (res == Result.OKEY)
                {
                    if (!Field.Inited)
                    {
                        Field.CreateContent(GameState);
                        if (Field.Inited)
                        {
                            res = await GetGameStateAsync();
                            Log("Все игроки подключены");
                            break;
                        }
                    }
                }
                await Task.Delay(1000);
            }
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
            var res = await LogoutAsync();
            if (res != Result.OKEY)
                Log("Ошибка: " + res.ToString());
        }
        #endregion

        #region ChatSendMsgCommand : отправить сообщение в чат
        /// <summary>отправить сообщение в чат</summary>
        public ICommand ChatSendMsgCommand { get; }
        private bool CanChatSendMsgCommandExecute(object p) => Connected && ChatText?.Length > 1 && VecEnable;
        private void OnChatSendMsgCommandExecuted(object p)
        {
            var res = ChatSend(ChatText);
            if (res == Result.OKEY)
            {
                Chat.Add(ChatText);
                ChatText = "";
            }
            else
                Log("Ошибка: " + res.ToString());
        }
        #endregion

        #region GetActionsCommand : summury
        /// <summary>summury</summary>
        public ICommand GetActionsCommand { get; }
        private bool CanGetActionsCommandExecute(object p) => Connected;
        private void OnGetActionsCommandExecuted(object p)
        {
            var res = GetActions();
            if (res != Result.OKEY)
                Log("Ошибка: " + res.ToString());
        }
        #endregion

        #region GetGameStateCommand : получить статус игры
        /// <summary>получить статус игры</summary>
        public ICommand GetGameStateCommand { get; }
        private bool CanGetGameStateCommandExecute(object p) => true;
        private void OnGetGameStateCommandExecuted(object p)
        {
            var res = GetGameState();
            if (res != Result.OKEY)
                Log("Ошибка: " + res.ToString());
        }
        #endregion

        static object locker = new object();

        Result SendPacket<T>(WebActions action, T data, bool scip_data = false)
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
                    res = send_packet(web, action, 0, IntPtr.Zero, pointer_out_size, pointer_buffer);
                else
                    res = send_packet(web, action, size_msg.Length, pointer_msg, pointer_out_size, pointer_buffer);

                message = Encoding.UTF8.GetString(buffer, 0, BitConverter.ToInt32(out_size, 0));

                pinned_msg.Free();
                pinned_out_size.Free();
                pinned_buffer.Free();
            }

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
                            //Map.size = 12;
                            App.Current.Dispatcher.Invoke(new Action(() => { Field.CreateField(Map); }));

                        }
                        break;
                    case WebActions.GAME_STATE:
                        {
                            GameState = JsonConvert.DeserializeObject<GameState>(message);

                            if (Field.Inited)
                            {
                                if (GameState.winner != null)
                                {
                                    MessageWait = "Победитель: " + Field.players[GameState.winner.Value].player.name;
                                }
                                else
                                {
                                    if (GameState.current_player_idx == Player.idx)
                                    {
                                        GetActions();
                                        VecEnable = true;
                                        if (false) // true - enable io
                                        {
                                            IntPtr player_s_ptr = IntPtr.Zero;
                                            GCHandle player_pipe = default(GCHandle);
                                            IntPtr vehicle_s_ptr = IntPtr.Zero;
                                            GCHandle vehicle_pipe = default(GCHandle);
                                            IntPtr winpoints_s_ptr = IntPtr.Zero;
                                            GCHandle winpoints_pipe = default(GCHandle);
                                            IntPtr attackmatrix_s_ptr = IntPtr.Zero;
                                            GCHandle attackmatrix_pipe = default(GCHandle);

                                            Player_native[] player_s = null;
                                            if (this.GameState.players != null)
                                            {
                                                player_s = new Player_native[this.GameState.players.Length];
                                                for (int i = 0; i < this.GameState.players.Length; i++)
                                                {
                                                    player_s[i].idx = this.GameState.players[i].idx;
                                                    player_s[i].is_observer = this.GameState.players[i].is_observer ? 1 : 0;
                                                }
                                            }
                                            Vehicle_native[] vehicle_s = null;
                                            if (this.GameState.vehicles.Count > 0)
                                            {
                                                vehicle_s = new Vehicle_native[this.GameState.vehicles.Count];
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
                                                    switch (item.Value.vehicle_type)
                                                    {
                                                        case "medium_tank": vehicle_s[i].vehicle_type = VehicleType.MT; break;
                                                        case "light_tank": vehicle_s[i].vehicle_type = VehicleType.LT; break;
                                                        case "heavy_tank": vehicle_s[i].vehicle_type = VehicleType.HT; break;
                                                        case "at_spg": vehicle_s[i].vehicle_type = VehicleType.ASPG; break;
                                                        case "spg": vehicle_s[i].vehicle_type = VehicleType.SPG; break;
                                                        default: break;
                                                    }
                                                    i++;
                                                }
                                            }
                                            WinPoints_native[] winpoints_s = null;
                                            if (this.GameState.win_points.Count > 0)
                                            {
                                                winpoints_s = new WinPoints_native[this.GameState.win_points.Count];
                                                int i = 0;
                                                foreach (var item in this.GameState.win_points)
                                                {
                                                    winpoints_s[i].id = item.Key;
                                                    winpoints_s[i].capture = item.Value.capture;
                                                    winpoints_s[i].kill = item.Value.kill;
                                                    i++;
                                                }
                                            }
                                            AttackMatrix_native[] attackmatrix_s = null;
                                            if (this.GameState.attack_matrix.Count > 0)
                                            {
                                                attackmatrix_s = new AttackMatrix_native[this.GameState.attack_matrix.Count];
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

                                            var act = get_action(Player.idx,
                                                player_s_ptr, player_s.Length,
                                                vehicle_s_ptr, vehicle_s.Length,
                                                winpoints_s_ptr, winpoints_s.Length,
                                                attackmatrix_s_ptr, attackmatrix_s.Length);

                                            if (player_s != null) player_pipe.Free();
                                            if (vehicle_s != null) vehicle_pipe.Free();
                                            if (winpoints_s != null) winpoints_pipe.Free();
                                            if (attackmatrix_s != null) attackmatrix_pipe.Free();
                                        }
                                    }
                                    else
                                    {
                                        VecEnable = false;
                                        Turn();
                                        GetGameState();
                                    }
                                }
                                App.Current.Dispatcher.Invoke(new Action(() => { Field.UpdateContent(GameState); }));
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
                                        Message msg = JsonConvert.DeserializeObject<Message>(i.data.ToString());
                                        App.Current.Dispatcher.Invoke(new Action(() => { Chat.Add(msg.message); }));
                                    }
                                    //else if (i.action_type == NetActions.MOVE)
                                    //{
                                    //    ActionMove move = JsonConvert.DeserializeObject<ActionMove>(i.data.ToString());
                                    //    //Field.VehicleMove(move.vehicle_id, move.target);
                                    //}
                                    //else if (i.action_type == NetActions.SHOOT)
                                    //{
                                    //    ActionShoot shoot = JsonConvert.DeserializeObject<ActionShoot>(i.data.ToString());
                                    //}
                                }
                            }
                        }
                        break;
                    case WebActions.TURN:
                        {

                        }
                        break;
                    case WebActions.CHAT:
                        {

                        }
                        break;
                    case WebActions.MOVE:
                        {

                        }
                        break;
                    case WebActions.SHOOT:
                        break;
                    default:
                        break;
                }
            }
            return res;


        }

        public Result Login()
        {
            if (!Connected) return Result.CONNECTED_FALSE;

            LoginCreate log = new LoginCreate();
            log.name = UserName;
            log.password = Pass;
            log.game = GameName;
            log.num_turns = TurnMax;
            log.num_players = PlayersMax;
            log.is_observer = IsObserver;
            return SendPacket(WebActions.LOGIN, log);

        }

        public async Task<Result> LoginAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;

            LoginCreate log = new LoginCreate();
            log.name = UserName;
            log.password = Pass;
            log.game = GameName;
            log.num_turns = TurnMax;
            log.num_players = PlayersMax;
            log.is_observer = IsObserver;
            return await Task.Run(() => SendPacket(WebActions.LOGIN, log));
        }

        public Result Logout()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return SendPacket(WebActions.LOGOUT, 0, true);
        }

        public async Task<Result> LogoutAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.LOGOUT, 0, true));
        }

        public Result GetMap()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return SendPacket(WebActions.MAP, 0, true);
        }

        public async Task<Result> GetMapAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.MAP, 0, true));
        }

        public Result GetGameState()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return SendPacket(WebActions.GAME_STATE, 0, true);
        }

        public async Task<Result> GetGameStateAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.GAME_STATE, 0, true));
        }

        public Result GetActions()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return SendPacket(WebActions.GAME_ACTIONS, 0, true);
        }

        public async Task<Result> GetActionsAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.GAME_ACTIONS, 0, true));
        }

        public Result Turn()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return SendPacket(WebActions.TURN, 0, true);
        }

        public async Task<Result> TurnAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            return await Task.Run(() => SendPacket(WebActions.TURN, 0, true));
        }

        public Result ChatSend(string msg)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            Message msgs = new Message();
            msgs.message = msg;
            return SendPacket(WebActions.CHAT, msgs);
        }

        public Result Move(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionMove move = new ActionMove();
            move.target = point;
            move.vehicle_id = id;
            return SendPacket(WebActions.MOVE, move);
        }

        public async Task<Result> MoveAsync(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionMove move = new ActionMove();
            move.target = point;
            move.vehicle_id = id;
            return await Task.Run(() => SendPacket(WebActions.MOVE, move));
        }

        public Result Shoot(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionShoot shoot = new ActionShoot();
            shoot.target = point;
            shoot.vehicle_id = id;
            return SendPacket(WebActions.SHOOT, shoot);
        }

        public async Task<Result> ShootAsync(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionShoot shoot = new ActionShoot();
            shoot.target = point;
            shoot.vehicle_id = id;
            return await Task.Run(() => SendPacket(WebActions.SHOOT, shoot));
        }

        public async void Connect()
        {
            try
            {
                Log("Подключение к серверу...");
                var config = App.Host.Services.GetRequiredService<AppConfig>();

                IPAddress[] addresslist = Dns.GetHostAddresses(config.NetConfig.HostName);
                await Task.Run(() => { connect_(web, (uint)addresslist[0].Address, config.NetConfig.Port); });

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