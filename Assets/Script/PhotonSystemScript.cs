﻿using UnityEngine;
using System.Collections;
 
public class PhotonSystemScript : Photon.MonoBehaviour {

	public GameObject battleGameStartObject;
	public GameObject prefab;

	//GameStartObjectのScript
	private BattleGameStartScript gameStartScript;
 
	/// マスターサーバーのロビーに入るに呼び出されます。
	void OnJoinedLobby() {
		Debug.Log("ロビーに入室");
		//ランダムにルームへ参加
		PhotonNetwork.JoinRandomRoom();
	}
	 
	/// 部屋に入るとき呼ばれます。
	/// これは参加する際だけでなく作成する際も含みます。
	void OnJoinedRoom() {
		Debug.Log("部屋に入室");
		GameObject chara = PhotonNetwork.Instantiate("Character", new Vector3(-1.29f, 11.56f, -1f), new Quaternion(0, 180f, 0f, 0f), 0) as GameObject;
		gameStartScript.charaSetting(chara);
	}
	 
	/// JoinRandom()の入室が失敗した場合に後に呼び出されます。
	void OnPhotonRandomJoinFailed() {
		Debug.Log("部屋入室失敗");
		//名前のないルームを作成
		PhotonNetwork.CreateRoom(null);
	}
	 
	void Awake() {
		gameStartScript = battleGameStartObject.GetComponent<BattleGameStartScript>();

		//マスターサーバーへ接続
		Debug.Log("マスターサーバーへ接続します");
		PhotonNetwork.ConnectUsingSettings("v0.1");
	}

}