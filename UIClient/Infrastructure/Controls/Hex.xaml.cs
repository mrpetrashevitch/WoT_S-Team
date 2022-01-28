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

namespace UIClient.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для CellHex.xaml
    /// </summary>
    public partial class Hex : UserControl
    {
        public enum HexType
        {
            Free,
            Nun,
            Water,
            Rock,
            Base,
            Spawn
        }

        static Dictionary<HexType, Style> Stats = new Dictionary<HexType, Style>()
        {
             { HexType.Free, Application.Current.FindResource("ButtonFreeHex") as Style },
             { HexType.Nun, Application.Current.FindResource("ButtonNunHex") as Style },
             { HexType.Water, Application.Current.FindResource("ButtonWaterHex") as Style },
             { HexType.Rock, Application.Current.FindResource("ButtonRockHex") as Style },
             { HexType.Base, Application.Current.FindResource("ButtonBaseHex") as Style },
             { HexType.Spawn, Application.Current.FindResource("ButtonSpawnHex") as Style }
        };

        public static double size_x = 40;
        public static double size_y = 35;

        public Hex(Point2 point2, Point3 point3, HexType type = HexType.Free)
        {
            InitializeComponent();
            Type = type;
            Point2 = point2;
            Point3 = point3;
        }

        public object Tank
        {
            get { return (object)GetValue(TankProperty); }
            set { SetValue(TankProperty, value); }
        }
        public static readonly DependencyProperty TankProperty =
            DependencyProperty.Register(nameof(Tank), typeof(object), typeof(Hex), new PropertyMetadata(default(object)));

        public HexType Type
        {
            get { return (HexType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(HexType), typeof(Hex), new PropertyMetadata(new PropertyChangedCallback(TypeChanged)));

        public static void TypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is Hex cell)
            {
                HexType type = (HexType)e.NewValue;
                cell.Btn.Style = Stats[type];
            }
        }

        public Point2 Point2 { get; set; }
        public Point3 Point3 { get; set; }
    }
}
