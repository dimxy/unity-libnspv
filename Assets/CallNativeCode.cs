using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour {

	private static bool enterred = false;

	// run test calls to kogs blockchain rpcs
	void OnGUI()
	{
		//float a = 3;
		//float b = 10;
		//GUI.Label (new Rect (15, 125, 450, 100), "adding " + a  + " and " + b + " in native code equals " + add(a,b));

		if (enterred) return;

		enterred = true;

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
		/*		
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
				*/

		// test 'params':
		env<string[]> r = new env<string[]>();
		r.method = "method1";
		string[] arr = new string[2];
		arr[0] = "s000001";
		arr[1] = "s000002"; 
		r.@params = arr;
		string json = JsonUtility.ToJson(r);
		Debug.Log("arr to json=" + json);


		string[] ids;
		int rc;
		string err;
		string sChainName = "DIMXY11";
		string wifStr = "UuKUSQHnRGk4CDbRnbLRrJHq5Dwx58qR9Q9K2VpJjn3APXLurNcu";
		string txData = "";

		rc = NSPV.Init(sChainName, out err);
		Debug.Log("NSPV.Init rc=" + rc + " error=" + err);

		rc = NSPV.Login(wifStr, out err);
		Debug.Log("NSPV.Login rc=" + rc + " error=" + err);

		rc = KogsRPC.kogskoglist(true, out ids, out err);
		Debug.Log("KogsRPC.kogskoglist rc=" + rc + " error=" + err + " ids.Length=" + (ids != null ? ids.Length : 0));

		string[] playerids = { "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204", "ec5ecbe5f7e55e824afcfaf3a5e7b9dfa4fb896c4a31d367ecabd007b694e4d2" };
		rc = KogsRPC.kogsstartgame("650dd21139e11798fd13869c66e92f6267432983ffb26d905474d09ae029c543", playerids, out txData, out err);
		Debug.Log("KogsRPC.kogsstartgame rc=" + rc + " error=" + err);

		KogsBaseInfo baseInfo;
		rc = KogsRPC.kogsobjectinfo("bee801d2f5d870a8d3e4ab282e1238560e7b16b078791cd33dc1134f6874e703", out baseInfo, out err);
		Debug.Log("KogsRPC.kogsobjectinfo rc=" + rc + " error=" + err + " baseInfo.objectId=" + (baseInfo != null ? baseInfo.objectType : "baseInfo-is-null"));
		if (baseInfo != null) {
			KogsGameStatus gameStatus = (KogsGameStatus)baseInfo;
			Debug.Log("KogsRPC.kogsobjectinfo baseInfo.objectId=" + baseInfo.objectType + " gameStatus.result=" + gameStatus.result);
		}
		else
			Debug.Log("KogsRPC.kogsobjectinfo baseInfo is null");

		rc = KogsRPC.kogsburntoken("10e4dfef7a81da3654f6c424ffe9d5a394f87650a83fd8eef5aa96746eda03fd", out txData, out err);
		Debug.Log("KogsRPC.kogsburntoken rc=" + rc + " error=" + err);

		rc = KogsRPC.kogsslamdata("bee801d2f5d870a8d3e4ab282e1238560e7b16b078791cd33dc1134f6874e703", "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204", 10, 15, out txData, out err);
		Debug.Log("KogsRPC.kogsslamdata rc=" + rc + " error=" + err);

	}
}
