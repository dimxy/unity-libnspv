package com.DefaultCompany.TestAndroidSO;

import com.unity3d.player.*;
import android.app.Activity;
import android.content.Intent;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.util.Log;
import android.content.res.AssetManager;

public class MyUnityPlayerActivity extends UnityPlayerActivity
{
    // Setup activity layout
    @Override protected void onCreate(Bundle savedInstanceState)
    {
		super.onCreate(savedInstanceState);
		Log.i("MyUnityPlayerActivity", "Overridden MyUnityPlayerActivity onCreate called!");

		AssetManager assetManager = getAssets();

		int rc = loadCoinsFile(assetManager);
		Log.i("MyUnityPlayerActivity", "loadCoinsFile rc=" + rc);
    }
    public static native int loadCoinsFile(AssetManager assetManager);

    /** Load jni .so on initialization */
    static {
         System.loadLibrary("nspv");
    }

}
