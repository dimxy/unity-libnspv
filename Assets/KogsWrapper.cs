using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Runtime.InteropServices;

using kogs;

namespace kogs
{

	[Serializable]
	class TxData
	{
		public string result;
		public string hexTx;
		public string sigData;
		public string error;
	}

	[Serializable]
	class BroadcastResult
	{
		public string result;
		public string expected;
		public string broadcast;
		public int retcode;
		public string type;
	}

	/*[Serializable]
	public class _txid
	{
		public string txid;
	};*/

	[Serializable]
	class TokenidsResult
	{
		public string[] tokenids;
	};

	[Serializable]
	class RpcRequest<T>
	{
		public RpcRequest(string _method) { method = _method; }
		public string method;
		public T @params;  // escaping keyword
	};

	[Serializable]
	class KogsBaseInfo
	{
		public string result;
		public string objectType;
		public string version;
		public string nameId;
		public string descriptionId;
		public string originatorPubKey;
	};

	[Serializable]
	class KogsGameStatus : KogsBaseInfo
	{
		[Serializable]
		class GameInfo
		{
			public string[] KogsWonByPlayerId;
			public string[] KogsWonByPlayerIdTotals;
			public string PreviousTurn;
			public string PreviousPlayerId;
			public string NextTurn;
			public string NextPlayerId;
			public string[] KogsInStack;
		}

		GameInfo gameinfo;
	};

	[Serializable]
	class KogsContainerInfo : KogsBaseInfo
	{
		public string[] tokenids;
	};

	[Serializable]
	class KogsPackInfo : KogsBaseInfo
	{
		// empty
	};

	[Serializable]
	class KogsPlayerInfo : KogsBaseInfo
	{
		// empty
	};

	// match-object is a kog or slammer
	[Serializable]
	class KogsMatchObjectInfo : KogsBaseInfo
	{
		public string imageId;
		public string setId;
		public string subsetId;
		public string printId;
		public string appearanceId;
		public string borderId;  // this is relevant only for slammer
	};


	// libnspv basic features:
	class NSPV
	{
		protected const int NSPV_MAXERRORLEN = 128;
		protected const string NSPV_SUCCESS = "success";
		protected const string NSPV_ERROR = "error";

