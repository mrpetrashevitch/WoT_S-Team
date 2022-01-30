using Microsoft.Xaml.Behaviors;
using UIClient.Model;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using UIClient.Infrastructure.Controls;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UIClient.Infrastructure.Behavior
{
    public class HexBehavior : Behavior<Hex>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        }

        private async void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Hex curr_hex) return;
            if (AssociatedObject.DataContext is not ViewModel.GamePageViewModel vm) return;
            if (vm.Core.GameState.current_player_idx != vm.Core.Player.idx) return;

            Hex last_hex = vm.Core.SelectedHex;
            vm.Core.SelectedHex = curr_hex;
            if (last_hex == null) return;
            if (last_hex.Tank == null) return;
            Tank tank = (Tank)last_hex.Tank;
            if (tank.Vehicle.vehicle.player_id != vm.Core.Player.idx) return;

            Tank new_tank = (Tank)curr_hex.Tank;

            if (new_tank == null)
            {
                await vm.Core.MoveAsync(tank.Vehicle.id, curr_hex.Point3).ConfigureAwait(false);
            }
            else
            {
                if (new_tank.Vehicle.vehicle.player_id == vm.Core.Player.idx) return;
                await vm.Core.ShootAsync(tank.Vehicle.id, curr_hex.Point3).ConfigureAwait(false);
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
        }
    }
}
