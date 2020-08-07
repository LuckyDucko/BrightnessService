
<br />
<p align="center">
  <h1 align="center">BrightnessService</h3>
  <p align="center">
    React to brightness changes in real-time
    <br />
  </p>
</p>



<!-- TABLE OF CONTENTS -->

## Table of Contents

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/1d6fc153e83044d39daf23b3383ab8e2)](https://www.codacy.com/manual/LuckyDucko/BrightnessService?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=LuckyDucko/BrightnessService&amp;utm_campaign=Badge_Grade)
[![nuget](https://img.shields.io/nuget/v/Plugin.BrightnessService.svg)](https://www.nuget.org/packages/Plugin.BrightnessService)

* [Getting Started](#getting-started)
  * [Installation](#installation)
* [Usage](#usage)
* [Contributing](#contributing)
* [License](#license)
* [Contact](#contact)
* [Acknowledgements](#acknowledgements)



<!-- ABOUT THE PROJECT -->
## About The Project

Brightness service aims to provide an interface where developers are able to create dynamic content based on the brightness of their app, 
allowing them to go far beyond a simple theme change.

It also includes an easy way for direct access to an android devices light sensor

![Gif Example](https://j.gifs.com/nx4PqD.gif)

<!-- GETTING STARTED -->
## Getting Started


### Installation

Simply ensure that you have `CrossBrightnessService.Current.Active = true;` with the package installed in both the Shared Platform and iOS/Android

<!-- USAGE EXAMPLES -->
## Usage

To get the above effect, i used this code snippet

```csharp
CrossBrightnessService.Current.Active = true;
CrossBrightnessService.Current.Attach(() =>
{
  var LocalBackgroundColour = Color.Green;
  LocalBackgroundColour = LocalBackgroundColour.AddLuminosity(CrossBrightnessService.Current.CheckBrightness());
  LocalBackgroundColour = LocalBackgroundColour.WithLuminosity(CrossBrightnessService.Current.CheckBrightness());
  LocalBackgroundColour = LocalBackgroundColour.WithHue(CrossBrightnessService.Current.CheckBrightness());
  LocalBackgroundColour = LocalBackgroundColour.WithSaturation(CrossBrightnessService.Current.CheckBrightness());
  BackgroundColour = LocalBackgroundColour;
});
```

# Thats it! For a quick explainer on the details, read on

Plugin.BrightnessService allows access through the `IBrightnessService` interface, which has the following members/functions available

**`public List<Action> TickActions { get; set; }`**
> This is the TickActions that will be executed in parallel on a timed basis, with the timing depending on `MillisecondResolution`. 
*All Functions are invoked on the main thread.*
While direct access through the setter is possible, it is recommended you use `Attach`, `Attached` and `OverWriteFunctions` for changing its values


**`public double MillisecondResolution { get; set; }`**
> This is the speed in which the timer will wait before executing `TickActions` in parallel. 
While direct access through the setter is possible, it is recommended you use `OverwriteRefreshResolution` for changing its value

**`public bool Active { get; set; }`**
> This determines if the timer is active or not.

**`public bool AndroidLightSensorDirect { get; set; }`**
> This determines if Android will use the screenbrightness of the device (safe), or the direct readouts from the light sensor (be prepared for calibration difficulties.). 

**`public event Action BrightnessResolveTick;`**
> This is what is invoked by the timer. Feel free to attach other things to run on the exact same timing as the brightness service. However, this also serves for how the tickactions get executed. care recommended.


**`public double CheckBrightness();`**
> This provides the brightness readout. if not using the Android direct light sensor, will be within 0-255

**`public bool BrightnessChanged();`**
> Provides a more manual approach to checking brightness. First call will 'set' the last brightness
each subsequent call will check against the last brightness recorded against the current light reading, if it is different, it will return true and update the last brightness to the current light reading


**`public List<Action> Attached();`**
>Returns all functions attached and running on `TickActions`

**`public void Attach(Action brightnessServiceFunction);`**
> Attaches a new function ontop of `TickActions`, briefly stopping and restarting the timer when doing so. 

**`public void OverwriteFunctions(List<Action> brightnessServiceFunctions);`**
> Overwrites `TickFunctions` with the supplied functions, briefly stopping and restarting the timer when doing so. 

**`public void OverwriteRefreshResolution(double millisecondResolution);`**
>Overwrites the refresh resolution to the supplied values, briefly stopping and restarting the timer when doing so. 



<!-- LICENSE -->
## License

This project uses the MIT License


<!-- CONTACT -->
## Contact

My [Github](https://github.com/LuckyDucko),
or reach me on the [Xamarin Slack](https://xamarinchat.herokuapp.com/),
or on my [E-mail](tyson@logchecker.com.au)

Project Link: [BrightnessService](https://github.com/LuckyDucko/BrightnessService)
