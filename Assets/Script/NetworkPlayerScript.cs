using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PhotonView))]
[RequireComponent (typeof(PlayerAnimScript))] //Animationの設定を行うスクリプト

public class NetworkPlayerScript : Photon.MonoBehaviour {
    private Vector3 correctPlayerPos = Vector3.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;

    private BattleEnemyScript battleEnemyScript;
    private BattleGameStartScript battleGameStartScript;
    public int actionNumber = 0;
    //変速設定系
    public float[] bombSpeedPetern = new float[10];
    public float[] moveFloorSpeedPetern = new float[10];

    //設定値を反映したか
    private bool isSettingUpdate = false;

    //ゲーム終了フラグ(フラグをtrueにした人が負け)
    public bool isGameEnd = false;

    void Start() {
        battleEnemyScript = this.gameObject.GetComponent <BattleEnemyScript>();

        GameObject gameStartObj = GameObject.Find("GameStartObj");
        battleGameStartScript =  gameStartObj.GetComponent<BattleGameStartScript>();
    }

    void Update () {
        //自分のキャラクター以外の時はLerpを使って滑らかに位置と角度を変更
        if (!photonView.isMine) {
            //transform.position = Vector3.Lerp (transform.position, this.correctPlayerPos, Time.deltaTime * 5);
            //transform.rotation = Quaternion.Lerp (transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
        }
    }

    void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        //データを送る
        if (stream.isWriting) {
            //現在地と角度を送信
            stream.SendNext (transform.position);
            stream.SendNext (transform.rotation);

            //現在のアニメーションの番号と再生速度を送信
            PlayerAnimScript AnimeScript = GetComponent<PlayerAnimScript> ();
            stream.SendNext (AnimeScript.AnimeNoNow);
            stream.SendNext (AnimeScript.AnimeSpeedNow);

            //アクション番号を送信
            stream.SendNext (actionNumber);
            actionNumber = 0;

            //設定値を送信
            stream.SendNext(bombSpeedPetern);
            stream.SendNext(moveFloorSpeedPetern);

            //ゲーム終了フラグ
            stream.SendNext(isGameEnd);

        //データを受け取る
        } else {
            //現在地と角度を受信
            this.correctPlayerPos = (Vector3)stream.ReceiveNext ();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext ();
            //現在のアニメーションの番号と再生速度を受信
            PlayerAnimScript AnimeScript = GetComponent<PlayerAnimScript> ();
            AnimeScript.AnimeNoNow = (int)stream.ReceiveNext ();
            AnimeScript.AnimeSpeedNow = (float)stream.ReceiveNext ();

            //アクション番号を受信
            int actNum = (int) stream.ReceiveNext();
            if (actNum > 0) {
                battleEnemyScript.enemyAction(actNum);
                actionNumber = 0;
            }

            //設定値を受信
            float[] bombSetting = (float[]) stream.ReceiveNext();
            float[] moveFloorSetting = (float[]) stream.ReceiveNext();
            if (bombSetting[0] != null && bombSetting[0] > 0 && !isSettingUpdate) {
                battleGameStartScript.bombSpeedPetern = bombSetting;
                battleGameStartScript.moveFloorSpeedPetern = moveFloorSetting;
                isSettingUpdate = true;
            }

            //ゲーム終了フラグを受信
            bool gameEndFlg = (bool) stream.ReceiveNext();
            Debug.Log("GAME END FLG:" + gameEndFlg);
            if (gameEndFlg) {
                Debug.Log("WIN");
                battleGameStartScript.win();
            }
        }
    }

    public void updateActionNumber(int num) {
        actionNumber = num;
    }

    public void updateSettings(float[] bomb, float[] moveFloor) {
        Debug.Log("キテルカ");
        Debug.Log(bomb);
        Debug.Log(moveFloor);
        bombSpeedPetern = bomb;
        moveFloorSpeedPetern = moveFloor;
    }

    //ゲーム切断
    public void gameEnd() {
        PhotonNetwork.Disconnect();
    }
}


