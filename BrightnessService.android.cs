using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;

using Android.Provider;
using Java.Interop;

using Plugin.CurrentActivity;

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

		private bool _androidLightSensorDirect;
		public bool AndroidLightSensorDirect
		{
			get => _androidLightSensorDirect;
			set => SetValue(ref _androidLightSensorDirect, value);
		}

		private double LastBrightness;
		LightDetector LightSensor = new LightDetector();

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

		static Handler s_handler;

		public BrightnessServiceImplementation()
		{
			LastBrightness = 0;
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
					BeginInvokeOnMainThread(() => tickAction.Invoke());
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occurred when invoking a brightness Action: {ex.Message}\n{ex.StackTrace}");
				}
			});
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
			try
			{
				if (AndroidLightSensorDirect)
				{
					return (float)double.Parse((LightSensor.lastLightValue).ToString());
				}
				else
				{
					return double.Parse(Settings.System.GetInt(CrossCurrentActivity.Current.AppContext.ContentResolver, Settings.System.ScreenBrightness).ToString()) / 255.0;
				}

			}
			catch (Exception)
			{
				return 1.0;
			}
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

			if (s_handler == null || s_handler.Looper != Looper.MainLooper)
			{
				s_handler = new Handler(Looper.MainLooper);
			}

			s_handler.Post(action);
		}

		private void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			var handler = new Handler(Looper.MainLooper);
			handler.PostDelayed(() =>
			{
				if (callback())
					StartTimer(interval, callback);

				handler.Dispose();
				handler = null;
			}, (long)interval.TotalMilliseconds);
		}
	}

	public class LightDetector : Java.Lang.Object, ISensorEventListener
	{
		public float lastLightValue { get; set; }
		public readonly SensorManager _sensorManager = (SensorManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.SensorService);

		public LightDetector()
		{
			try
			{
				lastLightValue = 1F;
				_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Light), SensorDelay.Ui);
			}
			catch (Exception ex)
			{
			}
		}


		public JniManagedPeerStates JniManagedPeerState => JniManagedPeerStates.Activatable;

		public void OnSensorChanged(SensorEvent e)
		{
			lastLightValue = e.Values[0];
		}

		public void SetJniIdentityHashCode(int value)
		{

		}

		public void SetPeerReference(JniObjectReference reference)
		{

		}

		public void SetJniManagedPeerState(JniManagedPeerStates value)
		{
			throw new NotImplementedException();
		}

		public void DisposeUnlessReferenced()
		{
			throw new NotImplementedException();
		}

		public void Disposed()
		{
			_sensorManager.Dispose();
		}

		public void Finalized()
		{
			throw new NotImplementedException();
		}

		public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{

		}
	}
}
