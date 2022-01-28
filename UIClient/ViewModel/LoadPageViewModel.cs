using UIClient.Infrastructure.Command;
using UIClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using UIClient.View.Pages;

namespace UIClient.ViewModel
{
    class LoadPageViewModel : Base.ViewModelBase
    {
        #region Properties

        #region string Title : Заголовок акна
        private string _Title;
        /// <summary>Заголовок акна</summary>
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }
        #endregion

        #region Core Core : ядро
        private Core _Core;
        /// <summary>ядро</summary>
        public Core Core
        {
            get { return _Core; }
            set { Set(ref _Core, value); }
        }
        #endregion


        #region Visibility IsLoadVisible : summury
        private Visibility _IsLoadVisible;
        /// <summary>summury</summary>
        public Visibility IsLoadVisible
        {
            get { return _IsLoadVisible; }
            set { Set(ref _IsLoadVisible, value); }
        }
        #endregion

        #region Visibility IsControlVisible : summury
        private Visibility _IsControlVisible;
        /// <summary>summury</summary>
        public Visibility IsControlVisible
        {
            get { return _IsControlVisible; }
            set { Set(ref _IsControlVisible, value); }
        }
        #endregion

        //..
        #endregion

        #region Commands


        //..
        #endregion

        public LoadPageViewModel()
        {
            #region Properties
            Title = "Load";
            IsLoadVisible = Visibility.Hidden;
            Core = App.Core;
            
            //..
            #endregion

            #region Commands

            //..
            #endregion
        }
    }
}
