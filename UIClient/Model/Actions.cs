using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model
{
    public class Actions
    {
        public ActionNet[] actions { get; set; }
    }

    public class ActionNet
    {
        public int player_id { get; set; }
        public WebActions action_type { get; set; }
        public object data { get; set; }
    }

    public class ActionMove
    {
        public int vehicle_id { get; set; }
        public Point3 target { get; set; }
    }

    public class ActionShoot
    {
        public int vehicle_id { get; set; }
        public Point3 target { get; set; }
    }
}
