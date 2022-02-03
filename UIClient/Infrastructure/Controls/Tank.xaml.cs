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

namespace UIClient.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для Tank.xaml
    /// </summary>
    public partial class Tank : UserControl
    {
        public Tank(VehicleEx vehicle, Brush team_color)
        {
            InitializeComponent();
            HP = vehicle.vehicle.health;
            Vehicle = vehicle;
            SetVahicleType(vehicle.vehicle.vehicle_type);
            TeamBrush = team_color;
            TeamColor = ((SolidColorBrush)team_color).Color;
        }

        public void SetVahicleType(Model.Server.VehicleType type)
        {
            Uri uriSource;
            switch (type)
            {
                case Model.Server.VehicleType.СТ:
                    uriSource = new Uri("pack://application:,,,/Resources/Images/MT.png", UriKind.RelativeOrAbsolute);
                    HPMax = 2;
                    Speed = 2;
                    Damage = 1;
                    Gold = 2;
                    ShootMin = 2;
                    ShootMax = 2;
                    break;
                case Model.Server.VehicleType.ЛТ:
                    uriSource = new Uri("pack://application:,,,/Resources/Images/LT.png", UriKind.RelativeOrAbsolute);
                    HPMax = 1;
                    Speed = 3;
                    Damage = 1;
                    Gold = 1;
                    ShootMin = 2;
                    ShootMax = 2;
                    break;
                case Model.Server.VehicleType.ТТ:
                    uriSource = new Uri("pack://application:,,,/Resources/Images/HT.png", UriKind.RelativeOrAbsolute);
                    HPMax = 3;
                    Speed = 1;
                    Damage = 1;
                    Gold = 3;
                    ShootMin = 1;
                    ShootMax = 2;
                    break;
                case Model.Server.VehicleType.ПТ:
                    uriSource = new Uri("pack://application:,,,/Resources/Images/ATSPG.png", UriKind.RelativeOrAbsolute);
                    HPMax = 2;
                    Speed = 1;
                    Damage = 1;
                    Gold = 2;
                    ShootMin = 1;
                    ShootMax = 3;
                    break;
                case Model.Server.VehicleType.САУ:
                    uriSource = new Uri("pack://application:,,,/Resources/Images/SPG.png", UriKind.RelativeOrAbsolute);
                    HPMax = 1;
                    Speed = 1;
                    Damage = 1;
                    Gold = 1;
                    ShootMin = 3;
                    ShootMax = 3;
                    break;
                default:
                    throw new Exception("Error vehicle type");
            }

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = uriSource;
            logo.EndInit();
            Type = logo;
        }

        public VehicleEx Vehicle { get; set; }
        public int HPMax { get; set; }
        public int Speed { get; set; }
        public int Damage { get; set; }
        public int Gold { get; set; }
        public int ShootMin { get; set; }
        public int ShootMax { get; set; }

        public Color TeamColor
        {
            get { return (Color)GetValue(TeamColorProperty); }
            set { SetValue(TeamColorProperty, value); }
        }
        public static readonly DependencyProperty TeamColorProperty =
            DependencyProperty.Register(nameof(TeamColor), typeof(Color), typeof(Tank), new PropertyMetadata(default(Color)));

        public Brush TeamBrush
        {
            get { return (Brush)GetValue(TeamBrushProperty); }
            set { SetValue(TeamBrushProperty, value); }
        }
        public static readonly DependencyProperty TeamBrushProperty =
            DependencyProperty.Register(nameof(TeamBrush), typeof(Brush), typeof(Tank), new PropertyMetadata(default(Brush)));

        public ImageSource Type
        {
            get { return (ImageSource)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(ImageSource), typeof(Tank), new PropertyMetadata(default(ImageSource)));

        public int HP
        {
            get { return (int)GetValue(HPProperty); }
            set { SetValue(HPProperty, value); }
        }
        public static readonly DependencyProperty HPProperty =
            DependencyProperty.Register(nameof(HP), typeof(int), typeof(Tank), new PropertyMetadata(default(int)));

        public override string ToString()
        {
            return String.Concat("Type: ", Vehicle.vehicle.vehicle_type);
        }
    }
}
