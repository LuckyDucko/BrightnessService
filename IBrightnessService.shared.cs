using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.BrightnessService
{
	public interface IBrightnessService
	{
		public List<Action> TickActions { get; set; }
		public double MillisecondResolution { get; set; }
		public bool Active { get; set; }
		public event Action BrightnessResolveTick;

		public double CheckBrightness();
		public bool BrightnessChanged();
		public List<Action> Attached();
		public void Attach(Action brightnessServiceFunction);
		public void OverwriteFunctions(List<Action> brightnessServiceFunctions);
		public void OverwriteRefreshResolution(double millisecondResolution);
	}
}
