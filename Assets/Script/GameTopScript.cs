using UnityEngine;
using System.Collections;
using MiniJSON;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class GameTopScript : MonoBehaviour {

    // Use this for initialization
    private int stageEasy = 20;
    private int stageNormal = 20;
    private int stageHard = 20;

    private DataTable easyDataTable;
    private DataTable normalDataTable;
    private DataTable hardDataTable;

    //キャラテーブル
    private DataTable characterTable;

    private string currentDifficulty = "easy";

    void Start () {
        StartCoroutine(characterInit());
        StartCoroutine(databaseInit());
    }

    // Update is called once per frame
    void Update () {

    }

    //トップボタン一覧
    public GameObject[] topButtonList;

    public GameObject[] difficultyButtonList;

    public GameObject stageSelectPanel;
    public GameObject stageBase;
    public GameObject scrollViewContent;
    public GameObject charaSelectPanel;
    public GameObject charaHeadImage;
    public Text charaNameText;

    //シーン変更時のオブジェクト
    public GameObject[] sceneChangeObject;

    //初期位置
    private Vector3 stageSelectPanelDefaultPosition; 
    private Vector3 charaSelectPanelDefaultPosition;

    //選択中のキャラ番号
    private int selectedCharaNumber;
    private int selectedCharaTableNumber;
    private GameObject selectCharaObject;

    //キャラ選択パネル用オブジェクト
    public Text levelText;
    public Text growthText;
    public Text skillNameText;
    public Text skillDescriptionText;
    public GameObject growthProgressObject;

    //ステージ選択表示用
    public void sceneStageSelect() {
        //ステージ選択パネルを表示する
        StartCoroutine(showStageSelect());

        string stg = "easy";
        currentDifficulty = stg;
        stageChange(stg);
    }

    //キャラ選択表示用
    public void sceneCharaSelect() {
        //ステージ選択パネルを表示する
        StartCoroutine(showCharaSelect());
    }

    //ステージ選択クローズ用
    public void closeStage() {
        StartCoroutine(closeStageSelect());
    }

    //キャラ選択クローズ用
    public void closeChara() {
        StartCoroutine(closeCharaSelect());
    }

    //キャラ選択画面表示用
    IEnumerator showCharaSelect() {
        //スタートボタン系を削除
        foreach (GameObject n in topButtonList) {
            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(n, iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.05f));
            yield return new WaitForSeconds(0.1f);
        }

        charaSelectPanelDefaultPosition = charaSelectPanel.transform.localPosition;
        iTween.MoveTo(charaSelectPanel, iTween.Hash(
                    "position", new Vector3(0, 0, 0),
                    "time", 0.2f,
                    "islocal", true,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(1.0f);
    }

    //ステージ選択画面表示用
    IEnumerator showStageSelect() {
        //スタートボタン系を削除
        foreach (GameObject n in topButtonList) {
            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(n, iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.05f));
            yield return new WaitForSeconds(0.1f);
        }

        stageSelectPanelDefaultPosition = stageSelectPanel.transform.localPosition;
        iTween.MoveTo(stageSelectPanel, iTween.Hash(
                    "position", new Vector3(0, 0, 0),
                    "time", 0.2f,
                    "islocal", true,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(1.0f);
    }

    //ステージ選択画面を閉じる
    IEnumerator closeStageSelect() {
        iTween.MoveTo(stageSelectPanel, iTween.Hash(
                    "position", stageSelectPanelDefaultPosition,
                    "time", 0.2f,
                    "islocal", true,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(0.1f);

        foreach (GameObject n in topButtonList) {
            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(n, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.05f));
            yield return new WaitForSeconds(0.1f);
        }
    }

    //キャラ選択画面を閉じる
    IEnumerator closeCharaSelect() {
        iTween.MoveTo(charaSelectPanel, iTween.Hash(
                    "position", charaSelectPanelDefaultPosition,
                    "time", 0.2f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(0.1f);

        foreach (GameObject n in topButtonList) {
            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(n, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.05f));
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void stageChange(string difficulty) {
        Sprite activeButton = Resources.Load <Sprite> ("Prefab/Stage/Button/button_theme1_rectangle");
        Sprite visibleButton = Resources.Load <Sprite> ("Prefab/Stage/Button/button_theme5_rectangle");

        if (difficulty == "easy") {
            for (int i = 0; i < difficultyButtonList.Length; i++) {
                if (i == 0) {
                    difficultyButtonList[i].GetComponent<Image>().sprite = activeButton;
                } else {
                    difficultyButtonList[i].GetComponent<Image>().sprite = visibleButton;
                }
            }
        } else if (difficulty == "normal") {
            for (int i = 0; i < difficultyButtonList.Length; i++) {
                if (i == 1) {
                    difficultyButtonList[i].GetComponent<Image>().sprite = activeButton;
                } else {
                    difficultyButtonList[i].GetComponent<Image>().sprite = visibleButton;
                }
            }
        } else if (difficulty == "hard") {
            for (int i = 0; i < difficultyButtonList.Length; i++) {
                if (i == 2) {
                    difficultyButtonList[i].GetComponent<Image>().sprite = activeButton;
                } else {
                    difficultyButtonList[i].GetComponent<Image>().sprite = visibleButton;
                }
            }
        }

        currentDifficulty = difficulty;
        StartCoroutine(showStageList(difficulty));
    }

    public void stageSelect(int stageNumber) {
        PlayerPrefs.SetString ("difficulty", currentDifficulty);
        PlayerPrefs.SetInt ("stage_number", stageNumber);
        Debug.Log("難易度：" + currentDifficulty);
        Debug.Log("ステージ番号：" + stageNumber);

        StartCoroutine(viewChange("RaceScene"));
    }

    public void gameBattle() {
        StartCoroutine(viewChange("RaceBattleScene"));
    }

    IEnumerator viewStart() {
        yield return new WaitForSeconds(1.1f);

        sceneChangeObject[0].transform.SetAsLastSibling();
        sceneChangeObject[1].transform.SetAsLastSibling();

        iTween.MoveTo(sceneChangeObject[0], iTween.Hash(
                    "position", new Vector3(3, 479, -500),
                    "time", 2.0f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));
        iTween.MoveTo(sceneChangeObject[1], iTween.Hash(
                    "position", new Vector3(3, -487, -500),
                    "time", 2.0f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(2.1f);
    }

    IEnumerator viewChange(string sceneName) {
        Destroy(charaHeadImage);

        sceneChangeObject[0].transform.SetAsLastSibling();
        sceneChangeObject[1].transform.SetAsLastSibling();

        iTween.MoveTo(sceneChangeObject[0], iTween.Hash(
                    "position", new Vector3(3, 159, -500),
                    "time", 1.0f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));
        iTween.MoveTo(sceneChangeObject[1], iTween.Hash(
                    "position", new Vector3(3, -159, -500),
                    "time", 1.0f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(1.1f);

        Application.LoadLevel(sceneName);
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
            if (currentStage > 0 && i != currentDataTable.Rows.Count && currentDataTable.Rows[i] != null) {
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

            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(temporaryObject, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.05f));

/*
            iTween.ValueTo(gameObject, iTween.Hash(
                        "from", 0,
                        "to", 1.0f,
                        "time", 0.05f,
                        "onupdate", "fadeInObject"  // 毎フレーム SetAlpha() を呼びます。
                        ));
                        */

            yield return new WaitForSeconds(0.1f);

            if ((i+1) >= loop) {
                break;
            }
        }

        //最後のステージじゃなければ、ロックステージを追加
        Debug.Log("CURRENT STAGE:" + currentStage);
        if ((currentStage+1) < loop) {
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

    //キャラクター更新系
    IEnumerator characterInit() {
        string url = "http://pe-yan.top/faster/characters/master";
        // 送信開始
        WWW www = new WWW (url);
        yield return www;

        // 成功
        if (www.error == null) {
            Debug.Log("Get Success");
            string response = www.text;

            var jsonFullData = (IDictionary) MiniJSON.Json.Deserialize (response);
            var jsonCharaData = (IList) jsonFullData["character_list"];

            //キャラクターデータを取得
            SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
            //Statusが無ければインサート
            string selectQuery = "select * from Character";
            DataTable characterTable = sqlDB.ExecuteQuery(selectQuery);

            Debug.Log(response);

            for (int i = 0; i < jsonCharaData.Count; i++) {
                var json = (IDictionary) jsonCharaData[i];
                string cID = (string) json["id"];
                string cName = (string) json["name"];
                string cSkillType = (string) json["skill_type"];
                string cSkillNumber = (string) json["skill_number"];
                string cSkillName = (string) json["skill_name"];
                string cSkillDescription = (string) json["skill_description"];
                string cSkillPlusDescription = (string) json["skill_plus_description"];
                int cShowFlg = 0;
                if ((bool) json["show_flg"]) {
                    cShowFlg = 1;
                }

                //存在チェック
                bool isUpdate = false;
                foreach (DataRow row in characterTable.Rows) {
                    if (row["id"].ToString() == cID) {
                        isUpdate = true;
                        break;
                    }
                }

                //アップデート
                if (isUpdate) {
                    string query = "update Character set name='"+cName+"', skill_number="+cSkillNumber+", skill_name='"+cSkillName+"', skill_type="+cSkillType+", skill_description='"+cSkillDescription+"', skill_plus_description='"+cSkillPlusDescription+"', show_flg="+cShowFlg+" where id="+cID;
                    Debug.Log(query);
                    sqlDB.ExecuteNonQuery(query);
                //インサート
                } else {
                    string query = "insert into Character(id, name, skill_number, skill_name, skill_description, skill_plus_description, show_flg) values("+cID+",'"+cName+"',"+cSkillNumber+",'"+cSkillName+"','"+cSkillDescription+"','"+cSkillPlusDescription+"',"+cShowFlg+")";
                    Debug.Log(query);
                    sqlDB.ExecuteNonQuery(query);
                }
            }

        }
        // 失敗
        else{
            Debug.Log("Get Failure");
        }
    }

    //データベース系
    IEnumerator databaseInit() {
        string url = "http://pe-yan.top/faster/stages/master";
        // HEADERはHashtableで記述
        // 送信開始
        WWW www = new WWW (url);
        yield return www;

        // 成功
        if (www.error == null) {
            Debug.Log("Get Success");

            // 本来はサーバからのレスポンスとしてjsonを戻し、www.textを使用するが
            // 今回は便宜上、下記のjsonを使用する
            //string txt = "{\"name\": \"okude\", \"level\": 99, \"friend_names\": [\"ichiro\", \"jiro\", \"saburo\"]}";

            string response = www.text;

            Debug.Log(response);
            var json = Json.Deserialize(response) as Dictionary<string, object>;

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

            //Statusが無ければインサート
            selectQuery = "select * from UserStatus";
            DataTable statusTable = sqlDB.ExecuteQuery(selectQuery);
            if (statusTable.Rows.Count > 0) {
                int baseMoney = (int) statusTable.Rows[0]["money"];
                string money = baseMoney.ToString();

                Debug.Log("MONEY:" + money);

                GameObject money1 = GameObject.Find("number1").gameObject;
                GameObject money2 = GameObject.Find("number2").gameObject;
                GameObject money3 = GameObject.Find("number3").gameObject;
                GameObject money4 = GameObject.Find("number4").gameObject;

                String m1 = "0";
                String m2 = "0";
                String m3 = "0";
                String m4 = "0";

                if (money.Length >= 4) {
                    m4 = money.Substring(0, 1);
                    m3 = money.Substring(1, 1);
                    m2 = money.Substring(2, 1);
                    m1 = money.Substring(3, 1);
                } else if (money.Length >= 3) {
                    m3 = money.Substring(0, 1);
                    m2 = money.Substring(1, 1);
                    m1 = money.Substring(2, 1);
                } else if (money.Length >= 2) {
                    m2 = money.Substring(0, 1);
                    m1 = money.Substring(1, 1);
                } else {
                    m1 = money.Substring(0, 1);
                }

                money1.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m1);
                money2.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m2);
                money3.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m3);
                money4.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m4);
            } else {
                string query = "insert into UserStatus(user_name, money, created, updated) values('NoName', 0, datetime(), datetime())";
                sqlDB.ExecuteNonQuery(query);
            }

            //キャラクター系設定
            //selectQuery = "select * from Character where get_flg = 1";
            selectQuery = "select * from Character";
            characterTable = sqlDB.ExecuteQuery(selectQuery);
            string charaName = "";
            string skName = "";
            string charaLevel = "";
            string growth = "";
            string skDescription = "";

            //現在選択中のキャラ
            for (int i = 0; i < characterTable.Rows.Count; i++) {
                    Debug.Log(characterTable.Rows[i]["name"]);
                if ((int)characterTable.Rows[i]["select_flg"] == 1) {
                    selectedCharaNumber = (int) characterTable.Rows[i]["id"];
                    charaName = (string) characterTable.Rows[i]["name"];
                    skName = (string) characterTable.Rows[i]["skill_name"];
                    charaLevel = (string) characterTable.Rows[i]["get_count"].ToString();
                    growth = (string) characterTable.Rows[i]["growth"];
                    growth = "35";
                    skDescription = (string) characterTable.Rows[i]["skill_description"];

                    selectedCharaTableNumber = i;
                    break;
                }
            }

            //キャラ画像設定
            charaHeadImage.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + selectedCharaNumber + "/head");
            GameObject selectCharaPrefab = Resources.Load <GameObject> ("Prefab/Chara/Character" + selectedCharaNumber);
            selectCharaObject = GameObject.Instantiate(selectCharaPrefab) as GameObject;
            //Canvasの子要素として登録する
            selectCharaObject.transform.SetParent (charaSelectPanel.transform, false);
            //位置とスケールを設定
            selectCharaObject.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
            selectCharaObject.transform.localPosition = new Vector3(-4.0f, 118.0f, -100.0f);
            selectCharaObject.GetComponent<Rigidbody2D>().isKinematic = true;
            charaNameText.text = charaName;
            levelText.text = "Lv." + charaLevel;
            growthText.text = growth + "%";
            skillNameText.text = "スキル:" + skName;
            skillDescriptionText.text = skDescription;
            growthProgressObject.GetComponent<Image>().fillAmount = (int.Parse(growth) / 100.0f);

            StartCoroutine(viewStart());
        }
        // 失敗
        else{
            Debug.Log("Get Failure");
        }
    }

    public void charaSelectNext(int nextType=0) {
        if (nextType == 0) {
            if (selectedCharaTableNumber == (characterTable.Rows.Count - 1)) {
                selectedCharaTableNumber = 0;
            } else {
                selectedCharaTableNumber++;
            }
        } else {
            if (selectedCharaTableNumber == 0) {
                selectedCharaTableNumber = (characterTable.Rows.Count - 1);
            } else {
                selectedCharaTableNumber--;
            }
        }
        selectedCharaNumber = (int)characterTable.Rows[selectedCharaTableNumber]["id"];

        //既存キャラを削除
        iTween.ScaleTo(selectCharaObject, iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.1f));
        Destroy(selectCharaObject);

        //キャラ画像設定
        string charaName = "";
        string skName = "";
        string charaLevel = "";
        string growth = "";
        string skDescription = "";

        charaName = (string) characterTable.Rows[selectedCharaTableNumber]["name"];
        skName = (string) characterTable.Rows[selectedCharaTableNumber]["skill_name"];
        charaLevel = (string) characterTable.Rows[selectedCharaTableNumber]["get_count"].ToString();
        growth = (string) characterTable.Rows[selectedCharaTableNumber]["growth"];
        growth = "35";
        skDescription = (string) characterTable.Rows[selectedCharaTableNumber]["skill_description"];

        //charaHeadImage.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + selectedCharaNumber + "/head");
        GameObject selectCharaPrefab = Resources.Load <GameObject> ("Prefab/Chara/Character" + selectedCharaNumber);
        selectCharaObject = GameObject.Instantiate(selectCharaPrefab) as GameObject;
        selectCharaObject.transform.localScale = new Vector3(0, 0, 0);
        //Canvasの子要素として登録する
        selectCharaObject.transform.SetParent (charaSelectPanel.transform, false);
        //位置とスケールを設定
        selectCharaObject.transform.localPosition = new Vector3(-4.0f, 118.0f, -100.0f);
        selectCharaObject.GetComponent<Rigidbody2D>().isKinematic = true;
        charaNameText.text = charaName;
        levelText.text = "Lv." + charaLevel;
        growthText.text = growth + "%";
        skillNameText.text = "スキル:" + skName;
        skillDescriptionText.text = skDescription;
        growthProgressObject.GetComponent<Image>().fillAmount = (int.Parse(growth) / 100.0f);

        iTween.ScaleTo(selectCharaObject, iTween.Hash("x", 0.13f, "y", 0.13f, "z", 0.13f, "time", 0.1f));
    }

    public void charaChange() {
        selectedCharaNumber = (int)characterTable.Rows[selectedCharaTableNumber]["id"];
        charaHeadImage.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + selectedCharaNumber + "/head");

        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string query = "update Character set select_flg=0";
        sqlDB.ExecuteNonQuery(query);

        query = "update Character set select_flg=1 where id = " + selectedCharaNumber;
        sqlDB.ExecuteNonQuery(query);
    }
}