		// kogs plugin imports
		[DllImport("kogsplugin")]
		// Initialize libnspv for a chain of sChainName. The chain of sChainName should be defined in the coins file (stored in android apk Assets)
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_InitNSPV([MarshalAs(UnmanagedType.LPStr)]string sChainName, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// Login to libnspv for a private key in wif format passed in wifStr
		// Note that login is valid for 777 sec and you need re-login after that time
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_LoginNSPV([MarshalAs(UnmanagedType.LPStr)]string wifStr, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// Calls rpc method with params passed in jparams string which should contain json array
		// resultPtr is reference to the string with the json result returned from rpc
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_CallRpcMethod([MarshalAs(UnmanagedType.LPStr)]string method, [MarshalAs(UnmanagedType.LPStr)]string jparams, out Int64 resultPtr, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// Calls rpc method with JSON request passed in jsonStr string param 
		// resultPtr is reference to the string with the json result returned from rpc
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_CallRpcWithJson([MarshalAs(UnmanagedType.LPStr)]string jsonStr, out Int64 resultPtr, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// Finalizes a transaction created by a previously called rpc method (signs tx vins with user private key). 
		// jTxData is a string with json result object, it should be first retrieved from resultPtr by calls to uplugin_StringLength and uplugin_GetString
		// Please note that some rpcs return arrays of created transactions, so in such cases it is necessary to parse an array of txns and send only array element to uplugin_FinalizeCCTx as a string
		// resultPtr is reference to the string with the signed transaction 
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_FinalizeCCTx([MarshalAs(UnmanagedType.LPStr)]string jTxData, out Int64 resultPtr, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// Sends finalized (signed) tx to the chain.
		// You should get string with a signed tx from a resultPtr returned by uplugin_FinalizeCCTx with by calls to uplugin_StringLength and uplugin_GetString
		// A string with the signed tx is passed as hexTx param
		// resultPtr is reference to the string with json result object (which may have either txid or error adding the tx to the chain) 
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_BroadcastTx([MarshalAs(UnmanagedType.LPStr)]string hexTx, out Int64 resultPtr, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// queries the length in chars of a string result returned by kogplugin functions as resultPtr
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_StringLength(Int64 stringPtr, out int length, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// copies the string in resultPtr returned by kogplugin functions into StringBuilder outStr param
		// returns == 0 if success, if not 0, a err msg is returned in errorStr 
		protected static extern int uplugin_GetString(Int64 stringPtr, StringBuilder outStr, StringBuilder errorStr);

		[DllImport("kogsplugin")]
		// de-initializes the libnspv, closes connections to the chain
		protected static extern void uplugin_FinishNSPV();

		protected static string NSPVPtr2String(Int64 resultPtr, out string errorStr)
		{
			int resultLen = 0;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);
			errorStr = "";
			int rc = uplugin_StringLength(resultPtr, out resultLen, sbErrorStr);
			if (rc == 0)
			{
				StringBuilder sbJsonResult = new StringBuilder(resultLen);
				sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);
				rc = uplugin_GetString(resultPtr, sbJsonResult, sbErrorStr);
				if (rc == 0)
				{
					// TODO: free NSPV result
					return sbJsonResult.ToString();
				}
				else
				{
					errorStr = "internal uplugin_GetString error: " + sbErrorStr.ToString();
					return null;
				}
			}
			else
			{
				errorStr = "internal uplugin_StringLength error: " + sbErrorStr.ToString();
				return null;
			}
		}

		// initializes libnspv for the sChainName which must be defined in the coins file
		public static int Init(string sChainName, out string errorStr)
		{
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);
			int rc = uplugin_InitNSPV(sChainName, sbErrorStr);
			errorStr = sbErrorStr.ToString();
			return rc;
		}

		// logs in to libnspv with wif privkey
		public static int Login(string wifStr, out string errorStr)
		{
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);
			int rc = uplugin_LoginNSPV(wifStr, sbErrorStr);
			errorStr = sbErrorStr.ToString();
			return rc;
		}

		// finalizes (signs the unsigned vins) txData object like { "hex": "1D456A53086..." }
		// returns signed tx in hex
		public static int FinalizeCCTx(string tx, out string signedTx, out string errorStr)
		{
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);
			Int64 signedTxPtr = 0;
			errorStr = "";
			signedTx = "";

			int rc = uplugin_FinalizeCCTx(tx, out signedTxPtr, sbErrorStr);
			if (rc == 0)
			{
				signedTx = NSPVPtr2String(signedTxPtr, out errorStr);
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// broadcast signed tx to the chain
		public static int BroadcastTx(string signedTx, out string txid, out string errorStr)
		{
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);
			Int64 broadcastResultPtr = 0;

			errorStr = "";
			txid = "";
			int rc = uplugin_BroadcastTx(signedTx, out broadcastResultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(broadcastResultPtr, out errorStr);
				BroadcastResult result = JsonUtility.FromJson<BroadcastResult>(jresult);
				if (result.retcode >= 0)
					txid = result.broadcast;
				else
				{
					errorStr = "tx broadcast error: " + result.type;
					rc = -1;
				}
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}
	};

	// kogs rpc methods
	class KogsRPC : NSPV
	{
		// rpc signature: 'kogskoglist [my]'
		public static int kogskoglist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogskoglist", onlyMy ? "my" : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsslammerlist [my]'
		public static int kogsslammerlist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsslammerlist", onlyMy ? "my" : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogscontainerlist [my]'
		public static int kogscontainerlist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogscontainerlist", onlyMy ? "my" : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogspacklist [my]'
		public static int kogspacklist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogspacklist", onlyMy ? "my" : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsplayerlist'
		public static int kogsplayerlist(out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsplayerlist", null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsgameconfiglist'
		public static int kogsgameconfiglist(out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsgameconfiglist", null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsgamelist [playerid]'
		public static int kogsgamelist(string playerid, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsgamelist", playerid, out tokenidsOut, out errorStr);
		}

		// internal universal method to call kogsxxxlist rpcs
		private static int kogsobjectlist(string method, string paramext, out string[] tokenidsOut, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			tokenidsOut = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>(method);
			if (paramext != null)
			{
				request.@params = new string[] { paramext };
			}

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);
			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				//string jresult = "[ \"111\", \"222\"]";
				// string jresult = "{ \"result\":{ \"kogids\":[\"10e4dfef7a81da3654f6c424ffe9d5a394f87650a83fd8eef5aa96746eda03fd\"]	}}";
				//string jresult = "{ \"result\":{ \"kogids\":[{\"txid\": \"10e4dfef7a81da3654f6c424ffe9d5a394f87650a83fd8eef5aa96746eda03fd\"}]	}}";

				/*KogIdsResult res = JsonUtility.FromJson<KogIdsResult>(jresult);
				kogidsOut = null; // res.result.kogids.txids;
				Debug.Log("size=" + res.result.kogids.Length);
				//Debug.Log("elem[0]=" + res.result.kogids[0].txid);
				Debug.Log("elem[0]=" + res.result.kogids[0]);*/

				//string []s = JsonUtility.FromJson<string[]>(jresult);
				//string[] arr = new string[2];
				//arr[0] = "s000001";
				//arr[1] = "s000002"; 
				// string json = JsonUtility.ToJson(arr);
				Debug.Log("jresult=" + jresult);
				TokenidsResult result = JsonUtility.FromJson<TokenidsResult>(jresult);
				tokenidsOut = result.tokenids;
			}
			else
				errorStr = sbErrorStr.ToString();

			//Debug.Log("rc=" + rc + " errorStr=" + sbErrorStr.ToString());
			return rc;
		}

		// rpc signature: 'kogsstartgame gameconfigid playerid1 playerid2 ...'
		public static int kogsstartgame(string gameid, string[] playerids, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsstartgame");
			request.@params = new string[1 + playerids.Length];
			request.@params[0] = gameid;
			for (int i = 0; i < playerids.Length; i++)
				request.@params[i + 1] = playerids[i];

			string requestStr = JsonUtility.ToJson(request);

			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;

			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsstartgame name desc params:{}'
		// note: no param at this time
		// returns txData to sign and broadcast
		public static int kogscreateplayer(string name, string desc, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogscreateplayer");
			request.@params = new string[3];
			request.@params[0] = name;
			request.@params[1] = desc;
			request.@params[2] = "{}";

			string requestStr = JsonUtility.ToJson(request);

			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;

			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsburntoken tokenid ...'
		// burns a token by sending to 'dead' addr
		// used to burn packs to unseal them
		// returns txData to sign and broadcast
		public static int kogsburntoken(string tokenid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsburntoken");
			request.@params = new string[] { tokenid };
			string requestStr = JsonUtility.ToJson(request);

			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogscreatecontainer name description playerid tokenid1 tokenid2 ...'
		// create a container
		// return tx to sign and broadcast
		public static int kogscreatecontainer(string name, string description, string playerid, string[] tokenids, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogscreatecontainer");
			request.@params = new string[3 + tokenids.Length];
			request.@params[0] = name;
			request.@params[1] = description;
			request.@params[2] = playerid;
			for (int i = 0; i < tokenids.Length; i++)
				request.@params[i + 3] = tokenids[i];

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsaddkogstocontainer containerid tokenid1 tokenid2 ... slammerid...'
		// add kogs to a container
		// return tx to sign and broadcast
		public static int kogsaddkogstocontainer(string containerid, string[] tokenids, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsaddkogstocontainer");
			request.@params = new string[1 + tokenids.Length];
			request.@params[0] = containerid;
			for (int i = 0; i < tokenids.Length; i++)
				request.@params[i + 1] = tokenids[i];

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsdepositcontainer gameid containerid '
		// deposits container to a game
		// return tx to sign and broadcast
		public static int kogsdepositcontainer(string gameid, string containerid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsdepositcontainer");
			request.@params = new string[2];
			request.@params[0] = gameid;
			request.@params[1] = containerid;

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsslamdata gameid playerid armheight armstrength'
		// sends slam data to a game
		// return tx to sign and broadcast
		public static int kogsslamdata(string gameid, string playerid, int armheight, int armstrength, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsslamdata");
			request.@params = new string[4];
			request.@params[0] = gameid;
			request.@params[1] = playerid;
			request.@params[2] = armheight.ToString();
			request.@params[3] = armstrength.ToString();

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				txData = jresult;
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}



		// rpc signature: 'kogsobjectinfo objectid'
		// returns parsed info about game object in baseInfo
		// the caller needs to query baseInfo.objectId param and cast to the corresponding type, like:
		// if (baseInfo.objectId == "G")
		//     KogsGameStatus gameStatus = (KogsGameStatus)baseInfo;
		public static int kogsobjectinfo(string objectid, out KogsBaseInfo baseInfo, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			baseInfo = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsobjectinfo");
			request.@params = new string[] { objectid };
			string requestStr = JsonUtility.ToJson(request);

			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);

				KogsBaseInfo baseInit = JsonUtility.FromJson<KogsBaseInfo>(jresult);
				if (baseInit != null && baseInit.objectType != null)
				{
					switch (baseInit.objectType)
					{
						case "G":
							KogsGameStatus gameStatus = JsonUtility.FromJson<KogsGameStatus>(jresult);
							baseInfo = gameStatus;
							break;
						case "W":
							KogsPlayerInfo playerInfo = JsonUtility.FromJson<KogsPlayerInfo>(jresult);
							baseInfo = playerInfo;
							break;
						case "P":
							KogsPackInfo packInfo = JsonUtility.FromJson<KogsPackInfo>(jresult);
							baseInfo = packInfo;
							break;
						case "C":
							KogsContainerInfo containerInfo = JsonUtility.FromJson<KogsContainerInfo>(jresult);
							baseInfo = containerInfo;
							break;
						case "K":
						case "S":
							KogsMatchObjectInfo matchobjectInfo = JsonUtility.FromJson<KogsMatchObjectInfo>(jresult);
							baseInfo = matchobjectInfo;
							break;
					}
					return 0;
				}
				else
				{
					errorStr = "could not parse the returned result object";
					return -1;
				}
			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}
	};
}

public class KogsWrapper : MonoBehaviour
{

	private static bool enterred = false;

	// run test calls to kogs blockchain rpcs
	/*	void OnGUI()
		{

			if (enterred) return;
			enterred = true;

			string[] ids;
			int rc;
			string err;
			string sChainName = "DIMXY11";
			string wifStr = "UuKUSQHnRGk4CDbRnbLRrJHq5Dwx58qR9Q9K2VpJjn3APXLurNcu";
			string txData = "";

			rc = NSPV.Init(sChainName, out err);
			Debug.Log("NSPV.Init rc=" + rc + " error=" + err);
			GUI.Label(new Rect(15, 30, 450, 100), "NSPV.Init rc=" + rc);

			rc = NSPV.Login(wifStr, out err);
			Debug.Log("NSPV.Login rc=" + rc + " error=" + err);

			rc = KogsRPC.kogskoglist(true, out ids, out err);
			Debug.Log("KogsRPC.kogskoglist rc=" + rc + " error=" + err + " ids.Length=" + (ids != null ? ids.Length : 0));

			rc = KogsRPC.kogscreateplayer("myname", "mydesc", out txData, out err);
			Debug.Log("KogsRPC.kogscreateplayer rc=" + rc + " error=" + err);

			string[] playerids = { "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204", "ec5ecbe5f7e55e824afcfaf3a5e7b9dfa4fb896c4a31d367ecabd007b694e4d2" };
			rc = KogsRPC.kogsstartgame("650dd21139e11798fd13869c66e92f6267432983ffb26d905474d09ae029c543", playerids, out txData, out err);
			Debug.Log("KogsRPC.kogsstartgame rc=" + rc + " error=" + err);

			KogsBaseInfo baseInfo;
			rc = KogsRPC.kogsobjectinfo("bee801d2f5d870a8d3e4ab282e1238560e7b16b078791cd33dc1134f6874e703", out baseInfo, out err);
			Debug.Log("KogsRPC.kogsobjectinfo rc=" + rc + " error=" + err + " baseInfo.objectId=" + (baseInfo != null ? baseInfo.objectType : "baseInfo-is-null"));
			if (baseInfo != null)
			{
				KogsGameStatus gameStatus = (KogsGameStatus)baseInfo;
				Debug.Log("KogsRPC.kogsobjectinfo baseInfo.objectId=" + baseInfo.objectType + " gameStatus.result=" + gameStatus.result);
			}
			else
				Debug.Log("KogsRPC.kogsobjectinfo baseInfo is null");

			rc = KogsRPC.kogsburntoken("10e4dfef7a81da3654f6c424ffe9d5a394f87650a83fd8eef5aa96746eda03fd", out txData, out err);
			Debug.Log("KogsRPC.kogsburntoken rc=" + rc + " error=" + err);

			rc = KogsRPC.kogsslamdata("bee801d2f5d870a8d3e4ab282e1238560e7b16b078791cd33dc1134f6874e703", "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204", 10, 15, out txData, out err);
			Debug.Log("KogsRPC.kogsslamdata rc=" + rc + " error=" + err);

			rc = KogsRPC.kogsgamelist("10f84ddc4b35287253aa44a7d1edb19d05a75854b3f36e8091972067350571fe", out ids, out err);
			Debug.Log("KogsRPC.kogsgamelist rc=" + rc + " error=" + err + " ids.Length=" + (ids != null ? ids.Length : 0));
		}*/
}
