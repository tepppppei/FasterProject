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
    //ステータステーブル
    private DataTable statusTable;

    private string currentDifficulty = "easy";

    //DB
    SqliteDatabase sqlDB;

    void Start () {
        sqlDB = new SqliteDatabase("UserStatus.db");

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
    public GameObject gachaPanel;
    public Text charaNameText;

    //ガチャ実行時のオブジェクト
    public GameObject[] gachaCloseObjectList;
    public GameObject gachaCharacterObject;
    public GameObject gachaCharacterName;
    public GameObject gachaCharacterLevel;
    public Text gachaCharacterLevelText;
    public GameObject gachaCharacterGrowth;
    private int gachaCount = 0;

    //シーン変更時のオブジェクト
    public GameObject[] sceneChangeObject;

    //初期位置
    private Vector3 stageSelectPanelDefaultPosition; 
    private Vector3 charaSelectPanelDefaultPosition;
    private Vector3 gachaPanelDefaultPosition;

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

    //canvas
    public GameObject canvasObject;

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
        string selectQuery = "select * from Character where get_flg = 1";
        characterTable = sqlDB.ExecuteQuery(selectQuery);

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

    //ガチャクローズ用
    public void closeGacha() {
        StartCoroutine(closeGachaPanel());
        initGachaPanel();
    }

    //ガチャ遷移用
    public void gacha() {
        StartCoroutine(gachaShow());
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

    //ガチャ閉じる
    IEnumerator closeGachaPanel() {
        iTween.MoveTo(gachaPanel, iTween.Hash(
                    "position", gachaPanelDefaultPosition,
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

    //ガチャ遷移
    IEnumerator gachaShow() {
        iTween.MoveTo(charaSelectPanel, iTween.Hash(
                    "position", charaSelectPanelDefaultPosition,
                    "time", 0.2f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(0.1f);

        gachaPanelDefaultPosition = gachaPanel.transform.localPosition;
        iTween.MoveTo(gachaPanel, iTween.Hash(
                    "position", new Vector3(0, 0, 0),
                    "time", 0.2f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(0.1f);
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

        StartCoroutine(viewChange("NewRaceScene"));
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
            //SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
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

            //SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");

            // Select
            string selectQuery = "select * from Stage where difficulty = 1";
            easyDataTable = sqlDB.ExecuteQuery(selectQuery);

            selectQuery = "select * from Stage where difficulty = 2";
            normalDataTable = sqlDB.ExecuteQuery(selectQuery);

            selectQuery = "select * from Stage where difficulty = 3";
            hardDataTable = sqlDB.ExecuteQuery(selectQuery);

                //DEBUG用
                string testQuery = "update UserStatus set money = 1000";
                sqlDB.ExecuteNonQuery(testQuery);

            //Statusが無ければインサート
            selectQuery = "select * from UserStatus";
            statusTable = sqlDB.ExecuteQuery(selectQuery);
            if (statusTable.Rows.Count > 0) {


                int baseMoney = (int) statusTable.Rows[0]["money"];
                moneyChange(baseMoney);

            } else {
                string query = "insert into UserStatus(user_name, money, created, updated) values('NoName', 0, datetime(), datetime())";
                sqlDB.ExecuteNonQuery(query);
            }

            //キャラクター系設定
            selectQuery = "select * from Character where get_flg = 1";
            //selectQuery = "select * from Character";
            characterTable = sqlDB.ExecuteQuery(selectQuery);
            string charaName = "";
            string skName = "";
            string charaLevel = "";
            int growth = 0;
            string skDescription = "";

            //現在選択中のキャラ
            for (int i = 0; i < characterTable.Rows.Count; i++) {
                    Debug.Log(characterTable.Rows[i]["name"]);
                if ((int)characterTable.Rows[i]["select_flg"] == 1) {
                    selectedCharaNumber = (int) characterTable.Rows[i]["id"];
                    charaName = (string) characterTable.Rows[i]["name"];
                    skName = (string) characterTable.Rows[i]["skill_name"];
                    charaLevel = (string) characterTable.Rows[i]["get_count"].ToString();
                    growth = (int) characterTable.Rows[i]["growth"];
                    skDescription = (string) characterTable.Rows[i]["skill_description"];

                    selectedCharaTableNumber = i;
                    break;
                }
            }

            if (selectedCharaTableNumber == null) {
                selectedCharaNumber = (int) characterTable.Rows[0]["id"];
                charaName = (string) characterTable.Rows[0]["name"];
                skName = (string) characterTable.Rows[0]["skill_name"];
                charaLevel = (string) characterTable.Rows[0]["get_count"].ToString();
                growth = (int) characterTable.Rows[0]["growth"];
                skDescription = (string) characterTable.Rows[0]["skill_description"];

                selectedCharaTableNumber = 0;
            }

            //キャラ画像設定
            charaHeadImage.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + selectedCharaNumber + "/head");
            GameObject selectCharaPrefab = Resources.Load <GameObject> ("Prefab/Chara/Character" + selectedCharaNumber);
            selectCharaObject = GameObject.Instantiate(selectCharaPrefab) as GameObject;
            //Canvasの子要素として登録する
            selectCharaObject.transform.SetParent (charaSelectPanel.transform, false);
            //位置とスケールを設定
            selectCharaObject.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
            selectCharaObject.transform.localPosition = new Vector3(0, 100.0f, -100.0f);
            selectCharaObject.GetComponent<Rigidbody2D>().isKinematic = true;
            charaNameText.text = charaName;
            levelText.text = "Lv." + charaLevel;
            growthText.text = growth.ToString() + "%";
            skillNameText.text = "スキル:" + skName;
            skillDescriptionText.text = skDescription;
            growthProgressObject.GetComponent<Image>().fillAmount = (growth / 100.0f);

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

        selectedCharaNumber = (int) characterTable.Rows[selectedCharaTableNumber]["id"];

        //既存キャラを削除
        iTween.ScaleTo(selectCharaObject, iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.1f));
        Destroy(selectCharaObject);

        //キャラ画像設定
        string charaName = "";
        string skName = "";
        string charaLevel = "";
        int growth = 0;
        string skDescription = "";

        charaName = (string) characterTable.Rows[selectedCharaTableNumber]["name"];
        skName = (string) characterTable.Rows[selectedCharaTableNumber]["skill_name"];
        charaLevel = (string) characterTable.Rows[selectedCharaTableNumber]["get_count"].ToString();
        growth = (int) characterTable.Rows[selectedCharaTableNumber]["growth"];
        skDescription = (string) characterTable.Rows[selectedCharaTableNumber]["skill_description"];

        //charaHeadImage.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + selectedCharaNumber + "/head");
        GameObject selectCharaPrefab = Resources.Load <GameObject> ("Prefab/Chara/Character" + selectedCharaNumber);
        selectCharaObject = GameObject.Instantiate(selectCharaPrefab) as GameObject;
        selectCharaObject.transform.localScale = new Vector3(0, 0, 0);
        //Canvasの子要素として登録する
        selectCharaObject.transform.SetParent (charaSelectPanel.transform, false);
        //位置とスケールを設定
        selectCharaObject.transform.localPosition = new Vector3(0, 100.0f, -100.0f);
        selectCharaObject.GetComponent<Rigidbody2D>().isKinematic = true;
        charaNameText.text = charaName;
        levelText.text = "Lv." + charaLevel;
        growthText.text = growth.ToString() + "%";
        skillNameText.text = "スキル:" + skName;
        skillDescriptionText.text = skDescription;
        growthProgressObject.GetComponent<Image>().fillAmount = (growth / 100.0f);

        iTween.ScaleTo(selectCharaObject, iTween.Hash("x", 0.13f, "y", 0.13f, "z", 0.13f, "time", 0.1f));
    }

    public void charaChange() {
        selectedCharaNumber = (int)characterTable.Rows[selectedCharaTableNumber]["id"];
        charaHeadImage.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + selectedCharaNumber + "/head");

        //SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string query = "update Character set select_flg=0";
        sqlDB.ExecuteNonQuery(query);

        query = "update Character set select_flg=1 where id = " + selectedCharaNumber;
        sqlDB.ExecuteNonQuery(query);
    }

    public void gachaStart() {
        //SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string selectQuery = "select * from UserStatus";
        statusTable = sqlDB.ExecuteQuery(selectQuery);

        //お金チェック
        int money = (int) statusTable.Rows[0]["money"];
        if (money < 100) {
            errorMessage("コインが足りません。");
        } else {
            //お金を減らす処理
            money -= 100;
            string query = "update UserStatus set money = " + money.ToString();
            sqlDB.ExecuteNonQuery(query);

            iTween.ValueTo(gameObject,iTween.Hash(
                "from", (money + 100),
                "to", money,
                "time",0.5f,
                "onupdate","moneyChange"
                ));

            if (gachaCount >= 1) {
                initGachaPanel();
            }

            StartCoroutine(gachaAction());
        }
    }

    private GameObject temporaryGachaCharacterObject;
    private void initGachaPanel() {
        //ガチャで引いたキャラを削除
        if (temporaryGachaCharacterObject != null) {
            Destroy(temporaryGachaCharacterObject);
        }

        gachaCharacterName.transform.localScale = new Vector3(0, 0, 0);
        gachaCharacterObject.GetComponent<Animation>().Play("Idle");
        gachaCharacterObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        gachaCharacterLevel.transform.localScale = new Vector3(0, 0, 0);
        gachaCharacterGrowth.transform.localScale = new Vector3(0, 0, 0);
    }

    //ガチャ実行
    private int afterLevel = 1;
    IEnumerator gachaAction() {
        //キャラ選定
        //SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string selectQuery = "select * from Character";
        DataTable allCharacterTable = sqlDB.ExecuteQuery(selectQuery);
        int gachaCharaNumber = (int) UnityEngine.Random.Range(0, (allCharacterTable.Rows.Count));
        Debug.Log("NUMBER:" + gachaCharaNumber);
        Debug.Log(allCharacterTable.Rows[gachaCharaNumber]["name"] + "GET!!");

        int beforeLevel = 1;
        int beforeGrowth = 0;
        int afterGrowth = 0;

        //取得チェック
        if ((int) allCharacterTable.Rows[gachaCharaNumber]["get_flg"] == 1) {
        //取得済みの場合
            beforeLevel = (int) allCharacterTable.Rows[gachaCharaNumber]["get_count"];
            beforeGrowth = (int) allCharacterTable.Rows[gachaCharaNumber]["growth"];

            if (beforeLevel == 1) {
                afterLevel = 2;
            } else if (beforeLevel == 2) {
                afterLevel = beforeLevel;
                if (beforeGrowth == 0) {
                    afterGrowth = 50;
                } else {
                    afterLevel = 3;
                }
            } else {
                afterLevel = beforeLevel;
                if (beforeGrowth < 75) {
                    afterGrowth = beforeGrowth + 25;
                } else {
                    afterLevel = beforeLevel++;
                }
            }

            string query = "update Character set get_count = " + afterLevel.ToString() + ",growth = " + afterGrowth.ToString() + " where id = " + (gachaCharaNumber+1).ToString();
            sqlDB.ExecuteNonQuery(query);
        } else {
        //未取得の場合
            string query = "update Character set get_flg = 1, get_count = 1 where id = " + (gachaCharaNumber+1).ToString();
            sqlDB.ExecuteNonQuery(query);
        }

        //スタートボタン系を削除
        foreach (GameObject n in gachaCloseObjectList) {
            iTween.ScaleTo(n, iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.05f));
            yield return new WaitForSeconds(0.1f);
        }

        GameObject gachaEffect = Resources.Load <GameObject> ("Effect/Gacha");
        GameObject gachaEffectObject = GameObject.Instantiate(gachaEffect) as GameObject;
        gachaEffectObject.transform.SetParent (gachaPanel.transform, false);
        yield return new WaitForSeconds(1.3f);

        gachaCharacterObject.GetComponent<Animation>().Play("Open");

        yield return new WaitForSeconds(1.0f);
        GameObject charaPrefab = Resources.Load <GameObject> ("Prefab/Chara/Character" + (gachaCharaNumber+1));
        temporaryGachaCharacterObject = GameObject.Instantiate(charaPrefab) as GameObject;
        temporaryGachaCharacterObject.transform.localScale = new Vector3(0, 0, 0);
        temporaryGachaCharacterObject.transform.localPosition = new Vector3(0, 35.0f, -100.0f);
        temporaryGachaCharacterObject.GetComponent<Rigidbody2D>().isKinematic = true;

        temporaryGachaCharacterObject.transform.SetParent (gachaPanel.transform, false);
        //位置とスケールを設定

        yield return new WaitForSeconds(2.0f);
        Destroy(gachaEffectObject, 0.8f);

        iTween.ScaleTo(temporaryGachaCharacterObject, iTween.Hash("x", 0.2f, "y", 0.2f, "z", 0.2f, "time", 1.3f));
        GameObject childObject = gachaCharacterName.transform.FindChild("Text").gameObject;
        childObject.GetComponent<Text>().text = allCharacterTable.Rows[gachaCharaNumber]["name"] + "GET!!";
        iTween.ScaleTo(gachaCharacterName, iTween.Hash("x", 1.0f, "y", 1.0f, "z", 1.0f, "time", 1.3f));
        yield return new WaitForSeconds(0.8f);

        gachaCharacterObject.transform.localScale = new Vector3(0, 0, 0);

        //Lvとゲージを表示
        gachaCharacterLevelText.text = "Lv." + beforeLevel;
        iTween.ScaleTo(gachaCharacterLevel, iTween.Hash("x", 0.3f, "y", 0.3f, "z", 0.3f, "time", 0.8f));

        GameObject growthChildObject = gachaCharacterGrowth.transform.FindChild("Progress").gameObject;
        GameObject growthChildTextObject = gachaCharacterGrowth.transform.FindChild("GrowthText").gameObject;
        growthChildObject.GetComponent<Image>().fillAmount = (beforeGrowth / 100);
        growthChildTextObject.GetComponent<Text>().text = beforeGrowth.ToString() + "%";
        iTween.ScaleTo(gachaCharacterGrowth, iTween.Hash("x", 1.0f, "y", 1.0f, "z", 1.0f, "time", 0.8f));
        yield return new WaitForSeconds(0.8f);

        if (afterLevel > 1) {
            if (beforeGrowth < afterGrowth) {
                iTween.ValueTo(gameObject, iTween.Hash(
                    "from", beforeGrowth,
                    "to", afterGrowth,
                    "time", 0.5f,
                    "onupdate", "changeGrowthProgress"
                    ));
            } else if (afterGrowth == 0) {
                iTween.ValueTo(gameObject, iTween.Hash(
                    "from", beforeGrowth,
                    "to", 100,
                    "time", 0.5f,
                    "onupdate", "changeGrowthProgress"
                    ));
            }
        }


        //ボタンを元に戻す
        foreach (GameObject n in gachaCloseObjectList) {
            iTween.ScaleTo(n, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.05f));
            yield return new WaitForSeconds(0.1f);
        }

        gachaCount++;

        yield return new WaitForSeconds(2.0f);
    }

    //ガチャGrowthプログレスバー変更用
    private void changeGrowthProgress(int num) {
        GameObject childObject = gachaCharacterGrowth.transform.FindChild("Progress").gameObject;
        childObject.GetComponent<Image>().fillAmount = (num / 100.00f);

        GameObject growthChildTextObject = gachaCharacterGrowth.transform.FindChild("GrowthText").gameObject;
        growthChildTextObject.GetComponent<Text>().text = num.ToString() + "%";

        if (num == 100) {
            gachaCharacterLevelText.text = "Lv." + afterLevel;

            GameObject growthChildObject = gachaCharacterGrowth.transform.FindChild("Progress").gameObject;
            growthChildObject.GetComponent<Image>().fillAmount = 0;
            growthChildTextObject.GetComponent<Text>().text = "0%";
        }
    }

    //お金の表示を変更する処理
    private GameObject tempMoney1;
    private GameObject tempMoney2;
    private GameObject tempMoney3;
    private GameObject tempMoney4;
    private void moneyChange(int baseMoney) {
        string money = baseMoney.ToString();
        if (tempMoney1 == null) {
            tempMoney1 = GameObject.Find("number1").gameObject;
            tempMoney2 = GameObject.Find("number2").gameObject;
            tempMoney3 = GameObject.Find("number3").gameObject;
            tempMoney4 = GameObject.Find("number4").gameObject;
        }

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
            tempMoney4.transform.localScale = new Vector3(0, 0, 0);
            m3 = money.Substring(0, 1);
            m2 = money.Substring(1, 1);
            m1 = money.Substring(2, 1);
        } else if (money.Length >= 2) {
            tempMoney4.transform.localScale = new Vector3(0, 0, 0);
            tempMoney3.transform.localScale = new Vector3(0, 0, 0);
            m2 = money.Substring(0, 1);
            m1 = money.Substring(1, 1);
        } else {
            tempMoney4.transform.localScale = new Vector3(0, 0, 0);
            tempMoney3.transform.localScale = new Vector3(0, 0, 0);
            tempMoney2.transform.localScale = new Vector3(0, 0, 0);
            m1 = money.Substring(0, 1);
        }

        tempMoney1.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m1);
        tempMoney2.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m2);
        tempMoney3.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m3);
        tempMoney4.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + m4);
    }

    private void errorMessage(string mes) {
        GameObject errorPrefab = (GameObject)Resources.Load("Prefab/Canvas/ErrorMessage");
        errorPrefab.GetComponent<Text>().text = mes;
        GameObject errorObj = GameObject.Instantiate(errorPrefab) as GameObject;
        errorObj.transform.SetParent (canvasObject.transform, false);
    }
}
