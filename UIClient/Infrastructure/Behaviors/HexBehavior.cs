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
using System.Collections.Generic;
using UIClient.Model.Server;

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

            // clear last hex select
            foreach (var item in vm.Core.SelectedCanMove)
                item.CanMove = Visibility.Hidden;
            foreach (var item in vm.Core.SelectedCanShoot)
                item.CanShoot = Visibility.Hidden;
            vm.Core.SelectedCanMove.Clear();
            vm.Core.SelectedCanShoot.Clear();

            Tank tank = (Tank)curr_hex.Tank;
            if (tank != null && tank.Vehicle.vehicle.player_id == vm.Core.Player.idx)
            {
                vm.Core.SelectedCanMove = vm.Core.Field.GetHexAround(curr_hex.Point3, 1, 2);
                foreach (var item in vm.Core.SelectedCanMove)
                    if (item.Tank == null) item.CanMove = Visibility.Visible;

                vm.Core.SelectedCanShoot = vm.Core.Field.GetHexAround(curr_hex.Point3, 2, 2);
                foreach (var item in vm.Core.SelectedCanShoot)
                    if (item.Tank != null)
                        if (((Tank)item.Tank).Vehicle.vehicle.player_id != vm.Core.Player.idx)
                            item.CanShoot = Visibility.Visible;
            }

            Hex last_hex = vm.Core.SelectedHex;
            vm.Core.SelectedHex = curr_hex;
            if (last_hex == null) return;
            if (last_hex.Tank == null) return;

            tank = (Tank)last_hex.Tank;
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
