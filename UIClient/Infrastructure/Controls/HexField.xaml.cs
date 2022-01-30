using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIClient.Model;
using UIClient.Model.Client;
using UIClient.Model.Server;

namespace UIClient.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для CellField.xaml
    /// </summary>
    public partial class HexField : UserControl
    {
        static readonly Dictionary<int, Brush> team_colors = new Dictionary<int, Brush>()
        {
            { 0, Brushes.Aqua},
            { 1, Brushes.Red},
            { 2, Brushes.Lime}
        };

        public HexField()
        {
            InitializeComponent();
            Inited = false;
        }

        Hex[,] hex_field = null;
        bool even = false;
        int field_size = 0;
        int x = 0, y = 0;
        Point2 offset = new Point2();

        public Dictionary<int, PlayerEx> players = new Dictionary<int, PlayerEx>();
        Dictionary<int, VehicleEx> vehicles = new Dictionary<int, VehicleEx>();

        public bool Inited { get; set; }

        void CreateHexField(int size)
        {
            Canv.Children.Clear();
            x = 0; y = 0;
            offset.x = size - 1;
            offset.y = size - 1;
            FieldSizeX = (field_size + 1) * Hex.size_x * 0.75 - Hex.size_y / 2;
            FieldSizeY = (field_size) * Hex.size_y;

            int l = size - 2, r = size;
            int i = 0;

            for (; i < size / 2; i++)
            {
                for (int j = 0; j < field_size; j++)
                    if (j >= l && j <= r) AddHex(Hex.HexType.Free);
                    else AddHex(Hex.HexType.Nun);
                l -= 2;
                r += 2;
            }

            if (even)
            {
                for (; i < size + size / 2 - 1; i++)
                {
                    for (int j = 0; j < field_size; j++)
                        AddHex(Hex.HexType.Free);
                }
            }
            else
            {
                for (; i < size + size / 2; i++)
                {
                    for (int j = 0; j < field_size; j++)
                        AddHex(Hex.HexType.Free);
                }
            }

            l += 1;
            r -= 1;
            for (; i < size + size - 1; i++)
            {
                l += 2;
                r -= 2;

                for (int j = 0; j < field_size; j++)
                    if (j >= l && j <= r) AddHex(Hex.HexType.Free);
                    else AddHex(Hex.HexType.Nun);
            }
        }

        void CreateHexField2(int size)
        {
            Canv.Children.Clear();
            x = 0; y = 0;
            offset.x = size - 1;
            offset.y = size - 1;
            FieldSizeX = (field_size + 1) * Hex.size_x * 0.75;
            FieldSizeY = (field_size) * Hex.size_y;


            Random r = new Random();

            for (int i = 0; i < field_size * field_size;)
            {
                int l = r.Next(1, 15);
                Hex.HexType t = (Hex.HexType)r.Next(0, 4);
                for (int j = 0; j < l && i < field_size * field_size; j++, i++)
                    AddHex(t);
            }

            AddBase(new Point3 { x = -1, y = 0, z = 1 });
        }

        public Hex GetHex(Point3 p)
        {
            int of_x = offset.x + p.x;
            if (of_x < 0 || of_x > field_size - 1) return null;
            int of_y = offset.y + (p.z + (p.x - (p.x & 1)) / 2);
            if (of_y < 0 || of_y > field_size - 1) return null;
            return hex_field[of_y, of_x];
        }

        public int GetDistance(Point3 a, Point3 b)
        {
            return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2;
        }

        public List<Hex> GetHexAround(Point3 point, int n, int N)
        {
            List<Hex> res = new List<Hex>();
            if (n > N || n < 0 || N < 0) return res;

            Point3 hex = new Point3();
            for (int dx = -N; dx <= N; dx++)
            {
                for (int dy = -N; dy <= N; dy++)
                {
                    for (int dz = -N; dz <= N; dz++)
                    {
                        if (dx + dy + dz == 0)
                        {
                            hex.x = dx + point.x;
                            hex.y = dy + point.y;
                            hex.z = dz + point.z;
                            int d = GetDistance(point, hex);
                            if (d < n || d > N) continue;

                            var add_hex = GetHex(hex);
                            if (add_hex != null)
                                res.Add(add_hex);
                        }
                    }
                }
            }
            return res;
        }

        VehicleEx GetVehicle(Point3 p)
        {
            foreach (var item in vehicles)
            {
                var new_vec = item.Value.vehicle;
                if (new_vec.position.x == p.x &&
                    new_vec.position.y == p.y &&
                    new_vec.position.z == p.z)
                {
                    return item.Value;
                }
            }
            return null;
        }

        Point2 GetPoint2(Point3 p)
        {
            Point2 ret = new Point2();
            ret.x = offset.x + p.x;
            ret.y = offset.y + (p.z + (p.x - (p.x & 1)) / 2);
            return ret;
        }

        Point3 GetPoint3(Point2 p)
        {
            Point3 ret = new Point3();
            p.x -= offset.x;
            p.y -= offset.y;
            ret.x = p.x;
            ret.z = p.y - (p.x - (p.x & 1)) / 2;
            ret.y = -ret.x - ret.z;
            return ret;
        }

        public void CreateContent(GameState state)
        {
            if (state == null) return;
            if (state.players == null) return;
            PlayerCount = state.players.Length;
            if (state.num_players != state.players.Length) return;

            this.players.Clear();
            PlayerCount = 0;

            int i = 0;
            foreach (var item in state.players)
            {
                PlayerEx px = new PlayerEx();
                px.player = item;
                px.color = team_colors[i];
                this.players.Add(item.idx, px);
                i++;
                PlayerCount++;
            }

            vehicles.Clear();
            Dictionary<int, Vehicle> v = state.vehicles;
            if (v != null)
                foreach (var it in v)
                {
                    VehicleEx vx = new VehicleEx();
                    vx.vehicle = it.Value;
                    vx.hex = GetHex(vx.vehicle.position);
                    vx.id = it.Key;
                    Tank tank = new Tank(vx, players[vx.vehicle.player_id].color);
                    vx.hex.Tank = tank;
                    vehicles.Add(it.Key, vx);
                }

            Inited = true;
        }

        public void UpdateContent(GameState state)
        {
            Dictionary<int, Vehicle> v = state.vehicles;
            if (v != null)
                foreach (var i in v)
                {
                    //if (i.Value.player_id == App.Core.Player.idx)
                    //    continue;

                    var new_vec = i.Value;
                    var curr_vec = vehicles[i.Key];

                    if (new_vec.health != curr_vec.vehicle.health)
                    {
                        curr_vec.vehicle.health = new_vec.health;
                        ((Tank)curr_vec.hex.Tank).HP = new_vec.health;
                    }
                    if (new_vec.position.x != curr_vec.hex.Point3.x ||
                        new_vec.position.y != curr_vec.hex.Point3.y ||
                        new_vec.position.z != curr_vec.hex.Point3.z)
                    {
                        VehicleMove(i.Key, new_vec.position);
                    }
                }
        }

        public void VehicleMove(int id, Point3 point)
        {
            Hex to = GetHex(point);
            VehicleEx v = vehicles[id];
            Tank tank = (Tank)v.hex.Tank;

            if (to == v.hex) return;
            if (tank == null) return;
            if (to.Tank != null) return;

            v.hex.Tank = null;
            to.Tank = tank;
            v.hex = to;
            v.vehicle.position = to.Point3;
        }

        public void VehicleShoot(int id, Point3 point)
        {
            var target = GetVehicle(point);
            if (target == null) return;
            if (target.hex.Tank == null) return;

            Tank tank = (Tank)target.hex.Tank;
            tank.HP--;
            target.vehicle.health--;
            if (tank.HP == 0)
            {
                tank.HP = 2;
                target.vehicle.health = 2;
                VehicleMove(target.id, target.vehicle.spawn_position);
            }
        }

        public void AddBase(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.Base;
        }

        public void AddSpawn(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.Spawn;
        }

        public void CreateField(Map map)
        {
            if (map.size % 2 == 0) even = true;
            else even = false;
            field_size = map.size * 2 - 1;
            hex_field = new Hex[field_size, field_size];
            CreateHexField(map.size);

            if (map.content._base != null)
                foreach (var point3 in map.content._base)
                    AddBase(point3);

            if (map.spawn_points != null)
            {
                foreach (var player in map.spawn_points)
                {
                    if (player.medium_tank != null)
                        foreach (var spawn_point in player.medium_tank)
                            AddSpawn(spawn_point);
                    if (player.light_tank != null)
                        foreach (var spawn_point in player.light_tank)
                            AddSpawn(spawn_point);
                    if (player.heavy_tank != null)
                        foreach (var spawn_point in player.heavy_tank)
                            AddSpawn(spawn_point);
                    if (player.at_spg != null)
                        foreach (var spawn_point in player.at_spg)
                            AddSpawn(spawn_point);
                    if (player.spg != null)
                        foreach (var spawn_point in player.spg)
                            AddSpawn(spawn_point);
                }
            }

        }

        void AddHex(Hex.HexType type)
        {
            Point2 point2 = new Point2() { x = x, y = y };
            Hex bt = new Hex(point2, GetPoint3(point2), type);

            Canvas.SetLeft(bt, x * (Hex.size_x * 0.75));
            if (even)
            {
                if (x % 2 != 0) Canvas.SetTop(bt, y * Hex.size_y);
                else Canvas.SetTop(bt, y * Hex.size_y + (Hex.size_y / 2));
            }
            else
            {
                if (x % 2 == 0) Canvas.SetTop(bt, y * Hex.size_y);
                else Canvas.SetTop(bt, y * Hex.size_y + (Hex.size_y / 2));
            }
            Canv.Children.Add(bt);
            hex_field[y, x] = bt;
            x++;

            if (x == field_size)
            {
                y++;
                x = 0;
            }
        }

        public double FieldSizeX
        {
            get { return (double)GetValue(FieldSizeXProperty); }
            set { SetValue(FieldSizeXProperty, value); }
        }
        public static readonly DependencyProperty FieldSizeXProperty =
            DependencyProperty.Register(nameof(FieldSizeX), typeof(double), typeof(HexField), new PropertyMetadata(default(double)));

        public double FieldSizeY
        {
            get { return (double)GetValue(FieldSizeYProperty); }
            set { SetValue(FieldSizeYProperty, value); }
        }
        public static readonly DependencyProperty FieldSizeYProperty =
            DependencyProperty.Register(nameof(FieldSizeY), typeof(double), typeof(HexField), new PropertyMetadata(default(double)));

        public int PlayerCount
        {
            get { return (int)GetValue(PlayerCountProperty); }
            set { SetValue(PlayerCountProperty, value); }
        }
        public static readonly DependencyProperty PlayerCountProperty =
            DependencyProperty.Register(nameof(PlayerCount), typeof(int), typeof(HexField), new PropertyMetadata(default(int)));
    }
}
