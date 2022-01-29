using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model
{
    public struct player_native
    {
        public int idx;
        public int is_observer;
    }


    public enum vehicle_type : int
    {
        [EnumMember(Value = "medium_tank")]
        СТ,
        [EnumMember(Value = "light_tank")]
        ЛТ,
        [EnumMember(Value = "heavy_tank")]
        ТТ,
        [EnumMember(Value = "at_spg")]
        ПТ,
        [EnumMember(Value = "spg")]
        САУ
    }

    public struct vehicle_native
    {
        public int vehicle_id;
        public int player_id;
        public vehicle_type vehicle_type;
        public int health;
        public point spawn_position;
        public point position;
        public int capture_points;
    }

    public enum action_type : int
    {
        nun,
        move,
        shoot
    };

    public struct point
    {
        public int x, y, z;
    };

    public struct action
    {
        public action_type action_type;
        public int vec_id;
        public point point;
    };


    [StructLayout(LayoutKind.Sequential)]
    public struct action_ret
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public action[] actions;
    }

    public struct win_points_native
    {
        public int id;
        public int capture;
        public int kill;
    }

    public unsafe struct attack_matrix_native
    {
        public int id;
        public fixed int attack[3];
    }

}
