using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BrightnessServiceExample
{
    public partial class BrightnessLogin : ContentPage, INotifyPropertyChanged
    {
        private Xamarin.Forms.Color _brightnessColour;
        public Xamarin.Forms.Color BrightnessColour
        {
            get => _brightnessColour;
            set => SetValue(ref _brightnessColour, value);
        }
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

        public BrightnessLogin()
        {
            InitializeComponent();
            BrightnessColour = Color.FromHex("#00A45A");
            BrightnessService.GetInstance();

            BrightnessService.Attach(() => BrightnessColour = BrightnessColour.WithLuminosity(BrightnessService.Brightness));
            BrightnessService.Attach(() => BrightnessColour = BrightnessColour.WithHue(BrightnessService.Brightness));
            BrightnessService.Attach(() => BrightnessColour = BrightnessColour.WithSaturation(1 - BrightnessService.Brightness));
        }
    }
}
