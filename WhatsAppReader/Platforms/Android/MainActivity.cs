﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using static Android.Provider.Telephony.Mms;
namespace WhatsAppReader;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
}
