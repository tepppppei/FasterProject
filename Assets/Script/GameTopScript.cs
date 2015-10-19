using UnityEngine;
using System.Collections;
using MiniJSON;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameTopScript : MonoBehaviour {

    // Use this for initialization
    private int stageEasy = 20;
    private int stageNormal = 20;
    private int stageHard = 20;

    private DataTable easyDataTable;
    private DataTable normalDataTable;
    private DataTable hardDataTable;

    private string currentDifficulty = "easy";

    void Start () {
        databaseInit();
    }

    // Update is called once per frame
    void Update () {

    }

    //トップボタン一覧
    public GameObject[] topButtonList;

    public GameObject stageSelectPanel;
    public GameObject stageBase;
    public GameObject scrollViewContent;

    public void sceneStageSelect() {
        //ステージ選択パネルを表示する
        StartCoroutine(showStageSelect());

        string stg = "easy";
        currentDifficulty = stg;
        StartCoroutine(showStageList(stg));
    }

    //ステージ選択画面表示用
    IEnumerator showStageSelect() {
        //スタートボタン系を削除
        foreach (GameObject n in topButtonList) {
            Destroy(n);
        }

        iTween.MoveTo(stageSelectPanel, iTween.Hash(
                    "position", new Vector3(0, 0, 0),
                    "time", 0.2f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(1.0f);
    }

    public void stageChange(string difficulty) {
        currentDifficulty = difficulty;
        StartCoroutine(showStageList(difficulty));
    }

    public void stageSelect(int stageNumber) {
        PlayerPrefs.SetString ("difficulty", currentDifficulty);
        PlayerPrefs.SetInt ("stage_number", stageNumber);
        Debug.Log("難易度：" + currentDifficulty);
        Debug.Log("ステージ番号：" + stageNumber);

        Application.LoadLevel("RaceScene");
    }

    //ステージ一覧表示
    private GameObject temporaryObject;
    IEnumerator showStageList(string difficulty) {
        //ステージ一覧をリセット（削除）
        foreach (Transform n in scrollViewContent.transform) {
            Destroy(n.gameObject);
        }

        yield return new WaitForSeconds(0.4f);

        int loop = 0;
        int currentStage = 0;
        DataTable currentDataTable = new DataTable();
        if (difficulty == "easy") {
            loop = stageEasy;
            currentStage = easyDataTable.Rows.Count;
            currentDataTable = easyDataTable;
        } else if (difficulty == "normal") {
            loop = stageNormal;
            currentStage = normalDataTable.Rows.Count;
            currentDataTable = normalDataTable;
        } else if (difficulty == "hard") {
            loop = stageHard;
            currentStage = hardDataTable.Rows.Count;
            currentDataTable = hardDataTable;
        }

        //星のスプライトを読み込んでおく
        Sprite star0 = Resources.Load <Sprite> ("Prefab/Stage/" + difficulty + "/selectbutton1_0");
        Sprite star1 = Resources.Load <Sprite> ("Prefab/Stage/" + difficulty + "/selectbutton1_1");
        Sprite star2 = Resources.Load <Sprite> ("Prefab/Stage/" + difficulty + "/selectbutton1_2");
        Sprite star3 = Resources.Load <Sprite> ("Prefab/Stage/" + difficulty + "/selectbutton1_3");
        Sprite starLock = Resources.Load <Sprite> ("Prefab/Stage/" + difficulty + "/selectbutton1_4");

        //未クリア＆クリア済みステージのみ表示
        for(int i = 0; i < (currentStage+1); i++) {
            temporaryObject = GameObject.Instantiate(stageBase) as GameObject;
            temporaryObject.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
            //Canvasの子要素として登録する 
            temporaryObject.transform.SetParent (scrollViewContent.transform, false);

            int starCount = 0;
            //星の数を設定
            Sprite currentStarSprite = star0;
            if (currentStage > 0 && currentDataTable.Rows[i] != null) {
                starCount = (int) currentDataTable.Rows[i]["star"];
                if (starCount == 1) {
                    currentStarSprite = star1;
                } else if (starCount == 2) {
                    currentStarSprite = star2;
                } else if (starCount == 3) {
                    currentStarSprite = star3;
                }
            }
            //ボタンの画像を変更
            temporaryObject.GetComponent<Image>().sprite = currentStarSprite;

            //テキストを変更
            GameObject childObject = temporaryObject.transform.FindChild("Text").gameObject;
            if (childObject != null) {
                childObject.GetComponent<Text>().text = (i+1).ToString();
            }

            iTween.ValueTo(gameObject, iTween.Hash(
                        "from", 0,
                        "to", 1.0f,
                        "time", 0.05f,
                        "onupdate", "fadeInObject"  // 毎フレーム SetAlpha() を呼びます。
                        ));

            yield return new WaitForSeconds(0.15f);
        }

        //最後のステージじゃなければ、ロックステージを追加
        if (currentStage != loop) {
            temporaryObject = GameObject.Instantiate(stageBase) as GameObject;
            temporaryObject.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
            //Canvasの子要素として登録する 
            temporaryObject.transform.SetParent (scrollViewContent.transform, false);

            //ボタンの画像を変更
            temporaryObject.GetComponent<Image>().sprite = starLock;

            //テキストを変更
            GameObject childObject = temporaryObject.transform.FindChild("Text").gameObject;
            if (childObject != null) {
                childObject.GetComponent<Text>().text = "";
            }

            iTween.ValueTo(gameObject, iTween.Hash(
                        "from", 0,
                        "to", 1.0f,
                        "time", 0.1f,
                        "onupdate", "fadeInObject"  // 毎フレーム SetAlpha() を呼びます。
                        ));
        }

        yield return new WaitForSeconds(0.2f);
    }

    //ステージボタンを大きくする
    private void fadeInObject(float value) {
        temporaryObject.GetComponent<RectTransform>().localScale = new Vector3 (value, value, value);
    }

    private void CompleteHandler() {
    }

    //データベース系
    private void databaseInit() {
        //本来はマスターデータ取得
        string jsonString = "{\"easy\": 10, \"normal\": 11, \"hard\": 12}";
        var json = Json.Deserialize(jsonString) as Dictionary<string, object>;

        stageEasy = int.Parse(json["easy"].ToString());
        stageNormal = int.Parse(json["normal"].ToString());
        stageHard = int.Parse(json["hard"].ToString());

        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");

        // Select
        string selectQuery = "select * from Stage where difficulty = 1";
        easyDataTable = sqlDB.ExecuteQuery(selectQuery);

        selectQuery = "select * from Stage where difficulty = 2";
        normalDataTable = sqlDB.ExecuteQuery(selectQuery);

        selectQuery = "select * from Stage where difficulty = 3";
        hardDataTable = sqlDB.ExecuteQuery(selectQuery);

        /*
           foreach(DataRow dr in dataTable.Rows){
        //id = (string)dr["name"];
        Debug.Log (dr["id"]);
        }
        */


        /*

           string query = "insert into Stage values";
           int loopCount = 1;

           int easyNum = int.Parse(json["easy"].ToString());
           easyNum = 1;
           if (easyNum > 0) {
           for (int i = 0; i < easyNum; i++) {
           query += "(" + loopCount.ToString() + "," + (i+1) + ",1,0,0,0,datetime(),datetime())";
           if (i != (easyNum-1)) {
           query += ",";
           }
           loopCount++;
           }
           }

           Debug.Log(query);
           sqlDB.ExecuteNonQuery(query);
           */

    }
}
