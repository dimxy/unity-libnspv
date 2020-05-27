/******************************************************************************
* Copyright � 2014-2020 The SuperNET Developers.                             *
*                                                                            *
* See the AUTHORS, DEVELOPER-AGREEMENT and LICENSE files at                  *
* the top-level directory of this distribution for the individual copyright  *
* holder information and the developer policies on copyright and licensing.  *
*                                                                            *
* Unless otherwise agreed in a custom licensing agreement, no part of the    *
* SuperNET software, including this file may be copied, modified, propagated *
* or distributed except according to the terms contained in the LICENSE file *
*                                                                            *
* Removal or modification of this copyright notice is prohibited.            *
*                                                                            *
******************************************************************************/

// Unity C# wrapper for libnspv calls

using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Runtime.InteropServices;

using kogs;

namespace kogs
{

	[Serializable]
	class BroadcastResult
	{
		public string result;
		public string expected;
		public string broadcast;
		public int retcode;
		public string type;
	}

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
		public class GameInfo
		{
			public string[] KogsWonByPlayerId;
			public string[] KogsWonByPlayerIdTotals;
			public string PreviousTurn;
			public string PreviousPlayerId;
			public string NextTurn;
			public string NextPlayerId;
			public string[] KogsInStack;
			public bool finished;
			public string WinnerId;
			public string[] players;
		}

