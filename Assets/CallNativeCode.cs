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
	private static extern int uplugin_InitNSPV([MarshalAs(UnmanagedType.LPStr)]string sChainName, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	private static extern int uplugin_LoginNSPV([MarshalAs(UnmanagedType.LPStr)]string wifStr, StringBuilder errorStr);

	//[DllImport("kogsplugin")]
	//private static extern int uplugin_KogsList(out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	private static extern int uplugin_CallMethod([MarshalAs(UnmanagedType.LPStr)]string method, [MarshalAs(UnmanagedType.LPStr)]string jparams, out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	private static extern int uplugin_FinalizeCCTx([MarshalAs(UnmanagedType.LPStr)]string method, out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	private static extern int uplugin_BroadcastTx([MarshalAs(UnmanagedType.LPStr)]string method, out Int64 resultPtr, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	private static extern int uplugin_StringLength(Int64 stringPtr, out int length, StringBuilder errorStr);

	[DllImport("kogsplugin")]
	private static extern int uplugin_GetString(Int64 stringPtr, StringBuilder outStr, StringBuilder errorStr);

/*	[DllImport("kogsplugin")]
	private static extern int uplugin_TxnsCount(Int64 hexTxnsPtr, out int count, StringBuilder errorStr);
	[DllImport("kogsplugin")]
	private static extern int uplugin_GetTxid(Int64 hexTxnsPtr, int index, StringBuilder txidStr, StringBuilder errorStr);
	[DllImport("kogsplugin")]
	private static extern int uplugin_GetTxSize(Int64 hexTxnsPtr, int index, out int txSize, StringBuilder errorStr);
	[DllImport("kogsplugin")]
	private static extern int uplugin_GetTx(Int64 hexTxnsPtr, int index, StringBuilder txStr, StringBuilder errorStr);  */

	[DllImport("kogsplugin")]
	private static extern void uplugin_FinishNSPV();


	void OnGUI ()
	{
		//float a = 3;
		//float b = 10;
		//GUI.Label (new Rect (15, 125, 450, 100), "adding " + a  + " and " + b + " in native code equals " + add(a,b));

		string sChainName = "DIMXY11";
		//char [] chainName = sChainName.ToCharArray();
		
		//char[] dst = new char[200];
		//StringBuilder sbDst = new StringBuilder(256);
		StringBuilder sbErrorStr = new StringBuilder(128);
		int y = 125;

		//GUI.Label(new Rect(15, y, 450, 100), "Libadd: rc=" + add(a,b));
		//strcpy2(sbDst, sChainName, sChainName.Length+1);
		//Array.Resize(ref dst, sChainName.Length);
		//y += 30;
		//GUI.Label(new Rect(15, y, 450, 100), "Libstrcpy2:" + " string(chainName)=" + new string(chainName) + " sbDst.ToString()=" + sbDst.ToString());
		//y += 30;

		// call libkogsplugin functions:

		// init NSPV lib

		int rc = uplugin_InitNSPV(sChainName, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_InitNSPV rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		// login

		string wifStr = "UuKUSQHnRGk4CDbRnbLRrJHq5Dwx58qR9Q9K2VpJjn3APXLurNcu";
		rc = uplugin_LoginNSPV(wifStr, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_LoginNSPV rc=" + rc + " sbErrorStr=" + sbErrorStr.ToString());
		y += 30;

		// call kogskoglist:

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

		sbJsonResult = new StringBuilder(sLen);
		sbErrorStr = new StringBuilder(128);
		rc = uplugin_GetString(jtxPtr, sbJsonResult, sbErrorStr);
		GUI.Label(new Rect(15, y, 450, 100), "libkogsplugin: " + "uplugin_GetString rc=" + rc + " sbJsonResult=" + sbJsonResult.ToString() + " sbErrorStr=" + sbErrorStr.ToString());
		y += 100;
		

		//uplugin_FinishNSPV();
	}
}
