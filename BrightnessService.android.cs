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

		private double LastBrightness;
		LightDetector LightSensor = new LightDetector();
		private ulong MeanValue { get; set; }
		private ulong MeanCount { get; set; }

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
			MeanValue = 1;
			MeanCount = 1;
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
				//if (Settings.System.GetInt(CrossCurrentActivity.Current.AppContext.ContentResolver, Settings.System.ScreenBrightnessMode) == (int)ScreenBrightness.ModeAutomatic)
				//{
				//    var stuff = NormaliseBrightness((float)double.Parse((LightSensor.lastLightValue / LightSensor._sensorManager.GetDefaultSensor(SensorType.Light).MaximumRange).ToString()));
				//    var stuff2 = double.Parse(Settings.System.GetInt(CrossCurrentActivity.Current.AppContext.ContentResolver, Settings.System.ScreenBrightness).ToString()) / 255.0;
				//    Console.WriteLine(stuff.ToString());
				//    return stuff;
				//}
				//else
				//{
				return double.Parse(Settings.System.GetInt(CrossCurrentActivity.Current.AppContext.ContentResolver, Settings.System.ScreenBrightness).ToString()) / 255.0;
				//}

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

		private bool BrightnessServiceActive()
		{
			if (!Settings.System.CanWrite(CrossCurrentActivity.Current.AppContext))
			{
				Intent intent = new Intent(Settings.ActionManageWriteSettings);
				intent.SetData(Android.Net.Uri.Parse("package:" + CrossCurrentActivity.Current.Activity.PackageName));
				CrossCurrentActivity.Current.Activity.StartActivity(intent);
				return false;
			}
			else
			{
				return true;
			}
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
		private float NormaliseBrightness(float lightReading)
		{
			MeanValue += (ulong)lightReading;
			MeanCount += 1;

			return (MeanValue + lightReading) / MeanCount;
		}

		private int AccessTable(int lightReading)
		{
			return lightReadingToBrightnessCurve[lightReading > 220 ? 220 : lightReading, 0];
		}

		private static int[,] lightReadingToBrightnessCurve ={
			{1, 0},
			{1, 6},
			{2, 11},
			{3, 15},
			{4, 19},
			{5, 22},
			{6, 24},
			{7, 27},
			{8, 29},
			{9, 31},
			{10, 33},
			{11, 34},
			{12, 36},
			{13, 38},
			{14, 39},
			{15, 41},
			{16, 42},
			{17, 43},
			{18, 45},
			{19, 46},
			{20, 47},
			{21, 49},
			{22, 50},
			{23, 51},
			{24, 52},
			{25, 53},
			{26, 54},
			{27, 55},
			{28, 56},
			{29, 57},
			{30, 57},
			{31, 57},
			{32, 57},
			{33, 60},
			{34, 60},
			{35, 61},
			{36, 62},
			{37, 62},
			{38, 63},
			{39, 63},
			{40, 64},
			{41, 64},
			{42, 65},
			{43, 65},
			{44, 66},
			{45, 66},
			{46, 67},
			{47, 67},
			{48, 68},
			{49, 68},
			{50, 69},
			{51, 69},
			{52, 70},
			{53, 70},
			{54, 70},
			{55, 71},
			{56, 71},
			{57, 71},
			{58, 72},
			{59, 72},
			{60, 72},
			{61, 73},
			{62, 73},
			{63, 73},
			{64, 74},
			{65, 74},
			{66, 74},
			{67, 75},
			{68, 75},
			{69, 75},
			{70, 75},
			{71, 76},
			{72, 76},
			{73, 76},
			{74, 76},
			{75, 76},
			{76, 77},
			{77, 77},
			{78, 77},
			{79, 78},
			{80, 78},
			{81, 78},
			{82, 78},
			{83, 78},
			{84, 79},
			{85, 79},
			{86, 80},
			{87, 80},
			{88, 80},
			{89, 80},
			{90, 80},
			{91, 81},
			{92, 81},
			{93, 81},
			{94, 81},
			{95, 81},
			{96, 82},
			{97, 82},
			{98, 82},
			{99, 82},
			{100, 82},
			{101, 83},
			{102, 83},
			{103, 83},
			{104, 83},
			{107, 84},
			{109, 84},
			{112, 85},
			{114, 85},
			{117, 85},
			{119, 86},
			{122, 87},
			{124, 87},
			{127, 87},
			{130, 88},
			{132, 88},
			{133, 88},
			{134, 88},
			{135, 88},
			{136, 88},
			{137, 88},
			{138, 89},
			{139, 89},
			{140, 89},
			{141, 89},
			{142, 89},
			{143, 89},
			{144, 89},
			{145, 90},
			{146, 90},
			{147, 90},
			{148, 90},
			{149, 90},
			{150, 90},
			{151, 90},
			{152, 91},
			{153, 91},
			{154, 91},
			{155, 91},
			{156, 91},
			{157, 91},
			{158, 91},
			{159, 91},
			{160, 91},
			{161, 91},
			{162, 92},
			{163, 92},
			{164, 92},
			{165, 92},
			{166, 92},
			{167, 92},
			{168, 92},
			{169, 92},
			{170, 92},
			{171, 93},
			{172, 93},
			{173, 93},
			{174, 93},
			{175, 93},
			{176, 93},
			{177, 93},
			{178, 93},
			{179, 93},
			{180, 94},
			{181, 94},
			{182, 94},
			{183, 94},
			{184, 94},
			{185, 94},
			{186, 94},
			{187, 94},
			{188, 94},
			{189, 94},
			{190, 95},
			{191, 95},
			{192, 95},
			{193, 95},
			{194, 95},
			{195, 95},
			{196, 95},
			{197, 95},
			{198, 95},
			{199, 95},
			{200, 96},
			{201, 96},
			{202, 96},
			{203, 96},
			{204, 96},
			{205, 96},
			{206, 96},
			{207, 96},
			{208, 96},
			{209, 96},
			{210, 96},
			{211, 96},
			{212, 97},
			{213, 97},
			{214, 97},
			{215, 97},
			{216, 97},
			{217, 97},
			{218, 97},
			{219, 97},
			{220, 97},
			{221, 97},
			{222, 97},
			{223, 98},
			{224, 98},
			{225, 98},
			{226, 98},
			{227, 98},
			{228, 98},
			{229, 98},
			{230, 98},
			{231, 98},
			{232, 98},
			{233, 98},
			{234, 98},
			{235, 99},
			{236, 99},
			{237, 99},
			{238, 99},
			{239, 99},
			{240, 99},
			{241, 99},
			{242, 99},
			{243, 99},
			{244, 99},
			{245, 99},
			{246, 99},
			{247, 99},
			{248, 100},
			{249, 100},
			{250, 100},
			{251, 100},
			{252, 100},
			{253, 100},
			{254, 100},
			{255, 100},
			{255, 100}};

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
			throw new NotImplementedException();
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
