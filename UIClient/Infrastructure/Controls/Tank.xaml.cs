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
    /// Логика взаимодействия для Tank.xaml
    /// </summary>
    public partial class Tank : UserControl
    {
        public Tank(VehicleEx vehicle, Brush team_color)
        {
            InitializeComponent();
            HP = vehicle.vehicle.health;
            string type = vehicle.vehicle.vehicle_type;
            switch (type)
            {
                case "medium_tank": type = "СТ"; break;
                case "light_tank": type = "ЛТ"; break;
                case "heavy_tank": type = "ТТ"; break;
                case "at_spg": type = "ПТ"; break;
                case "spg": type = "САУ"; break;
                default: break;
            }
            Vehicle = vehicle;
            Type = type;
            TeamColor = team_color;
        }

        public VehicleEx Vehicle { get; set; }

        public Brush TeamColor
        {
            get { return (Brush)GetValue(TeamColorProperty); }
            set { SetValue(TeamColorProperty, value); }
        }
        public static readonly DependencyProperty TeamColorProperty =
            DependencyProperty.Register(nameof(TeamColor), typeof(Brush), typeof(Tank), new PropertyMetadata(default(Brush)));

        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(string), typeof(Tank), new PropertyMetadata(default(string)));

        public double HP
        {
            get { return (double)GetValue(HPProperty); }
            set { SetValue(HPProperty, value); }
        }
        public static readonly DependencyProperty HPProperty =
            DependencyProperty.Register(nameof(HP), typeof(double), typeof(Tank), new PropertyMetadata(default(double)));
    }
}
