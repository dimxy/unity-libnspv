using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour {

	//[DllImport("native")]
	//private static extern float add(float x, float y);

	// simple test plugin
	[DllImport("add")]
	private static extern float add(float x, float y);

	// another simple test plugin
	[DllImport("strcpy2")]
	//private static extern void strcpy2([Out] char[] dst, [In] char[] src, int len);
	private static extern void strcpy2(StringBuilder sbDst, [In] [MarshalAs(UnmanagedType.LPStr)]string sChainName, int len);
	
	// -------------

	// kogs plugin imports
	[DllImport("kogsplugin")]
	// Initialize libnspv for a chain of sChainName. The chain of sChainName should be defined in the coins file (stored in android apk Assets)
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_InitNSPV([MarshalAs(UnmanagedType.LPStr)]string sChainName, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// Login to libnspv for a private key in wif format passed in wifStr
	// Note that login is valid for 777 sec and you need re-login after that time
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_LoginNSPV([MarshalAs(UnmanagedType.LPStr)]string wifStr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// Calls rpc method with params passed in jparams string which should contain json array
	// resultPtr is reference to the string with the json result returned from rpc
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_CallMethod([MarshalAs(UnmanagedType.LPStr)]string method, [MarshalAs(UnmanagedType.LPStr)]string jparams, out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// Finalizes a transaction created by a previously called rpc method (signs tx vins with user private key). 
	// jTxData is a string with json result object, it should be first retrieved from resultPtr by calls to uplugin_StringLength and uplugin_GetString
	// Please note that some rpcs return arrays of created transactions, so in such cases it is necessary to parse an array of txns and send only array element to uplugin_FinalizeCCTx as a string
	// resultPtr is reference to the string with the signed transaction 
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_FinalizeCCTx([MarshalAs(UnmanagedType.LPStr)]string jTxData, out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// Sends finalized (signed) tx to the chain.
	// You should get string with a signed tx from a resultPtr returned by uplugin_FinalizeCCTx with by calls to uplugin_StringLength and uplugin_GetString
	// A string with the signed tx is passed as hexTx param
	// resultPtr is reference to the string with json result object (which may have either txid or error adding the tx to the chain) 
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_BroadcastTx([MarshalAs(UnmanagedType.LPStr)]string hexTx, out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// queries the length in chars of a string result returned by kogplugin functions as resultPtr
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_StringLength(Int64 stringPtr, out int length, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// copies the string in resultPtr returned by kogplugin functions into StringBuilder outStr param
	// returns == 0 if success, if not 0, a err msg is returned in errorStr 
	private static extern int uplugin_GetString(Int64 stringPtr, StringBuilder outStr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	// de-initializes the libnspv, closes connections to the chain
	private static extern void uplugin_FinishNSPV();


	void OnGUI ()
	{
		//float a = 3;
		//float b = 10;
		//GUI.Label (new Rect (15, 125, 450, 100), "adding " + a  + " and " + b + " in native code equals " + add(a,b));

		int y = 125;


		// test strcpy strings

		//char[] dst = new char[200];
		//StringBuilder sbDst = new StringBuilder(256);
		//GUI.Label(new Rect(15, y, 450, 100), "Libadd: rc=" + add(a,b));
		//strcpy2(sbDst, sChainName, sChainName.Length+1);
		//Array.Resize(ref dst, sChainName.Length);
		//y += 30;
		//GUI.Label(new Rect(15, y, 450, 100), "Libstrcpy2:" + " string(chainName)=" + new string(chainName) + " sbDst.ToString()=" + sbDst.ToString());
		//y += 30;


		// sample calls to libkogsplugin functions:

		// init NSPV lib
		string sChainName = "DIMXY11";
		StringBuilder sbErrorStr = new StringBuilder(128);

		int rc = uplugin_InitNSPV(sChainName, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_InitNSPV rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		// login

		string wifStr = "UuKUSQHnRGk4CDbRnbLRrJHq5Dwx58qR9Q9K2VpJjn3APXLurNcu";
		rc = uplugin_LoginNSPV(wifStr, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_LoginNSPV rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		// call 'kogskoglist' rpc:

		Int64 jresultPtr;
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_CallMethod("kogskoglist", null, out jresultPtr, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_CallMethod('kogskoglist') rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		int sLen;
		
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_StringLength(jresultPtr, out sLen, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_StringLength rc=" + rc + " sLen=" + sLen + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		StringBuilder sbJsonResult = new StringBuilder(sLen);
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_GetString(jresultPtr, sbJsonResult, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_GetString rc=" + rc + " sbJsonResult=" + sbJsonResult.ToString() + " sbErrorStr=" + sbErrorStr.ToString());

		// call kogsstartgame
		Int64 jtxPtr;
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_CallMethod("kogsstartgame", 
			"[\"650dd21139e11798fd13869c66e92f6267432983ffb26d905474d09ae029c543\", [\"076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204\", \"ec5ecbe5f7e55e824afcfaf3a5e7b9dfa4fb896c4a31d367ecabd007b694e4d2\"] ]", 
			out jtxPtr, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_CallMethod('kogsstartgame') rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;

		//int sLen;
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_StringLength(jtxPtr, out sLen, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_StringLength rc=" + rc + " sLen=" + sLen + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		StringBuilder sbStartGameTx = new StringBuilder(sLen);
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_GetString(jtxPtr, sbStartGameTx, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_GetString rc=" + rc + " sbJsonResult=" + sbStartGameTx.ToString() + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;


		// call FinalizeCCTx
		Int64 signedTxPtr;
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_FinalizeCCTx(sbStartGameTx.ToString(),  out signedTxPtr, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_FinalizeCCTx rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;

		int signedTxLen;
		
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_StringLength(signedTxPtr, out signedTxLen, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_StringLength rc=" + rc + " signedTxLen=" + signedTxLen + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		StringBuilder sbSignedTx = new StringBuilder(signedTxLen);
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_GetString(signedTxPtr, sbSignedTx, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_GetString rc=" + rc + " sbSignedTx=" + sbSignedTx.ToString() + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;


		// call BroadcastTx
		Int64 broadcastResultPtr;
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_BroadcastTx(sbSignedTx.ToString(), out broadcastResultPtr, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_FinalizeCCtx rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;

		int broadcastResultLen;
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_StringLength(broadcastResultPtr, out broadcastResultLen, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_StringLength rc=" + rc + " broadcastResultLen=" + broadcastResultLen + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		StringBuilder sbBroascastResult = new StringBuilder(broadcastResultLen);
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_GetString(broadcastResultPtr, sbBroascastResult, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_GetString rc=" + rc + " sbBroascastResult=" + sbBroascastResult.ToString() + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;

		//uplugin_FinishNSPV();
	}
}
