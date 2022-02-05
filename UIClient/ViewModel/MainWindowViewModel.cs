using UIClient.Infrastructure.Command;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UIClient.View.Pages;
using UIClient.Model.Config;
using UIClient.Model;
using System.Threading;
using System.Reflection;

namespace UIClient.ViewModel
{
    internal class MainWindowViewModel : Base.ViewModelBase
    {
        #region Properties
        #region string Title : Заголовок акна
        private string _Title = "MainWindowViewModel";
        /// <summary>Заголовок акна</summary>
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }
        #endregion
        #region string Version : текущая версия приложения
        private string _Version;
        /// <summary>текущая версия приложения</summary>
        public string Version
        {
            get { return _Version; }
            set { Set(ref _Version, value); }
        }
        #endregion
        #region Page SelectedPage : Выбранная страница
        private Page _SelectedPage;
        /// <summary>выбранная страница</summary>
        public Page SelectedPage
        {
            get { return _SelectedPage; }
            set { Set(ref _SelectedPage, value); }
        }
        #endregion
        #region double Width : ширина окна
        private double _Width = 1200;
        /// <summary>ширина окна</summary>
        public double Width
        {
            get { return _Width; }
            set { Set(ref _Width, value); }
        }
        #endregion
        #region double Height : высота окна
        private double _Height = 700;
        /// <summary>высота окна</summary>
        public double Height
        {
            get { return _Height; }
            set { Set(ref _Height, value); }
        }
        #endregion
        #region double Opacity : прозрачность окна
        private double _Opacity;
        /// <summary>прозрачность окна</summary>
        public double Opacity
        {
            get { return _Opacity; }
            set { Set(ref _Opacity, value); }
        }
        #endregion
        #endregion

        #region Commands
        #region CloseAppCommand : Закрытие порграммы
        /// <summary>Закрытие порграммы</summary>
        public ICommand CloseAppCommand { get; }
        private bool CanCloseAppCommandExecute(object p) => true;
        private void OnCloseAppCommandExecuted(object p)
        {
            Application.Current.Shutdown();
        }
        #endregion
        #region HideAppCommand : Свернуть программу
        /// <summary>Свернуть программу</summary>
        public ICommand HideAppCommand { get; }
        private bool CanHideAppCommandExecute(object p) => true;
        private void OnHideAppCommandExecuted(object p)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        #endregion

        #region AboutCommand : о программе
        /// <summary>о программе</summary>
        public ICommand AboutCommand { get; }
        private bool CanAboutCommandExecute(object p) => true;
        private void OnAboutCommandExecuted(object p)
        {
            string text = "Сборка: " + Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture;
            text += "\n" + "Разработано в рамках WG Forge:";
            text += "\n" + "@mrpetrashevitch";
            text += "\n" + "@Artyom-Master";
            text += "\n" + "@banany2001";
            MessageBox.Show(text, "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #endregion

        public void SelectGamePage()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var page = App.Host.Services.GetRequiredService<LoadPageViewModel>();
                page.IsLoadVisible = Visibility.Visible;
                page.IsControlVisible = Visibility.Hidden;

                //Task.Delay(500);

                Opacity = 0;
                SelectedPage = App.Host.Services.GetRequiredService<GamePage>();
                //Task.Delay(500);

                Width = 1200;
                Height = 820;
                Opacity = 1;
            }));
        }

        public void SelectLoadPage()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var page = App.Host.Services.GetRequiredService<LoadPageViewModel>();
                page.IsLoadVisible = Visibility.Hidden;
                page.IsControlVisible = Visibility.Visible;

                Opacity = 0;
                //Task.Delay(500);
                SelectedPage = App.Host.Services.GetRequiredService<LoadPage>();
                //Task.Delay(500);
                Width = 650;
                Height = 250;
                Opacity = 1;
            }));
        }

        public MainWindowViewModel()
        {
            #region Properties
            Opacity = 1;
            Width = 650;
            Height = 250;
            Title = "World of Tanks: Strategy";
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SelectedPage = App.Host.Services.GetRequiredService<LoadPage>();

            #region Commands
            CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
            HideAppCommand = new LambdaCommand(OnHideAppCommandExecuted, CanHideAppCommandExecute);
            AboutCommand = new LambdaCommand(OnAboutCommandExecuted, CanAboutCommandExecute);
            #endregion

            App.Host.Services.GetRequiredService<GamePage>();

            //load config
            App.Host.Services.GetRequiredService<AppConfig>();

            #endregion

        }
    }
}
