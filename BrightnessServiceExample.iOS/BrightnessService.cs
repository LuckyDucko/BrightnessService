
using Xamarin.Forms;
using Foundation;
using UIKit;
using BrightnessServiceExample.Interfaces;
using BrightnessServiceExample;

[assembly: Dependency(typeof(BrightnessService))]
namespace BrightnessServiceExample.iOS
{
    [Preserve(AllMembers = true)]
    public class BrightnessService : IBrightness
    {
        private double LastBrightness = 0;

        public BrightnessService()
        {
            LastBrightness = CheckBrightness();
        }

        public bool BrightnessChanged()
        {
            if (LastBrightness.ToString() != CheckBrightness().ToString())
            {
                LastBrightness = CheckBrightness();
                return true;
            }
            else
            {
                return false;
            }

        }
        public double CheckBrightness()
        {
            return UIScreen.MainScreen.Brightness;
        }
    }

}
