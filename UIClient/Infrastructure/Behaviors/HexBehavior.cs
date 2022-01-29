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
            //AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            //AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        }

        private async void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Hex hex) return;
            if (AssociatedObject.DataContext is not ViewModel.GamePageViewModel vm) return;
            //if (vm.Core.GameState.current_player_idx != vm.Core.Player.idx) return;

            //MessageBox.Show(hex.Point2.x.ToString() + " " + hex.Point2.y.ToString() + "\n" +
            //    hex.Point3.x.ToString() + " " + hex.Point3.y.ToString() + " " + hex.Point3.z.ToString());

            Hex last = vm.Core.SelectedHex;
            vm.Core.SelectedHex = hex;
            if (last == null) return;
            if (last.Tank == null) return;
            Tank tank = (Tank)last.Tank;
            if (tank.Vehicle.vehicle.player_id != vm.Core.Player.idx) return;

            Tank new_tank = (Tank)hex.Tank;

            if (new_tank == null)
            {
                Result res = await vm.Core.MoveAsync(tank.Vehicle.id, hex.Point3);

                if (res == Result.OKEY)
                {
                    vm.Core.TotalStep++;
                    vm.Core.Field.VehicleMove(tank.Vehicle.id, hex.Point3);
                    vm.Core.Log("Машина перемещена по x: " + hex.Point3.x + ", y: " + hex.Point3.y + ", z: " + hex.Point3.z);

                    if (vm.Core.TotalStep == 5)
                        vm.Core.TimerStop();

                    //vm.Core.VecEnable = false;
                    //res = await vm.Core.TurnAsync();
                    //res = await vm.Core.GetGameStateAsync();
                }
                else if (res == Result.BAD_COMMAND)
                {
                    vm.Core.Log(res.ToString());
                }
                else
                {
                    vm.Core.Log("Ход перешел другому");
                    vm.Core.TimerStop();
                }
            }
            else
            {
                if (new_tank.Vehicle.vehicle.player_id == vm.Core.Player.idx) return;
                Result res = await vm.Core.ShootAsync(tank.Vehicle.id, hex.Point3);
                if (res == Result.OKEY)
                {
                    vm.Core.TotalStep++;
                    vm.Core.Field.VehicleShoot(tank.Vehicle.id, hex.Point3);
                    res = await vm.Core.TurnAsync();
                    res = await vm.Core.GetGameStateAsync();
                }
                else // has be late
                {
                    res = await vm.Core.TurnAsync();
                    res = await vm.Core.GetGameStateAsync();
                }
            }
        }

        //private void AssociatedObject_SelectionChanged(object sender, RoutedEventArgs e)
        //{
        //    if (AssociatedObject.DataContext is PacketSwap pw) { pw.UpdateOffsetCoret(AssociatedObject.SelectionStart); return; }
        //    if (AssociatedObject.DataContext is PacketNet pn) { pn.UpdateOffsetCoret(AssociatedObject.SelectionStart); return; }
        //}

        //private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (AssociatedObject.DataContext is PacketSwap pw) { pw.UpdateHex(); return; }
        //    if (AssociatedObject.DataContext is PacketNet pn) { pn.UpdateHex(); return; }
        //}
        protected override void OnDetaching()
        {
            //AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            //AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
        }
    }
}
