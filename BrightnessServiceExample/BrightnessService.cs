using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using BrightnessServiceExample.Interfaces;
using System.Linq;

namespace BrightnessServiceExample
{
    public class BrightnessService : INotifyPropertyChanged
    {
        public event Action BrightnessResolveTick;
        public List<Action> TickActions { get; set; }
        public double MillisecondResolution { get; set; }
        public bool Active { get; set; }

        public static double Brightness => DependencyService.Get<IBrightness>().CheckBrightness();

        private static readonly Lazy<BrightnessService> s_lazyBrightnessService = new Lazy<BrightnessService>(() => new BrightnessService());

        public static BrightnessService GetInstance() => s_lazyBrightnessService.Value;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SetValue<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return;
            backingField = value;
            OnPropertyChanged(propertyName);
        }

        public static List<Action> Attached() => GetInstance().TickActions;

        public static void Attach(Action brightnessServiceFunction)
        {
            BrightnessService singletonBrightness = GetInstance();
            singletonBrightness.Active = false;
            GetInstance().TickActions.Add(brightnessServiceFunction);
            singletonBrightness.Active = true;
            singletonBrightness.UpdateBrightness();
        }

        public static void OverwriteFunctions(List<Action> brightnessServiceFunctions)
        {
            BrightnessService singletonBrightness = GetInstance();
            singletonBrightness.Active = false;
            singletonBrightness.TickActions = brightnessServiceFunctions;
            singletonBrightness.Active = true;
            singletonBrightness.UpdateBrightness();
        }

        public static void OverwriteRefreshResolution(double millisecondResolution)
        {
            BrightnessService singletonBrightness = GetInstance();
            singletonBrightness.Active = false;
            singletonBrightness.MillisecondResolution = millisecondResolution;
            singletonBrightness.Active = true;
            singletonBrightness.UpdateBrightness();
        }


        private BrightnessService()
        {
            Active = true;
            MillisecondResolution = 100;
            TickActions = new List<Action>();
            UpdateBrightness();
            BrightnessResolveTick += BrightnessService_Ticked;
        }

        private void BrightnessService_Ticked()
        {
            TickActions.AsParallel().ForAll((Action tickAction) =>
            {
                try
                {
                    Device.BeginInvokeOnMainThread(() => tickAction.Invoke());
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"An error occurred when invoking a brightness Action: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        private void UpdateBrightness()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(MillisecondResolution), () =>
            {
                //if (DependencyService.Get<IBrightness>().BrightnessChanged())
                //{
                BrightnessResolveTick.Invoke();
                //}
                return Active;
            });
        }
    }
}
