using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class NewGameStartScript : MonoBehaviour {

    //------初期設定系------
    private float floorDefaultPositionX = 5.58f;
    private float floorDefaultPositionY = -1.2f;
    private float addFloorPositionX = 5.11f;
    private float addFloorCount = 1;
    private float moveSpeed = 0.045f;
    private int limitTime = 60;
    private int basePoint = 2;
    private float baseSpeed = 1.0f;
    private int maxFloorNumber = 9;

    private int point = 0;
    private float timeleft = 10.0f;
    private bool checkGroundFlg = false;

    private int stageNumber = 1;
    private int difficultyType = 1;

    //GameObject系
    public GameObject[] sceneChangeObject;

    private GameObject chara;
    public SpriteRenderer sr;
    public LayerMask groundlayer;

    //ゲームスタート系
    public GameObject canvasObject;
    public GameObject[] startObject;

    //ゲーム終了系
    public GameObject panelObject;
    public GameObject endDialogObject;
    public GameObject endTitleObject;
    public GameObject endScoreObject;
    public GameObject endHighScoreObject;
    public GameObject endCoinObject;
    public Text endCoinText;
    public GameObject endMenuButton;
    public GameObject endRetryButton;

    //canvas系
    public GameObject[] timeObject;
    public GameObject progressObject;
    public GameObject[] hpObject;
    public Text scoreTextObject;
    public Text speedTextObject;

    //CameraScript
    public GameObject cameraObject;
    private CameraScript cameraScript;

    //BGM
    public AudioSource bgmObject;

    //ゲームスタートフラグ
    private bool startFlg = false;
    //入力受付
    private bool isMove = false;
    private int jumpCount = 0;
    private Vector3 touchPos;
    private Vector3 touchWorldPos;
    //接地フラグ
    private bool isGrounded = true;
    //キャラの初期Ｘ値
    private float charaDefaultPositionX;
    //時間
    private DateTime startTime;
    //残りHP
    private int hp = 3;
    private int cd;
    //ダメージフラグ
    private bool damageFlg = false;
    private int damageNumber = 1;

    //スキル設定系
    private bool skillIntervalFlg = false;
    public float skillWaitTime = 5.0f;
    public GameObject skillButtonCoverObject;
    public GameObject skillButtonCountText;
    public GameObject skillEffectFrameObject;
    private int skillNumber = 0;
    private string skillName = "";
    private int skillType = 0;
    private int skillLevel = 1;
    private int charaNumber = 0;
    private int skillCount = 2;

    // Use this for initialization
    void Start () {
        Resources.UnloadUnusedAssets();

        StartCoroutine(viewStart());
        //使用中のキャラ取得
        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string selectQuery = "select * from Character where select_flg = 1";
        DataTable characterTable = sqlDB.ExecuteQuery(selectQuery);
        if (characterTable.Rows.Count >= 1) {
            charaNumber = (int) characterTable.Rows[0]["id"];
            GameObject selectCharaPrefab = Resources.Load <GameObject> ("Prefab/Chara/Character" + charaNumber);
            chara = GameObject.Instantiate(selectCharaPrefab) as GameObject;
            chara.transform.localPosition = new Vector3(-2.24f, 11.56f, -1.0f);
            chara.name = "Character";

            //スキル設定
            skillNumber = (int) characterTable.Rows[0]["skill_number"];
            skillName = (string) characterTable.Rows[0]["skill_name"];
            skillType = (int) characterTable.Rows[0]["skill_type"];
            skillLevel = (int) characterTable.Rows[0]["get_count"];

            addSkillCount();

            //キャラを落ちないように設定
            chara.GetComponent<Rigidbody2D>().isKinematic = true;

            sr = chara.GetComponent<SpriteRenderer>();
            charaDefaultPositionX = chara.transform.localPosition.x;

            StartCoroutine(gameStart());
        }

        //CameraScriptを取得
        cameraScript = (CameraScript) cameraObject.GetComponent<CameraScript>();
    }

    IEnumerator viewStart() {
        iTween.MoveTo(sceneChangeObject[0], iTween.Hash(
                    "position", new Vector3(3, 479, -500),
                    "time", 1.0f,
                    "islocal", true,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));
        iTween.MoveTo(sceneChangeObject[1], iTween.Hash(
                    "position", new Vector3(3, -487, -500),
                    "time", 1.0f,
                    "islocal", true,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(1.1f);
    }

    // Update is called once per frame
    void Update() {

        //時間更新
        CalcRealDeltaTime();

        if (startFlg && !damageFlg) {
            if (skillIntervalFlg) {
                float amo = 1.0f / skillWaitTime * Time.deltaTime;
                skillButtonCoverObject.GetComponent<Image>().fillAmount -= amo;
                if (skillButtonCoverObject.GetComponent<Image>().fillAmount <= 0) {
                    skillIntervalFlg = false;
                    skillEffectFrameObject.transform.localPosition = new Vector3(410, -10, -100);
                }
            }

            //接地判定
            isGrounded = Physics2D.Linecast(
                    chara.transform.position, chara.transform.position - chara.transform.up * 1.2f, groundlayer);

            //キャラを前進
            if (chara.transform.localRotation.y == 0) {
                chara.transform.Translate(new Vector2((moveSpeed * baseSpeed), 0.0f * Time.deltaTime));
            } else {
                chara.transform.Translate(new Vector2((moveSpeed * baseSpeed * -1.0f), 0.0f * Time.deltaTime));
            }

            if (cameraScript.moveOffsetX != (moveSpeed * baseSpeed)) {
                cameraScript.moveOffsetX = (moveSpeed * baseSpeed);
            }

            if (checkGroundFlg && isGrounded && jumpCount != 0) {
                chara.GetComponent<Animation>().Play("Idle");
                checkGroundFlg = false;
                jumpCount = 0;
            }

            //point増加
            addScore((int)(basePoint * baseSpeed));

            timeleft -= realDeltaTime;
            if (timeleft <= 0.0) {
                timeleft = 10.0f;

                baseSpeed += 0.1f;

                Time.timeScale = baseSpeed;
                speedTextObject.text = "速度×" + baseSpeed.ToString();

                errorMessage("スピードアップ！");
                //BGMのピッチをあげる
                bgmObject.pitch = 1.0f + 0.1f;
            }

            if (hp <= 0) {
                goal();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (startFlg && !damageFlg) {
            if (!chara.GetComponent<Animation>().IsPlaying("Sliding") && colliderX != 0) {
                chara.GetComponent<BoxCollider2D>().size = new Vector2(colliderX, colliderY);
            }

            if (Input.GetMouseButtonDown(0)) {
                Debug.Log("TOUCH DOWN");
                touchPos = Input.mousePosition;
                touchWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            } else if (Input.GetMouseButtonUp(0) && touchPos.y != 0) {
                Debug.Log("TOUCH UP");
                if (touchWorldPos.y >= -4.0f) {
                    Vector3 releasePos = Input.mousePosition;
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float swipeDistanceY = releasePos.y - touchPos.y;

                    /*
                    if (isGrounded && swipeDistanceY < -35) {
                        sliding();
                        touchTrueEffect(worldPos.x, worldPos.y);
                        */
                    if (jumpCount <= 1 && worldPos.y <= 2.3f) {
                        Vector2 force = (Vector2.up * 1200f);
                        jumpCount++;
                        if (jumpCount == 1) {
                            Invoke("checkGround", 0.4f);
                        }

                        chara.GetComponent<Animation>().Play("Jump");
                        chara.GetComponent<Rigidbody2D>().AddForce(force);

                        touchTrueEffect(worldPos.x, worldPos.y);
                    }
                }

                touchPos = new Vector3(0, 0, 0);
            }
        }
    }

    //ジャンプ
    private float charaAfterPositionY = 0;

    private float colliderX = 0;
    private float colliderY = 0;
    public void slidingStart() {
        //chara.GetComponent<Rigidbody2D>().isKinematic = true;
        //chara.GetComponent<BoxCollider2D>().isTrigger = true;
        colliderX = chara.GetComponent<BoxCollider2D>().size.x;
        colliderY = chara.GetComponent<BoxCollider2D>().size.y;
        chara.transform.localPosition = new Vector3(chara.transform.localPosition.x, (chara.transform.localPosition.y + 0.5f), chara.transform.localPosition.z);
        chara.GetComponent<BoxCollider2D>().size = new Vector2(colliderY, colliderX);

        chara.GetComponent<Animation>().Play("Sliding");
    }

    public void slidingEnd() {
        //chara.GetComponent<Rigidbody2D>().isKinematic = false;
        //chara.GetComponent<BoxCollider2D>().isTrigger = false;
        chara.GetComponent<Animation>().Play("Idle");
        chara.transform.localPosition = new Vector3(chara.transform.localPosition.x, (chara.transform.localPosition.y + 0.5f), chara.transform.localPosition.z);
        chara.GetComponent<BoxCollider2D>().size = new Vector2(colliderX, colliderY);
    }

    public void dash() {
        if (startFlg) {
            chara.transform.localPosition = new Vector3(chara.transform.localPosition.x + 0.4f, chara.transform.localPosition.y, chara.transform.localPosition.z);
        }
    }

    private void checkGround() {
        checkGroundFlg = true;
    }

    //移動OKタッチエフェクト
    void touchTrueEffect(float x, float y) {
        GameObject circlePrefab = (GameObject)Resources.Load("Prefab/CircleTrue");
        Instantiate (circlePrefab, new Vector3(x, y, -2), Quaternion.identity);
    }

    //移動NGタッチエフェクト
    void touchFalseEffect(float x, float y) {
        GameObject circlePrefab = (GameObject)Resources.Load("Prefab/CircleFalse");
        Instantiate (circlePrefab, new Vector3(x, y, -2), Quaternion.identity);
    }

    //当たり判定
    void OnTriggerExit2D(Collider2D collider){
        if(collider.gameObject.tag == "Star1"){
            addScore(50);
        }
    }


    void badMove() {
        isMove = true;
        iTween.ValueTo(gameObject, iTween.Hash(
                    "from",0,
                    "to",0.5f,
                    "time",0.2f,
                    "looptype","pingpong",
                    "onupdate","ValueChange"
                    ));

        StartCoroutine(stopAction(1.0f));
    }

    IEnumerator stopAction(float tm) {
        yield return new WaitForSeconds(tm);
        //sr.color = new Color(1, 1, 1, 1.0f);
        //chara.GetComponent<SkinnedMeshRenderer>().material.tintColor = new Color(1, 1, 1, 1.0f);
        chara.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        iTween.Stop(gameObject);
    }

    void ValueChange(float value){
        //sr.color = new Color(1, 1, 1, value);
        chara.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, value));
    }

    private void CompleteHandler() {
        chara.GetComponent<Animation>().Play("Idle");
        isMove = false;
    }

    private void floorCompleteHandler() {

    }

    IEnumerator gameStart() {
        chara.GetComponent<Rigidbody2D>().isKinematic = false;

        yield return new WaitForSeconds(0.5f);
        //GameObjectを生成、生成したオブジェクトを変数に代入
        GameObject prefab = (GameObject)Instantiate(startObject[0]); 
        prefab.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        yield return new WaitForSeconds(0.3f);
        //Canvasの子要素として登録する
        prefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(1.5f);

        Destroy(prefab);

        //GameObjectを生成、生成したオブジェクトを変数に代入
        prefab = (GameObject)Instantiate (startObject[1]); 
        //Canvasの子要素として登録する 
        prefab.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        yield return new WaitForSeconds(0.3f);
        prefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(0.7f);
        Destroy(prefab);

        startFlg = true;

    }

    public void goal() {
        if (startFlg) {
            startFlg = false;
            Time.timeScale = 1;
            StartCoroutine(gameCleared());
        }
    }

    IEnumerator gameCleared() {
        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string selectQuery = "select * from Stage where stage_number = " + stageNumber + " and difficulty = " + difficultyType;
        DataTable stageTable = sqlDB.ExecuteQuery(selectQuery);
        bool highScoreFlg = false;
        if (stageTable.Rows.Count == 0) {
            highScoreFlg = true;
        } else {
            int beforeScore = (int)stageTable.Rows[0]["score"];
            if (beforeScore < point) {
                highScoreFlg = true;
            }
        }

        //panelの表示
        GameObject panelPrefab = (GameObject)Instantiate(panelObject);
        panelPrefab.GetComponent<Image>().color = new Color(255f, 255f, 255f, 0);
        panelPrefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(0.2f);

        //ダイアログの表示
        GameObject dialogPrefab = (GameObject)Instantiate(endDialogObject);
        float scaleX = dialogPrefab.transform.localScale.x;
        float scaleY = dialogPrefab.transform.localScale.y;
        float scaleZ = dialogPrefab.transform.localScale.z;
        dialogPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        dialogPrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(dialogPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        //タイトルの表示
        GameObject endTitlePrefab = (GameObject)Instantiate(endTitleObject);
        scaleX = endTitlePrefab.transform.localScale.x;
        scaleY = endTitlePrefab.transform.localScale.y;
        scaleZ = endTitlePrefab.transform.localScale.z;
        endTitlePrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        endTitlePrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(endTitlePrefab, iTween.Hash("x", scaleX, "y", scaleY, "z", scaleZ, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        //スコアの表示
        GameObject scorePrefab = (GameObject)Instantiate(endScoreObject);
        scaleX = scorePrefab.transform.localScale.x;
        scaleY = scorePrefab.transform.localScale.y;
        scaleZ = scorePrefab.transform.localScale.z;
        scorePrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        scorePrefab.transform.SetParent (canvasObject.transform, false);
        //スコアのテキストを変更
        GameObject scoreText = scorePrefab.transform.FindChild("ScoreText").gameObject;
        scoreText.GetComponent<Text>().text = point.ToString();
        iTween.ScaleTo(scorePrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        //ハイスコアの表示
        if (highScoreFlg) {
            GameObject highScorePrefab = (GameObject)Instantiate(endHighScoreObject);
            scaleX = highScorePrefab.transform.localScale.x;
            scaleY = highScorePrefab.transform.localScale.y;
            scaleZ = highScorePrefab.transform.localScale.z;
            highScorePrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
            highScorePrefab.transform.SetParent (canvasObject.transform, false);
            iTween.ScaleTo(highScorePrefab, iTween.Hash("x", scaleX, "y", scaleY, "z", scaleZ, "time", 0.3f));
            yield return new WaitForSeconds(0.2f);
        }

        //コインの表示
        GameObject coinPrefab = (GameObject)Instantiate(endCoinObject);
        scaleX = coinPrefab.transform.localScale.x;
        scaleY = coinPrefab.transform.localScale.y;
        scaleZ = coinPrefab.transform.localScale.z;
        coinPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        coinPrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(coinPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        //コインテキストの表示
        Text coinTextPrefab = (Text)Instantiate(endCoinText);
        coinTextPrefab.transform.SetParent (canvasObject.transform, false);
        //コインの計算
        int coin = (point / 1000);
        coinTextPrefab.text = coin.ToString();
        yield return new WaitForSeconds(0.2f);

        //ボタンの表示
        GameObject endButtonPrefab = (GameObject)Instantiate(endMenuButton);
        scaleX = endButtonPrefab.transform.localScale.x;
        scaleY = endButtonPrefab.transform.localScale.y;
        scaleZ = endButtonPrefab.transform.localScale.z;
        endButtonPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        endButtonPrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(endButtonPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        endButtonPrefab = (GameObject)Instantiate(endRetryButton);
        scaleX = endButtonPrefab.transform.localScale.x;
        scaleY = endButtonPrefab.transform.localScale.y;
        scaleZ = endButtonPrefab.transform.localScale.z;
        endButtonPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        endButtonPrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(endButtonPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        string query;
        //インサート
        if (stageTable.Rows.Count == 0) {
            query = "insert into Stage(stage_number, difficulty, star, remaining_time, remaining_hp, score, created, updated) values(" + stageNumber + "," + difficultyType + ",0,0,0," + point + ",datetime(),datetime())";
            sqlDB.ExecuteNonQuery(query);
        //アップデート
        } else {
            int stageID = (int)stageTable.Rows[0]["id"];

            if (highScoreFlg) {
                query = "update Stage set star=0, remaining_time=0, remaining_hp=0, score=" + point + ",updated=dateTime() where id=" + stageID;
                sqlDB.ExecuteNonQuery(query);
            }
        }

        //お金アップデート
        query = "update UserStatus set money = (money + " + coin + ") where id = 1";
        sqlDB.ExecuteNonQuery(query);


        /*
        int clearTime = cd;

        //GameObjectを生成、生成したオブジェクトを変数に代入
        GameObject panelPrefab = (GameObject)Instantiate(panelObject); 
        panelPrefab.GetComponent<Image>().color = new Color(255f, 255f, 255f, 0);
        //Canvasの子要素として登録する 
        panelPrefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(0.2f);

        //ダイアログの表示
        GameObject clearDialogPrefab = (GameObject)Instantiate(clearDialogObject); 
        float scaleX = clearDialogPrefab.transform.localScale.x;
        float scaleY = clearDialogPrefab.transform.localScale.y;
        float scaleZ = clearDialogPrefab.transform.localScale.z;
        clearDialogPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        //Canvasの子要素として登録する 
        clearDialogPrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(clearDialogPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        //Complete
        GameObject completePrefab = (GameObject)Instantiate(completeObject); 
        scaleX = completePrefab.transform.localScale.x;
        scaleY = completePrefab.transform.localScale.y;
        scaleZ = completePrefab.transform.localScale.z;
        completePrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        //Canvasの子要素として登録する 
        completePrefab.transform.SetParent (canvasObject.transform, false);
        if (completePrefab.GetComponent<Text>() == null) {
            iTween.ScaleTo(completePrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        } else {
            iTween.ScaleTo(completePrefab, iTween.Hash("x", 0.3f, "y", 0.3f, "z", 0.3f, "time", 0.3f));
        }

        yield return new WaitForSeconds(0.2f);

        //star
        //星の数を計算する
        float decPer = (clearTime * 0.5f) / (limitTime * 1.0f);
        float decHpPer = (hp * 0.5f) / 2 * decPer;
        float totalPer = decPer + decHpPer;
        Debug.Log("DEC PER:" + decPer);
        Debug.Log("DEC HP PER:" + decHpPer);
        Debug.Log("TOTAL PER:" + totalPer);

        //星の数
        int starCount = 1;
        if (totalPer >= 0.49f) {
            starCount = 3;
        } else if (totalPer >= 0.35f) {
            starCount = 2;
        }

        int score = (int) Math.Ceiling(totalPer * 10000);
        int coin = (int) Math.Ceiling(totalPer * 10);
        Debug.Log("COIN:" + coin);

        //セーブ
        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string selectQuery = "select * from Stage where stage_number = " + stageNumber + " and difficulty = " + difficultyType;
        DataTable stageTable = sqlDB.ExecuteQuery(selectQuery);

        string query;
        //インサート
        if (stageTable.Rows.Count == 0) {
            query = "insert into Stage(stage_number, difficulty, star, remaining_time, remaining_hp, score, created, updated) values(" + stageNumber + "," + difficultyType + "," + starCount + "," + clearTime + "," + hp + "," + score + ",datetime(),datetime())";
            sqlDB.ExecuteNonQuery(query);
        //アップデート
        } else {
            int beforeScore = (int)stageTable.Rows[0]["score"];
            int stageID = (int)stageTable.Rows[0]["id"];
            if (beforeScore < score) {
                query = "update Stage set star=" + starCount + ", remaining_time=" + hp +", remaining_hp=" + hp + ", score=" + score + ",updated=dateTime() where id=" + stageID;
                sqlDB.ExecuteNonQuery(query);
            }
        }

        //お金アップデート
        query = "update UserStatus set money = (money + " + coin + ") where id = 1";
        sqlDB.ExecuteNonQuery(query);

        //結果ダイアログ
        GameObject resultPrefab = (GameObject)Instantiate(resultDialogObject);
        scaleX = resultPrefab.transform.localScale.x;
        scaleY = resultPrefab.transform.localScale.y;
        scaleZ = resultPrefab.transform.localScale.z;
        resultPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));

        //Canvasの子要素として登録する
        resultPrefab.transform.SetParent (canvasObject.transform, false);
        GameObject decTimeChild1 = resultPrefab.transform.FindChild("time_number_1").gameObject;
        GameObject decTimeChild2 = resultPrefab.transform.FindChild("time_number_2").gameObject;
        GameObject decTimeChild3 = resultPrefab.transform.FindChild("time_number_3").gameObject;

        String stTime = clearTime.ToString();
        String number1 = "0";
        String number2 = "0";
        String number3 = "0";

        if (stTime.Length >= 3) {
            number1 = stTime.Substring(0, 1);
            number2 = stTime.Substring(1, 1);
            number3 = stTime.Substring(2, 1);
        } else if (stTime.Length >= 2) {
            number2 = stTime.Substring(0, 1);
            number3 = stTime.Substring(1, 1);
        } else {
            number3 = stTime.Substring(0, 1);
        }

        decTimeChild1.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + number1);
        decTimeChild2.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + number2);
        decTimeChild3.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + number3);

        //hp設定
        GameObject decHpChild = resultPrefab.transform.FindChild("hp_number_1").gameObject;
        String stHp = hp.ToString();
        decHpChild.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + stHp);

        //coin設定
        GameObject coinChild1 = resultPrefab.transform.FindChild("coin_number_1").gameObject;
        GameObject coinChild2 = resultPrefab.transform.FindChild("coin_number_2").gameObject;
        String stCoin = coin.ToString();
        String coin1= "0";
        String coin2= "0";
        if (stCoin.Length >= 2) {
            coin2 = stCoin.Substring(0, 1);
            coin1 = stCoin.Substring(1, 1);
        } else {
            coin1 = stCoin.Substring(0, 1);
        }
        coinChild1.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + coin1);
        coinChild2.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + coin2);

        iTween.ScaleTo(resultPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.2f);

        GameObject starPrefab;
        for (int i = 0; i < starObject.Length; i++) {
            starPrefab = (GameObject)Instantiate(starObject[i]); 
            scaleX = starPrefab.transform.localScale.x;
            scaleY = starPrefab.transform.localScale.y;
            scaleZ = starPrefab.transform.localScale.z;

            starPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));

            if (i == 1 && totalPer <= 0.35f) {
                starPrefab.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/item_star_lost");
            } else if (i == 2 && totalPer <= 0.49f) {
                starPrefab.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/item_star_lost");
            }

            //Canvasの子要素として登録する 
            starPrefab.transform.SetParent (canvasObject.transform, false);

            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(starPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));

            yield return new WaitForSeconds(0.2f);
        }

        //button
        GameObject endButtonPrefab;
        for (int i = 0; i < endButtonObject.Length; i++) {
            endButtonPrefab = (GameObject)Instantiate(endButtonObject[i]);
            scaleX = endButtonPrefab.transform.localScale.x;
            scaleY = endButtonPrefab.transform.localScale.y;
            scaleZ = endButtonPrefab.transform.localScale.z;

            endButtonPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));

            //Canvasの子要素として登録する 
            endButtonPrefab.transform.SetParent (canvasObject.transform, false);
            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(endButtonPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));

            yield return new WaitForSeconds(0.2f);
        }
        */
            yield return new WaitForSeconds(0.2f);
    }

    public void retryGame() {
        StartCoroutine(viewChange("NewRaceScene"));
    }

    public void gameTop() {
        StartCoroutine(viewChange("TopScene"));
    }

    IEnumerator viewChange(string sceneName) {
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

    public void actionSkill() {
        if (!skillIntervalFlg) {
            if (skillCheck()) {
                skillIntervalFlg = true;
                skillButtonCoverObject.GetComponent<Image>().fillAmount = 1.0f;
                StartCoroutine(showSkill());
            }
        } else {
            errorMessage("まだ使用できません");
        }
    }

    private bool skillCheck() {
        if (skillType != 1) {
            errorMessage("このスキルはバトルでのみ使用できます");
            return false;
        }

        if (skillCount == 0) {
            errorMessage("スキル残数がありません");
            return false;
        }

        if (skillNumber == 1) {
            if (hp >= 3) {
                errorMessage("HPが最大です");
                return false;
            } else {
                skillCount--;
                changeSkillCountText(skillCount);
                return true;
            }
        } else if (skillNumber == 4) {
            skillCount--;
            changeSkillCountText(skillCount);
            return true;
        } else {
            return false;
        }
    }

    IEnumerator showSkill() {
        GameObject skillCharaImage = skillEffectFrameObject.transform.FindChild("SkillChara").gameObject;
        GameObject skillNameObj = skillEffectFrameObject.transform.FindChild("SkillText").gameObject;
        //スキルキャラ画像の差し替え
        skillCharaImage.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + charaNumber + "/head");
        //スキル名差し替え
        skillNameObj.GetComponent<Text>().text = skillName;

        iTween.MoveTo(skillEffectFrameObject, iTween.Hash(
                    "position", new Vector3(0, -10, -100),
                    "time", 0.5f, 
                    "islocal", true
                    ));
        yield return new WaitForSeconds(0.8f);

        iTween.MoveTo(skillEffectFrameObject, iTween.Hash(
                    "position", new Vector3(-420, -10, -100),
                    "time", 0.5f, 
                    "islocal", true
                    ));

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(effectSkill());
    }

    IEnumerator effectSkill() {
        //回復スキル
        if (skillNumber == 1) {
            GameObject skEffectPrefab = (GameObject)Resources.Load("Effect/Heal");
            GameObject skEffectObj = GameObject.Instantiate(skEffectPrefab) as GameObject;
            skEffectObj.transform.SetParent (chara.transform, false);
            skEffectObj.transform.localPosition = new Vector3(
                skEffectObj.transform.localPosition.x,
                (skEffectObj.transform.localPosition.y - 200.0f),
                skEffectObj.transform.localPosition.z
                );

            yield return new WaitForSeconds(0.5f);
            iTween.ScaleTo(hpObject[hp], iTween.Hash("x", 29.3f, "y", 29.3f, "z", 29.3f, "time", 0.4f));
            hp++;
        //移動スキル
        } else if (skillNumber == 4) {
            GameObject wind = (GameObject)Resources.Load("Effect/Wind");
            GameObject skEffectObj = GameObject.Instantiate(wind) as GameObject;
            skEffectObj.transform.SetParent (chara.transform, false);

            isMove = true;

            skEffectObj.transform.localPosition = new Vector3(
                chara.transform.localPosition.x,
                (chara.transform.localPosition.y - 200.0f),
                chara.transform.localPosition.z
                );

            yield return new WaitForSeconds(1.0f);
            int forwardCount = 3 + (1 * skillLevel);

            //ブロック追加
            /*
            if ((blockCount - (charaMoveCount + forwardCount)) <= 4) {
                int step = 5 - (blockCount - (charaMoveCount + forwardCount));
                //StartCoroutine(createFloor(step));
            }
            */

            StartCoroutine(forwardSkillAction(forwardCount));
        }

        yield return new WaitForSeconds(1.0f);
    }

    private void errorMessage(string mes) {
        GameObject errorPrefab = (GameObject)Resources.Load("Prefab/Canvas/ErrorMessage");
        errorPrefab.GetComponent<Text>().text = mes;
        GameObject errorObj = GameObject.Instantiate(errorPrefab) as GameObject;
        errorObj.transform.SetParent (canvasObject.transform, false);
    }

    private void addSkillCount() {
        if (skillNumber == 1) {
            skillCount = skillLevel;
        }

        changeSkillCountText(skillCount);
    }

    private void changeSkillCountText(int cnt) {
        skillButtonCountText.GetComponent<Text>().text = cnt.ToString();
    }

    IEnumerator forwardSkillAction(int fc) {
        isMove = true;
        yield return new WaitForSeconds(0.4f);
    }

    public void addFloor() {
        //ランダムでフロアを選択
        float randomFloorNumber = UnityEngine.Random.Range(1, (maxFloorNumber+1));

        GameObject floorPrefab = Resources.Load <GameObject> ("Prefab/Stage/Floor/FloorSet" + randomFloorNumber);
        GameObject floor = GameObject.Instantiate(floorPrefab) as GameObject;
        floor.transform.localPosition = new Vector3((floorDefaultPositionX + (addFloorPositionX * addFloorCount)), floorDefaultPositionY, -1.0f);

        addFloorCount++;

        if (addFloorCount == 2) {
            addFloor();
        }
    }

    private void addScore(int score) {
        point += score;
        scoreTextObject.text = point.ToString();
    }

    //timeScaleに影響されない時間
    float realDeltaTime;
    float lastRealTime;

    //現実時間基準でデルタ時間を求める.
    void CalcRealDeltaTime() {
        if(lastRealTime == 0) {
            lastRealTime = Time.realtimeSinceStartup;
        }
        realDeltaTime = Time.realtimeSinceStartup - lastRealTime;
        lastRealTime = Time.realtimeSinceStartup;
    }

    //壁に当たった処理
    public void damageWall() {
        if (startFlg && !damageFlg) {
            damageFlg = true;
            cameraScript.moveOffsetX = 0;
            badMove();

            hp--;
            iTween.ScaleTo(hpObject[hp], iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.5f));

            damageNumber = 1;
            Invoke("charaFall", 0.5f);
        }
    }

    //落ちた処理
    public void damageFall() {
        if (startFlg && !damageFlg) {
            damageFlg = true;
            cameraScript.moveOffsetX = 0;
            badMove();

            hp--;
            iTween.ScaleTo(hpObject[hp], iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.5f));

            damageNumber = 2;
            Invoke("charaFall", 0.5f);
        }
    }

    //ボム処理
    public void damageBomb() {
        if (startFlg && !damageFlg) {
            damageFlg = true;
            cameraScript.moveOffsetX = 0;
            badMove();

            hp--;
            iTween.ScaleTo(hpObject[hp], iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.5f));

            damageNumber = 3;
            Invoke("charaFall", 0.5f);
        }
    }

    private void charaFall() {
        //キャラを上から落とす
        if (damageNumber == 1) {
            chara.transform.localPosition = new Vector3((chara.transform.localPosition.x + 1.5f), 7.0f, chara.transform.localPosition.z);
        } else if (damageNumber == 2) {
            chara.transform.localPosition = new Vector3((chara.transform.localPosition.x + 0.5f), 7.0f, chara.transform.localPosition.z);
        } else if (damageNumber == 3) {
            chara.transform.localPosition = new Vector3((chara.transform.localPosition.x + 1.0f), 7.0f, chara.transform.localPosition.z);
        }

        checkGroundFlg = false;
        jumpCount = 0;
        damageFlg = false;

        chara.GetComponent<Animation>().Play("Idle");
    }
}
