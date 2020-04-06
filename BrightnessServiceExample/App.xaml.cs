using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrightnessServiceExample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new BrightnessLogin();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
