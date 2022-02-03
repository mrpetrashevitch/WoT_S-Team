using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model.Client.Api
{
    public class WebClientDll
    {
        const string _PathCoreDll = "WebClient.dll";


        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        public  extern static IntPtr create();

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result destroy(IntPtr web);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result connect_(IntPtr web, uint addr, ushort port);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result send_packet(IntPtr web, Model.WebAction action, int size, IntPtr data, IntPtr out_size, IntPtr out_data);

        [DllImport(_PathCoreDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result get_action(int curr_player,
                                        IntPtr players, int players_size,
                                        IntPtr vehicle, int vehicle_size,
                                        IntPtr win_points, int win_points_size,
                                        IntPtr attack_matrix, int attack_matrix_size,
                                        IntPtr base_, int base_size, out action_ret actions);
    }
}
