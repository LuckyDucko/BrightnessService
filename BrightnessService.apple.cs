using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Foundation;

using UIKit;

namespace Plugin.BrightnessService
{
	/// <summary>
	/// Interface for BrightnessService
	/// </summary>
	public class BrightnessServiceImplementation : IBrightnessService
	{
		private List<Action> _tickActions;
		public List<Action> TickActions
		{
			get => _tickActions;
			set => SetValue(ref _tickActions, value);
		}

		private double _millisecondResolution;
		public double MillisecondResolution
		{
			get => _millisecondResolution;
			set => SetValue(ref _millisecondResolution, value);
		}

		private bool _active;
		public bool Active
		{
			get => _active;
			set => SetValue(ref _active, value);
		}
		public bool AndroidLightSensorDirect { get; set; }

		private double LastBrightness = 0;

		public event Action BrightnessResolveTick;

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

		public BrightnessServiceImplementation()
		{
			Active = true;
			MillisecondResolution = 100;
			TickActions = new List<Action>();
			UpdateBrightness();
			BrightnessResolveTick += BrightnessService_Ticked;
		}

		public void Attach(Action brightnessServiceFunction)
		{
			Active = false;
			TickActions.Add(brightnessServiceFunction);
			Active = true;
			UpdateBrightness();
		}

		public List<Action> Attached()
		{
			return TickActions;
		}

		public bool BrightnessChanged()
		{

			if (LastBrightness != CheckBrightness())
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

		public void OverwriteFunctions(List<Action> brightnessServiceFunctions)
		{
			Active = false;
			TickActions = brightnessServiceFunctions;
			Active = true;
			UpdateBrightness();
		}

		public void OverwriteRefreshResolution(double millisecondResolution)
		{
			Active = false;
			MillisecondResolution = millisecondResolution;
			Active = true;
			UpdateBrightness();
		}

		private void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			NSTimer timer = NSTimer.CreateRepeatingTimer(interval, t =>
			{
				if (!callback())
					t.Invalidate();
			});
			NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
		}

		private void UpdateBrightness()
		{
			StartTimer(TimeSpan.FromMilliseconds(MillisecondResolution), () =>
			{
				BrightnessResolveTick.Invoke();
				return Active;
			});
		}

		private void BeginInvokeOnMainThread(Action action)
		{
			NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
		}

		private void BrightnessService_Ticked()
		{
			TickActions.AsParallel().ForAll((Action tickAction) =>
			{
				try
				{
					BeginInvokeOnMainThread(() => tickAction.Invoke());
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occurred when invoking a brightness Action: {ex.Message}\n{ex.StackTrace}");
				}
			});
		}
	}
}
