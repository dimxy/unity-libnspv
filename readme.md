# Libnspv for Unity
version 0.0.1

## Description:
Komodo libnspv support for Unity game development platform<br>
Unity C# script wrappers for komodo kogs game rpc.<br>
Supported platforms: Currently supports Windows and Android platforms

## Content
Package content:
* coins - file with chain descriptions, required by libnspv
* Assets/StreamingAssets/data/coins - copy of 'coins' file accessible in Android apk
* Assets/CallNativeCode.unity - sample Unity scene to call 
* Assets/KogsWrapper.cs - kogs rpc wrapper C# code
* Assets/java/com/DefaultCompany/TestAndroidSO/MyUnityPlayerActivity.java - overloaded UnityPlayerActivity.java to get access to assetManager object
* libs/Windows - libnspv and kogs wrapper native libs for Windows (the libs should be placed into a dir which is in the OS Windows search PATH)
* Assets/Plugins/Android - libnspv and kogs wrapper native libs for Android

## Installation
Import this package into Unity project<br>
Set IL2CCP script in the Player setting<br>
For android set only arm64 architecture in the Player setting.<br>

To test - uncomment test code in KogsWrapper.cs and add the scene in the 'Build and Run' window

## Debug log
Where lib's log:
* on Windows the lib logs into nspv-debug.log file in the Unity project root directory
* on Android the lib logs into android log accessible remotely with 'adb logcat'.
