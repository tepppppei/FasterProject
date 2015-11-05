using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PhotonSystemScript : Photon.MonoBehaviour {

    public GameObject battleGameStartObject;
    public GameObject prefab;

    //GameStartObjectのScript
    private BattleGameStartScript gameStartScript;

    //メッセージ用
    private Text messageObject;

    /// マスターサーバーのロビーに入るに呼び出されます。
    void OnJoinedLobby() {
        sendMessage("CONNECT LOBBY");
        Debug.Log("ロビーに入室");
        //ランダムにルームへ参加
        PhotonNetwork.JoinRandomRoom();
    }

    /// 部屋に入るとき呼ばれます。
    /// これは参加する際だけでなく作成する際も含みます。
    void OnJoinedRoom() {
        Debug.Log("部屋に入室");
        sendMessage("JOINED ROOM");
        //GameObject chara = PhotonNetwork.Instantiate("Character", new Vector3(-1.29f, 11.56f, -1f), new Quaternion(0, 180f, 0f, 0f), 0) as GameObject;

        int charaNumber = gameStartScript.charaNumber;
        GameObject chara;
        if (charaNumber == 3) {
            chara = PhotonNetwork.Instantiate("Character"+charaNumber.ToString(), new Vector3(-1.29f, -8.05f, -1f), new Quaternion(0, 0, 0, 0), 0) as GameObject;
        } else if (charaNumber == 4) {
            chara = PhotonNetwork.Instantiate("Character"+charaNumber.ToString(), new Vector3(-1.29f, -8.05f, -1f), new Quaternion(0, 0, 0, 0), 0) as GameObject;
        } else {
            chara = PhotonNetwork.Instantiate("Character"+charaNumber.ToString(), new Vector3(-1.29f, -8.05f, -1f), new Quaternion(0, 180f, 0, 0), 0) as GameObject;
        }

        gameStartScript.charaSetting(chara);
    }

    /// JoinRandom()の入室が失敗した場合に後に呼び出されます。
    void OnPhotonRandomJoinFailed() {
        Debug.Log("部屋入室失敗");
        sendMessage("CREATE ROOM");
        //名前のないルームを作成
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.isVisible = true;
        roomOptions.isOpen = true;
        roomOptions.maxPlayers = 2;
        roomOptions.customRoomPropertiesForLobby = new string[] {"CustomProperties"};

        PhotonNetwork.CreateRoom(null, roomOptions, null);

        //マスターフラグをtrueに
        gameStartScript.isMaster = true;
    }

    void Awake() {
        gameStartScript = battleGameStartObject.GetComponent<BattleGameStartScript>();
        messageObject = GameObject.Find("Message").GetComponent<Text>();

        //マスターサーバーへ接続
        Debug.Log("マスターサーバーへ接続します");
        sendMessage("CONNECT SERVER");
        PhotonNetwork.ConnectUsingSettings("v0.1");
    }

    private void sendMessage(string mes) {
        if (messageObject != null) {
            messageObject.text = mes;
        }
    }

    //エラー系
    void OnLeftRoom(){
        gameStartScript.connectError();
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player){
        gameStartScript.connectError();
    }
}
