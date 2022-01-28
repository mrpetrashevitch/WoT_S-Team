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
        //TcpClient tcpClient;


        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static IntPtr create();

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static Result destroy(IntPtr web);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static Result connect_(IntPtr web, uint addr, ushort port);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        extern static Result send_packet(IntPtr web, WebActions action, int size, IntPtr data, IntPtr out_size, IntPtr out_data);

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

            //tcpClient = new TcpClient();

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



                //var stream = tcpClient.GetStream();
                //stream.Write(act_bytes, 0, 4);
                //stream.Write(size_bytes, 0, 4);
                //stream.Write(size_msg, 0, size_msg.Length);
                //int packet_size = 8;
                //int total_read = 0;
                //while (total_read < packet_size)
                //{
                //    var read = stream.Read(buffer, total_read, packet_size - total_read);
                //    total_read += read;
                //    if (read == 0)
                //        break;   // connection was broken
                //}
                //res = (Result)BitConverter.ToInt32(buffer, 0);
                //packet_size = BitConverter.ToInt32(buffer, 4);
                //total_read = 0;
                //while (total_read < packet_size)
                //{
                //    var read = stream.Read(buffer, total_read, packet_size - total_read);
                //    total_read += read;
                //    if (read == 0)
                //        break;   // connection was broken
                //}
                //message = Encoding.UTF8.GetString(buffer, 0, packet_size);
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

                //await tcpClient.ConnectAsync(config.NetConfig.HostName, config.NetConfig.Port);

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