		public GameInfo gameinfo;
	};

	[Serializable]
	class KogsContainerInfo : KogsBaseInfo
	{
		public string[] tokenids;
	};

	[Serializable]
	class KogsAdvertisedPlayer
	{
		public string playerid;
		public string []opts;  // contains list of supported modes
	}

	[Serializable]
	class KogsAdvertisedInfo : KogsBaseInfo
	{
		public KogsAdvertisedPlayer[] advertisedPlayerList;
	};

	/*[Serializable]
	class KogsContainerResult 
	{
		public string result;
		public string error;
		public TxData[] hextxns;
	};*/


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

	[Serializable]
	class TokenOrder
	{
		public string funcid;
		public string txid;
		public int vout;
		public string amount;
		public string askamount;
		public string origaddress;  
		public string origtokenaddress;  
		public string tokenid;  
		public string totalrequired;  
		public string price;  
	};

	[Serializable]
	class TokenOrdersResult
	{
		public TokenOrder[] result; // that 'result' is in the json
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
		public static extern int uplugin_CallRpcMethod([MarshalAs(UnmanagedType.LPStr)]string method, [MarshalAs(UnmanagedType.LPStr)]string jparams, out Int64 resultPtr, StringBuilder errorStr);

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

		// protected
		public static string NSPVPtr2String(Int64 resultPtr, out string errorStr)
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
			Debug.Log("BroadcastTx signedTx is null=" + (signedTx == null).ToString() + " signedTx=" + signedTx);
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

		// end of init
		public static void Finish()
		{
			uplugin_FinishNSPV();
		}
	};

	// kogs rpc methods
	class KogsRPC : NSPV
	{
		// game modes:
		public const string OPT_PLAYFORKEEPS = "playforkeeps";
		public const string OPT_PLAYFORFUN = "playforfun";
		public const string OPT_PLAYFORWAGES = "playforwages";

		// rpc signature: 'kogskoglist [my]'
		public static int kogskoglist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogskoglist", onlyMy ? new string[]{"my"} : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsslammerlist [my]'
		public static int kogsslammerlist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsslammerlist", onlyMy ? new string[]{"my"} : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogscontainerlist [my]'
		public static int kogscontainerlist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogscontainerlist", onlyMy ? new string[]{"my"} : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogspacklist [my]'
		public static int kogspacklist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogspacklist", onlyMy ? new string[]{"my"} : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsplayerlist'
		public static int kogsplayerlist(bool onlyMy, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsplayerlist", onlyMy ? new string[]{"my"} : null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsgameconfiglist'
		public static int kogsgameconfiglist(out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsgameconfiglist", null, out tokenidsOut, out errorStr);
		}

		// rpc signature: 'kogsgamelist [playerid1] [playerid2]'
		public static int kogsgamelist(string playerid1, string playerid2, out string[] tokenidsOut, out string errorStr)
		{
			return kogsobjectlist("kogsgamelist", new string[]{ playerid1, playerid2 }, out tokenidsOut, out errorStr);
		}

		// internal universal method to call kogsxxxlist rpcs
		private static int kogsobjectlist(string method, string[] extparams, out string[] tokenidsOut, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			tokenidsOut = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>(method);
			System.Collections.Generic.List<string> notnull = new System.Collections.Generic.List<string>();
			if (extparams != null)
			{
				for (int i = 0; i < extparams.Length && extparams[i] != null ; i ++)
					notnull.Add(extparams[i]);
			}
			request.@params = notnull.ToArray();

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);
			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
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

		// rpc signature: 'kogscreateplayer name desc params:{}'
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

		private static void ParseHextxns(string sHextxns, out string []txDataArr)
		{
			txDataArr = null;
			const string hextxnsPattern = "\"hextxns\":";
			int hextxnsIndex = sHextxns.IndexOf(hextxnsPattern);
			if (hextxnsIndex > 0)
			{
				int leftArrIndex = sHextxns.IndexOf("[", hextxnsIndex + hextxnsPattern.Length);
				int bracketLev = 1;
				int parenthLev = 0;
				int leftObjIndex = 0, rightObjIndex = 0;
				ArrayList txDataList = new ArrayList();

				// search for txDatas as objects in the array [{ }, { },...]
				for (int i = leftArrIndex + 1; i < sHextxns.Length; i++)
				{
					if (sHextxns[i] == '[')
					{
						bracketLev++;
					}
					else if (sHextxns[i] == ']')
					{
						bracketLev--;
						if (bracketLev == 0)  // found topmost array end
							break;
					}
					else if (sHextxns[i] == '{')
					{
						parenthLev++;
						if (parenthLev == 1)
							leftObjIndex = i;
					}
					else if (sHextxns[i] == '}')
					{
						parenthLev--;
						if (parenthLev == 0)
						{
							rightObjIndex = i;
							txDataList.Add(sHextxns.Substring(leftObjIndex, rightObjIndex - leftObjIndex + 1));
						}
					}
					else if (sHextxns[i] == '"')
					{
						i++;
						while (i < sHextxns.Length && sHextxns[i] != '"') i++;   // skip quoted strings
					}
				}
				txDataArr = (string [])txDataList.ToArray(typeof(String));
			}
		}

		// rpc signature: 'kogscreatecontainer name description playerid tokenid1 tokenid2 ...'
		// create a container
		// return tx to sign and broadcast
		public static int kogscreatecontainer(string name, string description, string playerid,  out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogscreatecontainer");
			request.@params = new string[3];
			request.@params[0] = name;
			request.@params[1] = description;
			request.@params[2] = playerid;


			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string sResult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("sResult=" + sResult);
				string[] txDataArr = null;
				ParseHextxns(sResult, out txDataArr);
				if (txDataArr != null)
				{
					for (int i = 0; i < txDataArr.Length; i++)
					{
						Debug.Log("txDataArr[" + i + "]=" + txDataArr[i]);
					}
					txData = txDataArr[0];
				}
				else
					Debug.Log("txDataArr is null");

			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsaddkogstocontainer containerid tokenid1 tokenid2 ... slammerid...'
		// add kogs to a container
		// return tx to sign and broadcast
		public static int kogsaddkogstocontainer(string containerid, string []tokenids, out string []txDataArr, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txDataArr = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsaddkogstocontainer");
			request.@params = new string[1 + tokenids.Length];
			request.@params[0] = containerid;
			//request.@params[1] = tokenid;
			for (int i = 0; i < tokenids.Length; i++)
				request.@params[i + 1] = tokenids[i];

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string sResult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("sResult=" + sResult);
				//string[] txDataArr = null;
				ParseHextxns(sResult, out txDataArr);
				if (txDataArr != null)
				{
					for (int i = 0; i < txDataArr.Length; i++)
					{
						Debug.Log("txDataArr[" + i + "]=" + txDataArr[i]);
					}
					//txData = txDataArr[0];
				}
				else
					Debug.Log("txDataArr is null");

			}
			else
				errorStr = sbErrorStr.ToString();
			return rc;
		}

		// rpc signature: 'kogsremovekogsfromcontainer containerid tokenid1 tokenid2 ... slammerid...'
		// removes kogs from a container
		// return tx to sign and broadcast
		public static int kogsremovekogsfromcontainer(string containerid, string[] tokenids, out string[] txDataArr, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txDataArr = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsremovekogsfromcontainer");
			request.@params = new string[1];
			request.@params[0] = "{ \"containerid\":\""+ containerid +"\", \"tokenids\": [";
			for (int i = 0; i < tokenids.Length; i++)
			{
				request.@params[0] += "\"" + tokenids[i] + "\"";
				if (i < tokenids.Length - 1)
					request.@params[0] += ", ";
			}
			request.@params[0] += "] }";

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string sResult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("sResult=" + sResult);
				//string[] txDataArr = null;
				ParseHextxns(sResult, out txDataArr);
				if (txDataArr != null)
				{
					for (int i = 0; i < txDataArr.Length; i++)
					{
						Debug.Log("txDataArr[" + i + "]=" + txDataArr[i]);
					}
					//txData = txDataArr[0];
				}
				else
					Debug.Log("txDataArr is null");

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

		// rpc signature: 'kogsdepositcontainer gameid containerid slammerid'
		// deposits container and slammer to a game
		// return tx to sign and broadcast
		public static int kogsdeposittokens(string gameid, string containerid, string slammerid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsdeposittokens");
			request.@params = new string[3];
			request.@params[0] = gameid;
			request.@params[1] = containerid;
			request.@params[2] = slammerid;

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

		// rpc signature: kogsadvertiseplayer '{"playerid": "id", "playforkeeps":"true", "playforfun":"true", "playforwages":"true" }
		// creates advertise player tx
		// opts is list of modes  KogsRPC.OPT_PLAYFORKEEPS, OPT_PLAYFORFUN, OPT_PLAYFORWAGES
		// returns tx to sign and broadcast
		public static int kogsadvertiseplayer(string playerid, string[] opts, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsadvertiseplayer");
			request.@params = new string[1];
			request.@params[0] = "{ \"playerid\"" + ":" + "\"" + playerid + "\"" + ", \"opts\": [" ;
			for (int i = 0; i < opts.Length; i++)
			{
				request.@params[0] += "\"" + opts[i] + "\"";
				if (i < opts.Length - 1)
					request.@params[0] += ", ";
			}
			request.@params[0] += "] }";

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

		// kogsadvertisedplayerlist rpc, call with no params
		// returns json with array KogsAdvertisedPlayer
		public static int kogsadvertisedplayerlist(out KogsAdvertisedPlayer [] adlistOut, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			adlistOut = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsadvertisedplayerlist");

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);
			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				KogsAdvertisedInfo result = JsonUtility.FromJson<KogsAdvertisedInfo>(jresult);
				adlistOut = result.advertisedPlayerList;
			}
			else
				errorStr = sbErrorStr.ToString();

			return rc;
		}

		// rpc signature: kogsstopadvertiseplayer playerid
		// creates stop advertise player tx
		public static int kogsstopadvertiseplayer(string playerid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogsstopadvertiseplayer");
			request.@params = new string[] { playerid };

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

		// rpc signature: 'kogscreatefirstbaton gameid'
		public static int kogscreatefirstbaton(string gameid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("kogscreatefirstbaton");
			request.@params = new string[1];
			request.@params[0] = gameid;

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

		// rpc signature: tokenask number tokenid price
		// creates a tx for token order selling a kog or pack
		public static int tokenask(int number, string tokenid, double price, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("tokenask");
			request.@params = new string[] { number.ToString(), tokenid, price.ToString() };

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

		// rpc signature: tokenbid number tokenid price
		// creates a tx for token order buying a kog or pack
		public static int tokenbid(int number, string tokenid, double price, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("tokenbid");
			request.@params = new string[] { number.ToString(), tokenid, price.ToString() };

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

		// rpc signature: tokenfillask tokenid orderid number
		// creates a tx to fill ask order
		public static int tokenfillask(string tokenid, string orderid, int num, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("tokenfillask");
			request.@params = new string[] { tokenid, orderid, num.ToString() };

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
		// rpc signature: tokenfillbid tokenid orderid number
		// creates a tx to fill bid order
		public static int tokenfillbid(string tokenid, string orderid, int num, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("tokenfillbid");
			request.@params = new string[] { tokenid, orderid, num.ToString() };

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
		// rpc signature: tokencancelask tokenid orderid 
		// creates a tx to cancel ask order
		public static int tokencancelask(string tokenid, string orderid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("tokencancelask");
			request.@params = new string[] { tokenid, orderid };

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

		// rpc signature: tokencancelbid tokenid orderid 
		// creates a tx to cancel bid order
		public static int tokencancelbid(string tokenid, string orderid, out string txData, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			txData = "";
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request = new RpcRequest<string[]>("tokencancelbid");
			request.@params = new string[] { tokenid, orderid };

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

		// rpc signature: tokenorders tokenid|* 0xF4 
		// or: mytokenorders 0xF4 
		// creates a tx to cancel bid order
		public static int tokenordersf4(bool onlyMy, out TokenOrder[] tokenordersOut, out string errorStr)
		{
			Int64 jresultPtr;
			errorStr = "";
			tokenordersOut = null;
			StringBuilder sbErrorStr = new StringBuilder(NSPV_MAXERRORLEN);

			RpcRequest<string[]> request;
			if (onlyMy) {
				request = new RpcRequest<string[]>("mytokenorders");
				request.@params = new string[] { "0xF4" };
			}
			else {
				request = new RpcRequest<string[]>("tokenorders");
				request.@params = new string[] { "*", "0xF4" };
			}

			string requestStr = JsonUtility.ToJson(request);
			Debug.Log("rpc request=" + requestStr);

			int rc = uplugin_CallRpcWithJson(requestStr, out jresultPtr, sbErrorStr);
			if (rc == 0)
			{
				string jresult = NSPVPtr2String(jresultPtr, out errorStr);
				Debug.Log("jresult=" + jresult);
				TokenOrdersResult r = JsonUtility.FromJson<TokenOrdersResult>(jresult);  
				tokenordersOut = r.result;
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
	void OnGUI()
	{

		if (enterred) return;
		enterred = true;

		Debug.Log("OnGUI() started");
		StartCoroutine( RunTests() );
	}

	IEnumerator RunTests()
	{ 
		string[] kogids, containerids, packids, playerids;
		int rc;
		string err;
		//string sChainName = "RFOXLIKE"; //  "DIMXY11"; // "RFOXLIKE";
		string sChainName = "DIMXY14"; //  "DIMXY11"; // "RFOXLIKE";

		//string wifStr = "UuKUSQHnRGk4CDbRnbLRrJHq5Dwx58qR9Q9K2VpJjn3APXLurNcu";  // test "034777b18effce6f7a849b72de8e6810bf7a7e050274b3782e1b5a13d0263a44dc"
		//string wifStr = "UpUhjzv1x6gQoiRL6GkM4Yb44uYPjxshqigVdNSaUqpwDkoqFsGm";   // RTbiYv9u1mrp7TmJspxduJCe3oarCqv9K4
		//string wifStr = "Utgyem1EBZ42eEiuSF3cJT9m4VhjN27Z7vXWC9zRzMXhLa6ZLKBF";  //p1  028e65778cd99898eea7073789359c55e67bdd78643263abf6328888f566d56f19
		//string wifStr = "Uu64bT9NDRTZQDSBxfrKbtRcYvv7qYk2RotQzsJfntJsMENuKrja";  //p2  02e1bb3f95f46fd89a93c8fe39c6e287c8beef659b7277791345b1b1aaa68a19b3
																				 //string wifStr = "UvchGG2gYsTgsKA4vCAp4UNHAn6gLgUrRKEbcrjqzAFhbu8fqzUD";  // my test key 025fa5b41da1e4cb9b9af345dddd2a4c35feb5030580e1fa40faaf387957b36f41
																				 //string wifStr = "UpUdyyTPFsXv8s8Wn83Wuc4iRsh5GDUcz8jVFiE3SxzFSfgNEyed";  // sys pk
		// string wifStr = "UpUdyyTPFsXv8s8Wn83Wuc4iRsh5GDUcz8jVFiE3SxzFSfgNEyed"; // sys
		// string wif1 = "UpUhjzv1x6gQoiRL6GkM4Yb44uYPjxshqigVdNSaUqpwDkoqFsGm";
   		// "address": "RTbiYv9u1mrp7TmJspxduJCe3oarCqv9K4",
   		// "pubkey": "025f97b6c42409e8e69eb2fdab281219aafe15169deec801ee621c63cc1ba0bb8c",
		// string wif2 = "UvchGG2gYsTgsKA4vCAp4UNHAn6gLgUrRKEbcrjqzAFhbu8fqzUD";
   		// "address": "RLNXzPsfWkRvNbUcargXJchRsWXGSg7U4L",
   		// "pubkey": "025fa5b41da1e4cb9b9af345dddd2a4c35feb5030580e1fa40faaf387957b36f41",

		rc = NSPV.Init(sChainName, out err);
		Debug.Log("NSPV.Init rc=" + rc + " error=" + err);
		GUI.Label(new Rect(15, 30, 450, 100), "NSPV.Init rc=" + rc);

		yield return StartCoroutine( runTestGame() );
		//yield return StartCoroutine( runDelays() );

		//Debug.Log("after StartCoroutine runDelays");
		//StartCoroutine( runFinish() );
		Debug.Log("after runTestGame");

		//NSPV.Finish();
		///Debug.Log("NSPV.Finish exited"); 

		// doSlam(wif2, "0dc05b25129d6fe4035922c129549021afe192c90752f5fb3a66a36d240131e1", "9db526ecca080b3ad53558c98276e54d9ff7f850610a22dde6760fc2e659c017");

		//yield break;
		/*
		string txData = "";
		string signedTx;
		string errorStr;
		string txid = ""; 
		*/
		/*
		rc = NSPV.Login(wif2, out err);
		Debug.Log("NSPV.Login rc=" + rc + " error=" + err);  
		*/
		/*
		rc = KogsRPC.kogskoglist(true, out kogids, out err);
		Debug.Log("KogsRPC.kogskoglist rc=" + rc + " error=" + err + " kogids.Length=" + (kogids != null ? kogids.Length : 0));

		yield break;
		*/
		/*
		NSPV.Finish();
		Debug.Log("NSPV.Finish exited");
		*/
		/*
		//rc = KogsRPC.kogscontainerlist(true, out containerids, out err);
		//Debug.Log("KogsRPC.kogscontainerlist rc=" + rc + " error=" + err + " containerids.Length=" + (containerids != null ? containerids.Length : 0));
		*/

		/*		
		rc = KogsRPC.kogsplayerlist(true, out playerids, out err);
		Debug.Log("KogsRPC.kogsplayerlist rc=" + rc + " error=" + err + " playerids.Length=" + (playerids != null ? playerids.Length : 0));
		*/
		
		/*		
		TokenOrder[] orders;
		rc = KogsRPC.tokenordersf4(true, out orders, out err);
		Debug.Log("KogsRPC.tokenordersf4 rc=" + rc + " error=" + err + " orders.Length=" + (orders != null ? orders.Length : 0));
		for (int i = 0; i < orders.Length; i++)
		{
			Debug.Log("orders[" + i +"] funcid=" + orders[i].funcid + " txid=" + orders[i].txid + " tokenid=" + orders[i].tokenid + " amount=" + orders[i].amount + " required=" + orders[i].totalrequired + " price=" + orders[i].price);
		}
		*/		
		/*
		rc = KogsRPC.tokenask(1, kogids[0], 0.0001, out txData, out err);
		Debug.Log("KogsRPC.tokenask rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		*/
		/*
		rc = KogsRPC.tokenfillask("bec426003de0fdb7fdd4e16da7f954a6b5610c0b2c184e99f1d7c4d7238e6098", "c5e49f0d821c3a674bdc5d4c42cb1d24096b8892aec9f21ed2a0bca6d8d6ebbb", 1, out txData, out err);
		Debug.Log("KogsRPC.tokenfillask rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		*/
		/*
		if (orders.Length > 0)  {	
			rc = KogsRPC.tokencancelask(orders[0].tokenid, orders[0].txid, out txData, out err);
			Debug.Log("KogsRPC.tokencancelask rc=" + rc + " error=" + err);
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			NSPV.BroadcastTx(signedTx, out txid, out errorStr);
			Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		}
		*/		
		/*
		rc = KogsRPC.tokenbid(1, kogids[0], 0.0001, out txData, out err);
		Debug.Log("KogsRPC.tokenbid rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		*/
		/*
		rc = KogsRPC.tokenfillbid("2df6b5d82e5cb1296de709b2cdbc1e67d734d82c6af4928e328d16010055c162", "a68eaf1839fc9326119e52afed2d0b0bb623e4ca3d8bb0f8e07139e25dc03da7", 1, out txData, out err);
		Debug.Log("KogsRPC.tokenfillbid rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		*/

		/*
		rc = KogsRPC.kogspacklist(true, out packids, out err);
		Debug.Log("KogsRPC.kogspacklist rc=" + rc + " error=" + err + " packids.Length=" + (packids != null ? packids.Length : 0));
		*/

		/*
		rc = KogsRPC.kogscreateplayer("player2-004", "d", out txData, out err);
		Debug.Log("KogsRPC.kogscreateplayer rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx rc=" + rc + " errorStr=" + errorStr + " txid=" + txid);	*/	

		/*	
		rc = KogsRPC.kogsslamdata("ef0b61644b1bb97972ea29d43feecb4ff84a6fb9cde09003c9b3615dad7a5d42", "9db526ecca080b3ad53558c98276e54d9ff7f850610a22dde6760fc2e659c017", 50, 50, out txData, out errorStr);
		Debug.Log("kogsslamdata rc=" + rc + " error=" + errorStr);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx rc=" + rc + " errorStr=" + errorStr + " txid=" + txid);
		*/
 
		//string myplayerid = "f6889d933dbc06be34c601d97492cc769ec438f2a0a348161e4fc8bea76ac354"; //"4f5272588fd9586d2e5ea45edd4279d63571fde9512216d3792ece9f91e27ca7";
		//string player1 = "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204";
		//string[] tokens = new string[]  {"42943d0582528ecc6ea41a7c9b9c5916a2f58cfae06a13c6108b614f1caeae60" };  // in container already
		//string[] tokens = new string[] { "204e78060d9889df77f3406066e9c14bfbc99546529350012e62a8c02314d92a" };
		//string[] tokens = new string[] { "3fef198a90fdef3ba0c61563a9f596f98df901a4aa0f9d48f216076cff7beef8", "8119dc39c023e08aeddb51827e62b69ccf49b51bdf9e27197f12ec5d61c0b522" };
		/*
		string[] tokens = new string[] {  "b4d324183df02e056f2e9ff2959dc79d0b4d48f25db18d511535cf543c488ee5" };
		string[] txDataArr = null;

		rc = KogsRPC.kogsgamelist(myplayerid, out ids, out err);
		Debug.Log("KogsRPC.kogsgamelist rc=" + rc + " error=" + err + " ids.Length=" + (ids != null ? ids.Length : 0));
		*/

		//string myplayerid = "79452a5e38e65972bed57b160ce7e82c07e814398d95a1f4a3bbbe031f0b5b5b";  //my test wif 025fa5b41da1e4cb9b9af345dddd2a4c35feb5030580e1fa40faaf387957b36f41
		//string myplayerid = "8fe319bf7f663738dfbf4a422a74dc285197b096faa03214c76a07d3eb490c59";  // wif1
		//string myplayerid = "f4d5c5467e9dc92fdcac48f4a13638f205d606f09136cba9fcf8bcd56621d195";   // wif2

		/*rc = KogsRPC.kogsburntoken(kogids[0], out txData, out err);
		Debug.Log("KogsRPC.kogsburntoken rc=" + rc + " error=" + err);
		if (txData != null)
		{
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr + " signedTx=" + signedTx);
			//NSPV.BroadcastTx(signedTx, out txid, out errorStr);
			//Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		}*/

		//string myplayerid = "86b3339d2553f8636225a4e5c34706aade633788b57fc2b03f6e3e946ea4f3fa";
		/*
		rc = KogsRPC.kogscreatecontainer("my-dimxy-cont-004", "cont", myplayerid, out txData, out err);
		Debug.Log("KogsRPC.kogscreatecontainer rc=" + rc + " error=" + err);
		if (txData != null)
		{
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr + " signedTx=" + signedTx);
			NSPV.BroadcastTx(signedTx, out txid, out errorStr);
			Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		} 
		*/
		//string mycontainderid = containerids[0];

		//yield return new WaitForSeconds(0);

		// check mempool - not supported
		/*Int64 resultPtr;
		StringBuilder sbErrorStr = new StringBuilder(128);
		rc = NSPV.uplugin_CallRpcMethod("getrawmempool", "", out resultPtr, sbErrorStr);
		Debug.Log("uplugin_CallRpcMethod getrawmempool rc=" + rc);
		if (rc == 0)
		{
			string jresult = KogsRPC.NSPVPtr2String(resultPtr, out errorStr);
			Debug.Log("getrawmempool: " + jresult);
		}*/


		//string mycontainderid = txid; // "e530a04734cec4fa0a457adaf2eda479a947a7da3c905bfd0bc45d56a534ba04"; //txid; // "8ed0bad23b0b924057c61dcc41e25e56411173af78ca4fc4f84c520f4dcb0c69";// txid; //  "f09d8cafbd44a34ce033b8f900159f53d76024939637050793541d2958601153";
		/*
		string token1 = "b3a92e0d75cb2de6b12a490f2eb9aa388ab2a7ec9980210b0548ac0866836485";
		string token2 = "7e2a02ac76d88a4e3a2d849f8f434d0869c84e9edd8d3ebd1089f930ad56f3fb";*/


		//mycontainderid = "03fe1cf4ff521a0f2d10b762242979ee90f22fb3d86beb8766861e38dd85b09e";
		//string[] kogids2 = { kogids[0], kogids[1], kogids[2], kogids[3] };
		//string[] kogids3 = { "05052ecc405cd997b65b63e8e79d9231ad44c66da2d1f1717056b7cdd3ca029e", "cce0426fc47eb55372e82b8db34f2b48085fa85c9a14e93b58a899233c26b19e", "20d3275e13a090a4b6b46e47864037e40a1e0098c475d63e4e787a395e4a959f", "1edc8ae6cb8a79f39d032fd7e92978ceacc6fb36efe93c9b1d2088dea41323a0" };
		// string[] kogids4 = { "573ca1e0d7e44649920140256fbe0fb2f02e05cbe76ca842840e378e006cd8de", "b160395e26ecafadf25092fe67a6720ac2a7478d38bcd376ce556ae940a632f7" };
		//string []kogids5 = { "aee4db40160e12af6713471e16830b62c8e83307e5e931f806d31af369aae70f", "62338bbfa1ad1129422c314bc97d1a1ce2d861d7ea827032197005c68bb61114", "c7c4c78d54a4da5c904ad06fa268f0486328d823b29be74c84cdaa710bb0031c", "ede0c2bdbca47461ea848c60eeca2693964fc44934beb6f3540d97539f7b8e1c" };

		/*
		string[] txDataArr = null;
		rc = KogsRPC.kogsaddkogstocontainer(mycontainderid, kogids2, out txDataArr, out err);
		Debug.Log("KogsRPC.kogsaddkogstocontainer rc=" + rc + " error=" + err);
		if (txDataArr != null)
		{
			for (int i = 0; i < txDataArr.Length; i++)
			{
				NSPV.FinalizeCCTx(txDataArr[i], out signedTx, out errorStr);
				Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
				NSPV.BroadcastTx(signedTx, out txid, out errorStr);
				Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
			}
		}
		*/

		//string[] kogids2 = {"afeaff2624c0943dfd1aece311b52ad5e1f0d65527479c23b06a95c967696eaa", "bca7fd233fe12904b69dccf31de5320c3f4772b294cb16e371385cf65d054ea9" };
		/*
		txDataArr = null;
		rc = KogsRPC.kogsremovekogsfromcontainer(mycontainderid, kogids2, out txDataArr, out err);
		Debug.Log("KogsRPC.kogsremovekogsfromcontainer rc=" + rc + " error=" + err);
		if (txDataArr != null)
		{
			for (int i = 0; i < txDataArr.Length; i++)
			{
				NSPV.FinalizeCCTx(txDataArr[i], out signedTx, out errorStr);
				Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
				NSPV.BroadcastTx(signedTx, out txid, out errorStr);
				Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
			}
		}
		*/

		/*
		string mygameid = "a4143226346bcbe8f782c505b2d493daeed207e7df2917ea81df44fc905de907";
		string mycontainerid2 = "b212fc6569283ba255301e5c6d400127b723697694c27adeaa1a4abc66960103";
		rc = KogsRPC.kogsdepositcontainer(mygameid, mycontainerid2, out txData, out err);
		Debug.Log("KogsRPC.kogsaddkogstocontainer rc=" + rc + " error=" + err);
		*/
		/*if (txData != null)
		{
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			NSPV.BroadcastTx(signedTx, out txid, out errorStr);
			Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);

		}*/


		// StartCoroutine(WaitSecCoroutine());

		/*string[] playerids = { "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204", "ec5ecbe5f7e55e824afcfaf3a5e7b9dfa4fb896c4a31d367ecabd007b694e4d2" }; */
		/*string[] playerids = { "f6889d933dbc06be34c601d97492cc769ec438f2a0a348161e4fc8bea76ac354", "ec5ecbe5f7e55e824afcfaf3a5e7b9dfa4fb896c4a31d367ecabd007b694e4d2" };
		rc = KogsRPC.kogsstartgame("650dd21139e11798fd13869c66e92f6267432983ffb26d905474d09ae029c543", playerids, out txData, out err);
		Debug.Log("KogsRPC.kogsstartgame rc=" + rc + " error=" + err);

		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr);
		Debug.Log("KogsRPC.kogsstartgame txid=" + txid); */

		/*KogsBaseInfo baseInfo;
		rc = KogsRPC.kogsobjectinfo(kogids[0], out baseInfo, out err);
		Debug.Log("KogsRPC.kogsobjectinfo rc=" + rc + " error=" + err + " baseInfo.objectType=" + (baseInfo != null ? baseInfo.objectType : "baseInfo-is-null"));
		if (baseInfo != null)
		{
			//KogsGameStatus gameStatus = (KogsGameStatus)baseInfo;
			KogsMatchObjectInfo matchobj = (KogsMatchObjectInfo)baseInfo;
			//Debug.Log("KogsRPC.kogsobjectinfo baseInfo.objectId=" + baseInfo.objectType + " gameStatus.result=" + gameStatus.result);
			Debug.Log("KogsRPC.kogsobjectinfo baseInfo.objectId=" + baseInfo.objectType + " matchobj.nameId=" + matchobj.nameId);
		}
		else
			Debug.Log("KogsRPC.kogsobjectinfo baseInfo is null");*/

		/*rc = KogsRPC.kogsburntoken("10e4dfef7a81da3654f6c424ffe9d5a394f87650a83fd8eef5aa96746eda03fd", out txData, out err);
		Debug.Log("KogsRPC.kogsburntoken rc=" + rc + " error=" + err);

		rc = KogsRPC.kogsslamdata("bee801d2f5d870a8d3e4ab282e1238560e7b16b078791cd33dc1134f6874e703", "076aa1693ff7539f6e313766e547ddd27820da50fd30c5bb3b25dff330383204", 10, 15, out txData, out err);
		Debug.Log("KogsRPC.kogsslamdata rc=" + rc + " error=" + err);

		rc = KogsRPC.kogsgamelist("10f84ddc4b35287253aa44a7d1edb19d05a75854b3f36e8091972067350571fe", out ids, out err);
		Debug.Log("KogsRPC.kogsgamelist rc=" + rc + " error=" + err + " ids.Length=" + (ids != null ? ids.Length : 0));*/

		/*
		rc = KogsRPC.kogsslamdata("c19674e06e579ad3dbaa6c754cdb07af7811e76501785b2f881b4292df191312", "5253d25bb351cf6a8002865389e3a51cd8b63abe295107aeaca6a9bb3adaae87", 50, 55, out txData, out err);
		Debug.Log("KogsRPC.kogsslamdata rc=" + rc + " error=" + err);

		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr);
		*/

		
		//string crplayerid = "5253d25bb351cf6a8002865389e3a51cd8b63abe295107aeaca6a9bb3adaae87";

		/*rc = KogsRPC.kogscreateplayer("player-test-ad", "advertise", out txData, out err);
		Debug.Log("KogsRPC.kogscreateplayer rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out crplayerid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);

		string[] opts = { KogsRPC.OPT_PLAYFORKEEPS, KogsRPC.OPT_PLAYFORWAGES };
		rc = KogsRPC.kogsadvertiseplayer(crplayerid, opts, out txData, out err);
		Debug.Log("KogsRPC.kogsadvertiseplayer rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		*/
		//KogsAdvertisedPlayer[] adlist;
		//rc = KogsRPC.kogsadvertisedplayerlist(out adlist, out err);
		//Debug.Log("KogsRPC.kogsadvertisedplayerlist rc=" + rc + " error=" + err + " length=" + adlist.Length);
		/*for (int i = 0; i < adlist.Length; i++)
		{
			Debug.Log("adlist[" + i + "]=" + adlist[i].playerid);
			for (int j = 0; j < adlist[i].opts.Length; j++)
				Debug.Log("opts=" + adlist[i].opts[j]);
		}*/

		/*
		rc = KogsRPC.kogsstopadvertiseplayer(crplayerid, out txData, out err);
		Debug.Log("KogsRPC.kogsstopadvertiseplayer rc=" + rc + " error=" + err);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		//NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		//Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		*/

		/*
		NSPV_remoterpccall request json=0000023610595DB0 ({
	    "method":    "kogsaddkogstocontainer",
		"params":    ["dbf532e1ad8ba3da89a58fd76a4f9f0baa5b60a5849d59dd2372b50c7a7683ba", "cb0384bd0631b86ee8cf4e0a2d7af6959ffe4098a409707200c2280e4aa0193d", "35e3dfcd6b48b83e6eef6cd34a9024deea99b8e333fb13f189f26ab1a899e93e", "ea634c6c8bcce2218d8cbc05d1ebef16e1a35c603b64fcd17dc16b37e9787f3f", "1da24d10a9195d2e792dd6166225fc9f2709cba2f9d3a4ad6275f7bf2eb94440"],
		"mypk":    "028e65778cd99898eea7073789359c55e67bdd78643263abf6328888f566d56f19"	})
		*/
		/*		
			string[] txDataArr = null;
			string[] nfts1 = { "2d8e08fe4b5830d97db3ed841c5b57c4a51ce3440b61d0a3c7fbb9d8a0e4c2d4" };// "cb0384bd0631b86ee8cf4e0a2d7af6959ffe4098a409707200c2280e4aa0193d" }; //, "35e3dfcd6b48b83e6eef6cd34a9024deea99b8e333fb13f189f26ab1a899e93e" };//, "ea634c6c8bcce2218d8cbc05d1ebef16e1a35c603b64fcd17dc16b37e9787f3f", "1da24d10a9195d2e792dd6166225fc9f2709cba2f9d3a4ad6275f7bf2eb94440" };
			rc = KogsRPC.kogsaddkogstocontainer(containerids[0], nfts1, out txDataArr, out err);
			Debug.Log("KogsRPC.kogsaddkogstocontainer rc=" + rc + " error=" + err);
			if (txDataArr != null)
			{
				for (int i = 0; i < txDataArr.Length; i++)
				{
					NSPV.FinalizeCCTx(txDataArr[i], out signedTx, out errorStr);
					Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
					//NSPV.BroadcastTx(signedTx, out txid, out errorStr);
					//Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
				}
			}

		}*/
		//NSPV.Finish();
		yield break;
	}

	IEnumerator runDelays()
	{
		Debug.Log("Started runDelays at timestamp : " + Time.time);
		yield return new WaitForSeconds(2);
		Debug.Log("Continue 1 runDelays at timestamp : " + Time.time);
		yield return new WaitForSeconds(2);
		Debug.Log("Continue 2 runDelays at timestamp : " + Time.time);
		yield return new WaitForSeconds(2);
		Debug.Log("Exiting runDelays at timestamp : " + Time.time);
	}

	IEnumerator runFinish()
	{
		Debug.Log("Started runFinish at timestamp : " + Time.time);
		yield break;
	}

	IEnumerator WaitSecCoroutine()
	{
		//Print the time of when the function is first called.
		Debug.Log("Started Coroutine at timestamp : " + Time.time);

		//yield on a new YieldInstruction that waits for 5 seconds.
		yield return new WaitForSeconds(5);

		//After we have waited 5 seconds print the time again.
		Debug.Log("Finished Coroutine at timestamp : " + Time.time);
	}

	bool getPlayer(string wif, string gameid, out string player)
	{
		string errorStr;
		player = null;

		int rc = NSPV.Login(wif, out errorStr);
		Debug.Log("NSPV.Login rc=" + rc + " error=" + errorStr);
		if (rc != 0)
			return false;

		KogsBaseInfo baseInfo;
		rc = KogsRPC.kogsobjectinfo(gameid, out baseInfo, out errorStr);
		if (rc != 0) {
			Debug.Log("can't get game status");
			return false;
		}

		KogsGameStatus gameStat = (KogsGameStatus)baseInfo;

		string []players;

		rc = KogsRPC.kogsplayerlist(true, out players, out errorStr);
		Debug.Log("KogsRPC.kogsplayerlist rc=" + rc + " error=" + errorStr + " players.Length=" + (players==null? 0 : players.Length));
		if (players != null && gameStat.gameinfo.players != null)	{

			for (int i = 0; i < players.Length; i ++)	{
				for (int j = 0; j < gameStat.gameinfo.players.Length; j ++) {
					if (players[i] == gameStat.gameinfo.players[j])	{
						player = players[i];
						return true;
					}
				}
			}
		}
		return false;
	}

	bool createOrGetGameObjects(string wif, out string player, out string container, out string slammer)
	{
		string txData="", errorStr = "", signedTx, txid;
		player = container = slammer = null;

		int rc = NSPV.Login(wif, out errorStr);
		Debug.Log("NSPV.Login rc=" + rc + " error=" + errorStr);
		if (rc != 0)
			return false;

		string []players;
		rc = KogsRPC.kogsplayerlist(true, out players, out errorStr);
		Debug.Log("KogsRPC.kogsplayerlist rc=" + rc + " error=" + errorStr + " players.Length=" + (players==null? 0 : players.Length));
		if (players != null && players.Length > 0)
			player = players[0];
		else 
		{
			rc = KogsRPC.kogscreateplayer("player-test", "test", out txData, out errorStr);
			Debug.Log("KogsRPC.kogscreateplayer rc=" + rc + " error=" + errorStr);
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			rc = NSPV.BroadcastTx(signedTx, out player, out errorStr);
			Debug.Log("NSPV.BroadcastTx rc=" + rc + " errorStr=" + errorStr + " txid=" + player);
			if (rc != 0)  {
				Debug.Log("can't create create player");
				return false;
			}

			string[] opts = { KogsRPC.OPT_PLAYFORKEEPS, KogsRPC.OPT_PLAYFORWAGES };
			rc = KogsRPC.kogsadvertiseplayer(player, opts, out txData, out errorStr);
			Debug.Log("KogsRPC.kogsadvertiseplayer rc=" + rc + " error=" + errorStr);
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
			Debug.Log("NSPV.BroadcastTx rc=" + rc + " errorStr=" + errorStr + " txid=" + txid);
			if (rc != 0)  {
				Debug.Log("can't create advertise player");
				return false;
			}
		}

		string []containers;
		rc = KogsRPC.kogscontainerlist(true, out containers, out errorStr);
		Debug.Log("KogsRPC.kogscontainerlist rc=" + rc + " error=" + errorStr + " containers.Length=" + (containers==null?0:containers.Length));
		if (containers != null && containers.Length > 0)
			container = containers[0];
		else 
		{
			KogsRPC.kogscreatecontainer("cont-test", "test", player, out txData, out errorStr);
			Debug.Log("KogsRPC.kogscreatecontainer rc=" + rc + " error=" + errorStr);
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			rc = NSPV.BroadcastTx(signedTx, out container, out errorStr);
			Debug.Log("NSPV.BroadcastTx rc=" + rc + " errorStr=" + errorStr + " txid=" + container);
			if (rc != 0) {
				Debug.Log("can't create container");
				return false;
			}
		}

		KogsBaseInfo cinfo;
		rc = KogsRPC.kogsobjectinfo(container, out cinfo, out errorStr);
		if (rc != 0)	{
			Debug.Log("KogsRPC.kogsobjectinfo rc=" + rc + " error=" + errorStr);
			return false;
		}
		KogsContainerInfo continfo = (KogsContainerInfo)cinfo;
		Debug.Log("continfo.tokenids.Length=" + continfo.tokenids.Length);

		if (continfo.tokenids.Length < 2)
		{
			int required = 2 - continfo.tokenids.Length;
			string []kogs;
			rc = KogsRPC.kogskoglist(true, out kogs, out errorStr);
			Debug.Log("KogsRPC.kogskoglist rc=" + rc + " error=" + errorStr + " kogs.Length=" + (kogs==null?0:kogs.Length));
			if (kogs.Length < required)	{
				Debug.Log("not enough kogs, required=" + required);
				return false;
			}

			string []kogs2;
			if (required == 2)
				kogs2 = new string[]{ kogs[0], kogs[1] };
			else
				kogs2 = new string[]{ kogs[0] };
			string []txDataArr = null;
			rc = KogsRPC.kogsaddkogstocontainer(container, kogs2, out txDataArr, out errorStr);
			Debug.Log("KogsRPC.kogsaddkogstocontainer rc=" + rc + " error=" + errorStr);
			if (txDataArr != null)
			{
				for (int i = 0; i < txDataArr.Length; i++)
				{
					NSPV.FinalizeCCTx(txDataArr[i], out signedTx, out errorStr);
					Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
					rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
					Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
					if (rc != 0)	{
						Debug.Log("can't add kogs to container");
						return false;
					}
				}
			}
		}
		else if (continfo.tokenids.Length > 2)	
		{
			Debug.Log("too many kogs in container");
			return false;
		}

		string []slammers;
		rc = KogsRPC.kogsslammerlist(true, out slammers, out errorStr);
		Debug.Log("KogsRPC.kogsslammerlist rc=" + rc + " error=" + errorStr + " slammers.Length=" + (slammers==null?0:slammers.Length));
		if (slammers == null || slammers.Length < 1)	{
			Debug.Log("no slammers");
			return false;
		}
		slammer = slammers[0];
		return true;
	}

	bool depositTokens(string wif, string game, string container, string slammer)
	{
		string txData="", errorStr = "", signedTx, txid;


		int rc = NSPV.Login(wif, out errorStr);
		Debug.Log("NSPV.Login rc=" + rc + " error=" + errorStr);
		if (rc != 0)
			return false;


		rc = KogsRPC.kogsdeposittokens(game, container, slammer, out txData, out errorStr);
		Debug.Log("kogsdeposittokens rc=" + rc + " error=" + errorStr);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		if (rc != 0)	{
			return false;
		}
		return true;
	}

	bool doSlam(string wif, string game, string player)
	{
		string txData="", errorStr = "", signedTx, txid;

		int rc = NSPV.Login(wif, out errorStr);
		Debug.Log("NSPV.Login rc=" + rc + " error=" + errorStr);
		if (rc != 0)
			return false;
		

		KogsRPC.kogsslamdata(game, player, 50, 50, out txData, out errorStr);
		Debug.Log("kogsslamdata rc=" + rc + " error=" + errorStr);
		NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
		Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
		rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
		Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
		if (rc != 0)	
			return false;
		
		return true;
	}

	IEnumerator runTestGame()
	{
		int rc;
		//string sChainName = "DIMXY14"; //  "DIMXY11"; // "RFOXLIKE";

		// gui devs' wifs:
		//string wif1 = "Utgyem1EBZ42eEiuSF3cJT9m4VhjN27Z7vXWC9zRzMXhLa6ZLKBF";  //p1  028e65778cd99898eea7073789359c55e67bdd78643263abf6328888f566d56f19
		//string wif2 = "Uu64bT9NDRTZQDSBxfrKbtRcYvv7qYk2RotQzsJfntJsMENuKrja";  //p2  02e1bb3f95f46fd89a93c8fe39c6e287c8beef659b7277791345b1b1aaa68a19b3

		string wif1 = "UpUhjzv1x6gQoiRL6GkM4Yb44uYPjxshqigVdNSaUqpwDkoqFsGm";
   		// "address": "RTbiYv9u1mrp7TmJspxduJCe3oarCqv9K4",
   		// "pubkey": "025f97b6c42409e8e69eb2fdab281219aafe15169deec801ee621c63cc1ba0bb8c",
		string wif2 = "UvchGG2gYsTgsKA4vCAp4UNHAn6gLgUrRKEbcrjqzAFhbu8fqzUD";
   		// "address": "RLNXzPsfWkRvNbUcargXJchRsWXGSg7U4L",
   		// "pubkey": "025fa5b41da1e4cb9b9af345dddd2a4c35feb5030580e1fa40faaf387957b36f41",

		string txData = "";
		string signedTx;
		string errorStr;
		string txid = "";
		string gameconfigid = "4917408cd21451e51547b52988ab7f6e90742be69b94fa85d78132d5d3a3b69c";


		// already done:
		// rc = NSPV.Init(sChainName, out errorStr);
		// Debug.Log("NSPV.Init rc=" + rc + " error=" + errorStr);

		string player1, container1, slammer1; 
		string player2, container2, slammer2; 
		string gameid = null;
		//string gameid = "fe7f3e4f3938ea48b11876b33c0803a364fc1e4f543c9403978d1ef9be165acb";

		// create or get player and container and get slammer
		if (createOrGetGameObjects(wif1, out player1, out container1, out slammer1) == false && gameid == null) {
			Debug.Log("cant get game objects for wif1");
			yield break;
		}
		if (createOrGetGameObjects(wif2, out player2, out container2, out slammer2) == false && gameid == null) {
			Debug.Log("cant get game objects for wif2");
			yield break;
		} 


		if (!String.IsNullOrEmpty(gameid))	{
			// get players from existing game
			if (getPlayer(wif1, gameid, out player1) == false) {
				Debug.Log("cant get player for wif1");
				yield break;
			}
			if (getPlayer(wif2, gameid, out player2) == false) {
				Debug.Log("cant get player for wif2");
				yield break;
			}
		}
		Debug.Log("found players=" + player1 + " " + player2);

		
		string []playerids = { player1, player2 };

		rc = NSPV.Login(wif1, out errorStr);
		Debug.Log("NSPV.Login for wif1 rc=" + rc + " error=" + errorStr);
		if (rc != 0)
			yield break;

		if (String.IsNullOrEmpty(gameid)) 
		{
			rc = KogsRPC.kogsstartgame(gameconfigid, playerids, out txData, out errorStr);
			Debug.Log("kogsstartgame rc=" + rc + " error=" + errorStr);
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			rc = NSPV.BroadcastTx(signedTx, out gameid, out errorStr);
			Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " gameid=" + gameid);
			if (rc != 0)	{
				Debug.Log("can't start a game");
					yield break;
			}
		}

		yield return new WaitForSeconds(5);

		if (!String.IsNullOrEmpty(container1) && !String.IsNullOrEmpty(slammer1))	{ // if slammer is empty let's consider it is already deposited
			if (depositTokens(wif1, gameid, container1, slammer1) == false) {
				Debug.Log("can't deposit tokens for wif1");
				yield break;
			}
		}

		if (!String.IsNullOrEmpty(container2) && !String.IsNullOrEmpty(slammer2))	{
			if (depositTokens(wif2, gameid, container2, slammer2) == false) {
				Debug.Log("can't deposit tokens for wif2");
				yield break;
			}
		}

		int count = 0;
		rc = NSPV.Login(wif1, out errorStr);
		Debug.Log("NSPV.Login for wif1 rc=" + rc + " error=" + errorStr);
		if (rc != 0)
			yield break;
		while(true) 
		{
			Debug.Log("waiting for depositing nfts to game...");
			KogsBaseInfo baseInfo;
			rc = KogsRPC.kogsobjectinfo(gameid, out baseInfo, out errorStr);
			if (rc != 0) {
				Debug.Log("can't get game status");
				yield break;
			}
			KogsGameStatus gameStat = (KogsGameStatus)baseInfo;
			if (!String.IsNullOrEmpty(gameStat.gameinfo.NextPlayerId) && gameStat.gameinfo.NextPlayerId != "none")	{
				Debug.Log("next turn baton exists");
				break;
			}

			yield return new WaitForSeconds(5);

			rc = KogsRPC.kogscreatefirstbaton(gameid, out txData, out errorStr);
			Debug.Log("kogscreatefirstbaton rc=" + rc + " error=" + errorStr);
			NSPV.FinalizeCCTx(txData, out signedTx, out errorStr);
			Debug.Log("NSPV.FinalizeCCTx errorStr=" + errorStr);
			rc = NSPV.BroadcastTx(signedTx, out txid, out errorStr);
			Debug.Log("NSPV.BroadcastTx errorStr=" + errorStr + " txid=" + txid);
			if (rc == 0)
				break;
			if (rc != 0 && ++count == 10)	{
				Debug.Log("can't create first baton (maybe nfts not deposited)");
				yield break;
			}
		}

		string expectedNext = null;
		count = 0;
		while(true) 
		{
			yield return new WaitForSeconds(5);

			KogsBaseInfo baseInfo;
			rc = KogsRPC.kogsobjectinfo(gameid, out baseInfo, out errorStr);
			if (rc != 0) {
				Debug.Log("can't get game status");
				yield break;
			}

			KogsGameStatus gameStat = (KogsGameStatus)baseInfo;

			if (gameStat.gameinfo.finished)	{
				Debug.Log("game ended, winner=" + gameStat.gameinfo.WinnerId);
				break;
			}

			if ((String.IsNullOrEmpty(gameStat.gameinfo.NextPlayerId) || gameStat.gameinfo.NextPlayerId == "none")  // first baton is not created
				||
				!String.IsNullOrEmpty(expectedNext) && expectedNext != gameStat.gameinfo.NextPlayerId)  // next baton is not created
			{	
				if (++count == 10)	{
					Debug.Log("could not get next baton");
					break;
				}

				if (String.IsNullOrEmpty(expectedNext))
					Debug.Log("waiting for first baton");
				else
					Debug.Log("waiting for next player=" + expectedNext);
				continue;
			}

			bool slamok;
			if (gameStat.gameinfo.NextPlayerId == player1) {
				slamok = doSlam(wif1, gameid, player1);
			} 
			else {
				slamok = doSlam(wif2, gameid, player2);
			}
			if (slamok == false)	{
				Debug.Log("can't do slam");
				break;
			}
			expectedNext = (gameStat.gameinfo.NextPlayerId == player1 ? player2 : player1); // set expected next player
			count = 0;
		}
		Debug.Log("exiting test");

	}
}
