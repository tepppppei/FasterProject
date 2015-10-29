using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class BattleGameStartScript : Photon.MonoBehaviour {

    //------初期設定系------
    public float floorDefaultPositionX = -1.28f;
    public float floorDefaultPositionY = -3.86f;
    public float addCubePositionX = -1.05f;
    public float addCubePositionY = -1.05f;
    public float moveSpeed = 0.16f;
    public int backNumber = 5;
    private int limitTime = 60;

    //変速設定系
    public float[] bombSpeedPetern = new float[10];
    public float[] moveFloorSpeedPetern = new float[10];
    public bool isMaster = false;

    //＋:段差
    //-1:ボム
    //-2:動く床
    //-3:岩
    //-4:壁
    //-5:跳ねるボム
    //-6:ランダム速度の動く床
    //public int[] floorData = new int[] { 1, 1, 1, 1, -4, 1, 2, -5, 2, 1, 1, -3, 1, -3, 1, -1, 1, 2, -5, 2, 3, -1, 3, 2, -2, -2, -2, -2, -2, -2, 2,
        //-4, 2, 1, 1, -1, 1, 2, 3, -5, 3, 4, 5, 1, 2, -1, 2, 3, 1, 2, -1, 2, 3, 3, -2, -2, -2, 3, 4, 5, 2, 2, 3, -3, 3, 3, -5, 3, 4, 4};
    public int[] floorData = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    //GameObject系
    public GameObject chara;
    public SpriteRenderer sr;
    private GameObject floorPrefab;
    private GameObject bombPrefab;
    private GameObject jumpBombPrefab;
    private GameObject moveFloorPrefab;
    private GameObject rockPrefab;
    private GameObject wallPrefab;
    private GameObject goalStarPrefab;
    public LayerMask groundlayer;

    //ゲームスタート系
    public GameObject canvasObject;
    public GameObject[] startObject;

    //ゲーム終了系
    public GameObject panelObject;
    //順次表示する
    public GameObject[] gameEndObject;

    //canvas系
    public GameObject[] timeObject;
    public GameObject progressObject;
    public GameObject[] hpObject;

    //ゲームスタートフラグ
    public bool startFlg = false;
    //キャラの移動回数
    private int charaMoveCount = 0;
    private Vector3 touchPos;
    //現在何番目までブロックを作ったか
    public int blockCount = 0;
    //初回ブロック作成フラグ
    private bool firstCreateFlg = false;
    //岩フラグ
    private bool isRock = false;
    private int rockNumber = 0;
    //キャラの初期Ｘ値
    private float charaDefaultPositionX;
    //時間
    private DateTime startTime;
    //残りHP
    private int hp = 3;
    //ゴール接地フラグ
    private bool goalAddFlg = false;
    private int cd;
    //ゲーム終了フラグ
    private bool isEnd = false;
    //スタートアニメーションを再生したか
    private bool isStartAnimation = false;
    //動く床の数
    private int moveFloorCount = 0;
    //動く床のオブジェクト
    private ArrayList moveFloorObjectList = new ArrayList();
    //設定値を送信したか
    private bool isSettingSend = false;

    //BattleCharaScript
    private BattleCharaScript battleCharaScript;
    //BattleEnemyScript
    private BattleEnemyScript battleEnemyScript;

    //PhotonNetworkのScript
    private NetworkPlayerScript networkPlayerScript;

    //メッセージ用オブジェクト
    private Text messageObject;

    //待機ルーム用オブジェクト
    public GameObject[] sceneChangeObject;
    public GameObject fukidashiBase;
    public GameObject scrollViewContent;
    public GameObject[] fukidashiObject;
    private GameObject messageBackground;

    //キャラ番号
    public int charaNumber;
    public GameObject charaHeadImage;

    private bool gameStartCountdownFlg = false;
    private bool isGameStartAction = false;

    //相手が死んだか
    private bool enemyDieFlg = false;
    private int enemyLastMoveCount = 0;
    //自分が死んだか
    private bool myDieFlg = false;
    private int myLastMoveCount = 0;

    //スキル設定系
    private bool skillIntervalFlg = false;
    public float skillWaitTime = 5.0f;
    public GameObject skillButtonCoverObject;
    public GameObject skillButtonCountText;
    public GameObject skillEffectFrameObject;
    public GameObject enemySkillEffectFrameObject;
    private int skillNumber = 0;
    private string skillName = "";
    private int skillType = 0;
    private int skillLevel = 1;
    private int skillCount = 1;

    //敵スキル
    private int enemySkillNumber = 0;
    private int enemySkillLevel = 0;
    private string enemySkillName = "";

    //プレイヤーオブジェクト
    private GameObject charaObject; 
    //敵オブジェクト
    private GameObject enemyObject; 

    // Use this for initialization
    void Start () {
        // プレハブを取得
        floorPrefab = (GameObject)Resources.Load("Prefab/FloorCube");
        bombPrefab = (GameObject)Resources.Load("Prefab/Bomb");
        jumpBombPrefab = (GameObject)Resources.Load("Prefab/JumpBomb");
        moveFloorPrefab = (GameObject)Resources.Load("Prefab/FloorMove");
        rockPrefab = (GameObject)Resources.Load("Prefab/FloorRock");
        wallPrefab = (GameObject)Resources.Load("Prefab/FloorWall");
        goalStarPrefab = (GameObject)Resources.Load("Prefab/GoalStar");

        //sr = chara.GetComponent<SpriteRenderer>();
        //charaDefaultPositionX = chara.transform.localPosition.x;

        messageObject = GameObject.Find("Message").GetComponent<Text>();

        if (!firstCreateFlg) {
            firstCreateFlg = true;
            StartCoroutine(createFloor(5));
        }

        //フロアの設定値を事前に作成
        for (int i = 0; i < 10; i++) {
            bombSpeedPetern[i] = UnityEngine.Random.Range(0.1F, 0.7F);
            moveFloorSpeedPetern[i] = UnityEngine.Random.Range(0.5F, 1.4F);
        }

        charaInit();
        waitingRoomInit();
    }

    // Update is called once per frame
    private GameObject[] characters;
    void Update() {
        countdown();

        if (!startFlg) {
            if (!isStartAnimation && characters != null && characters.Length >= 2) {
                //isStartAnimation = true;
                //StartCoroutine(gameStart());

                if (!isGameStartAction) {
                    isGameStartAction = true;
                    gameStartCountdownFlg = true;
                    gsStartTime = DateTime.Now;
                }
            } else if (!isStartAnimation) {
                characters = GameObject.FindGameObjectsWithTag("Chara");
            }
        } else {
            if (skillIntervalFlg) {
                float amo = 1.0f / skillWaitTime * Time.deltaTime;
                skillButtonCoverObject.GetComponent<Image>().fillAmount -= amo;
                if (skillButtonCoverObject.GetComponent<Image>().fillAmount <= 0) {
                    skillIntervalFlg = false;
                    skillEffectFrameObject.transform.localPosition = new Vector3(410, -10, -100);
                }
            }
        }

        if (!startFlg && gameStartCountdownFlg) {
            gameStartCountdown();
        }

        if (!isEnd && enemyDieFlg) {
            checkOvertake();
        }

        if (!isEnd && myDieFlg) {
            checkEnemyOvertake();
        }
    }


    private int gsCd;
    private DateTime gsStartTime;
    private void gameStartCountdown() {
        gsCd = 10;
        int totalTime = 0;
        TimeSpan pastTime = DateTime.Now - gsStartTime;
        totalTime = pastTime.Seconds + (pastTime.Minutes * 60);
        gsCd = 10 - totalTime;

        sendMessage("ゲーム開始まで" + gsCd.ToString() + "秒");
        if (totalTime >= 10) {
            gameStartCountdownFlg = false;
            StartCoroutine(gameStartSetting());
        }
    }

    IEnumerator gameStartSetting() {
        //閉じる
        StartCoroutine(viewEnd());

        //canvasの不要なオブジェクトを削除
        GameObject tempObj = GameObject.Find("FailFrameSet");
        Destroy(tempObj);
        tempObj = GameObject.Find("FukidashiMessage");
        Destroy(tempObj);
        messageBackground = GameObject.Find("MessageBackground");
        messageBackground.transform.localScale = new Vector3(0, 0, 0);

        //カメラを移動
        Camera.main.transform.localPosition = new Vector3(0.31f, 0.78f, -100);

        //キャラクター達を移動
        characters[0].transform.localPosition = new Vector3(-1.29f, 11.56f, -1);
        characters[1].transform.localPosition = new Vector3(-1.29f, 11.56f, -1);
        characters[1].transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        GameObject cam = GameObject.Find("Main Camera");
        BattleCameraScript bcs = cam.GetComponent<BattleCameraScript>();
        bcs.cameraStartFlg = true;

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(viewStart());
        yield return new WaitForSeconds(0.5f);

        isStartAnimation = true;
        StartCoroutine(gameStart());
    }

    void LateUpdate() {
        //自分がマスターの場合は対戦相手に設定値を送信
        if (isMaster) {
            if (networkPlayerScript != null) {
                if (!isSettingSend) {
                    Debug.Log("設定値を送信します");
                    networkPlayerScript.updateSettings(bombSpeedPetern, moveFloorSpeedPetern);
                    //networkPlayerScript.bombSpeedPetern = bombSpeedPetern;
                    //networkPlayerScript.moveFloorSpeedPetern = moveFloorSpeedPetern;
                    isSettingSend = true;
                }
            } else {
                if (characters.Length > 0) {
                    sendMessage("WAITING");
                    //photon network
                    networkPlayerScript = characters[0].gameObject.GetComponent <NetworkPlayerScript>();
                }
            }
        } else {
            if (characters.Length > 0 && networkPlayerScript == null) {
                //photon network
                networkPlayerScript = characters[0].gameObject.GetComponent <NetworkPlayerScript>();
            }
        }


        if (characters.Length >= 2 && (battleCharaScript == null || battleEnemyScript == null)) {
            BattleCharaScript temps = characters[0].GetComponent<BattleCharaScript>();
            if (temps.checkIsMine()) {
                battleCharaScript = characters[0].GetComponent<BattleCharaScript>();
                battleEnemyScript = characters[1].GetComponent<BattleEnemyScript>();

                charaObject = characters[0];
                enemyObject = characters[1];
            } else {
                battleCharaScript = characters[1].GetComponent<BattleCharaScript>();
                battleEnemyScript = characters[0].GetComponent<BattleEnemyScript>();

                charaObject = characters[1];
                enemyObject = characters[0];
            }
        }
    }

/*
    void LateUpdate() {
    }
    */

    private void floorCompleteHandler() {

    }

    IEnumerator createFloor(int createCount) {
        for (int i = 0; i < createCount; i++) {
            if (blockCount < floorData.Length) {

                bool isBomb = false;
                bool isJumpBomb = false;
                bool isMoveFloor = false;
                bool isWall = false;

                //ボムの場合
                int vCount;
                if (floorData[blockCount] == -1) {
                    vCount = floorData[(blockCount-1)];
                    isBomb = true;
                    //動く床の場合
                } else if (floorData[blockCount] == -2) {
                    vCount = floorData[(blockCount-1)];
                    isMoveFloor = true;
                } else if (floorData[blockCount] == -3) {
                    vCount = floorData[(blockCount-1)];
                    rockNumber++;
                    if (isRock == false) {
                        isRock = true;
                        StartCoroutine(fallRock());
                    }
                } else if (floorData[blockCount] == -4) {
                    vCount = floorData[(blockCount-1)];
                    isWall = true;
                } else if (floorData[blockCount] == -5) {
                    vCount = floorData[(blockCount-1)];
                    isJumpBomb = true;
                } else {
                    vCount = floorData[blockCount];
                }

                float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * blockCount));

                //動く床は３つ分
                if (isMoveFloor) {
                    blockCount += 3;
                    float posY = floorDefaultPositionY + System.Math.Abs(addCubePositionY * vCount);

                    GameObject floorObject;
                    floorObject = Instantiate(moveFloorPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;

                    moveFloorObjectList.Add(floorObject);

                    MoveFloorScript mvs = floorObject.GetComponent<MoveFloorScript>();
                    mvs.updateFloorSpeed(moveFloorSpeedPetern[moveFloorCount]);
                    moveFloorCount++;
                    if (moveFloorCount >= moveFloorSpeedPetern.Length) {
                        moveFloorCount = 0;
                    }

                    iTween.MoveTo(floorObject, iTween.Hash(
                                "x", posX,
                                "y", posY,
                                "time", 0.2f,
                                "oncomplete", "floorCompleteHandler",
                                "oncompletetarget", gameObject
                                ));

                    yield return new WaitForSeconds(0.1f);
                } else {
                    blockCount++;
                    float posY = 0;

                    for (int j = 1; j <= vCount; j++) {
                        posY = floorDefaultPositionY + System.Math.Abs(addCubePositionY * j);

                        GameObject floorObject;

                        if (isBomb && j == vCount) {
                            floorObject = Instantiate(bombPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;
                        } else if (isJumpBomb && j == vCount) {
                            floorObject = Instantiate(jumpBombPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;
                        } else {
                            floorObject = Instantiate(floorPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;
                        }

                        iTween.MoveTo(floorObject, iTween.Hash(
                                    "x", posX,
                                    "y", posY,
                                    "time", 0.2f,
                                    "oncomplete", "floorCompleteHandler",
                                    "oncompletetarget", gameObject
                                    ));

                        yield return new WaitForSeconds(0.1f);

                        //最後のブロックにゴールを置く
                        if (j == vCount && blockCount == floorData.Length && !goalAddFlg) {
                            goalAddFlg = true;
                            posY = floorDefaultPositionY + System.Math.Abs(addCubePositionY * (j+1));
                            floorObject = Instantiate(goalStarPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;

                            iTween.MoveTo(floorObject, iTween.Hash(
                                        "x", posX,
                                        "y", posY,
                                        "time", 0.2f,
                                        "oncomplete", "floorCompleteHandler",
                                        "oncompletetarget", gameObject
                                        ));

                            yield return new WaitForSeconds(0.1f);
                        }
                    }

                    if (isWall) {
                        float defaultPositionY = posY + 0.4f;
                        for (int k = 1; k <= 5; k++) {
                            posY = defaultPositionY + System.Math.Abs((addCubePositionY + 0.1f) * k);
                            GameObject floorObject;
                            floorObject = Instantiate(wallPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;
                            floorObject.transform.Rotate(0, 0, 90); 

                            iTween.MoveTo(floorObject, iTween.Hash(
                                        "x", posX,
                                        "y", posY,
                                        "time", 0.2f,
                                        "oncomplete", "floorCompleteHandler",
                                        "oncompletetarget", gameObject
                                        ));

                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
            }
        }
    }

    //岩を落とす 
    IEnumerator fallRock() {
        int loopCount = 0;
        while (true) {
            for (int i = 1; i <= rockNumber; i++) {
                for (int j = 0; j < floorData.Length; j++) {
                    if (floorData[j] == -3) {
                        //落とす座標
                        float passX = charaDefaultPositionX + (addCubePositionX * -1 * j);
                        float passY = 5.5f;

                        Instantiate(rockPrefab, new Vector3(passX, passY, 0), Quaternion.identity);

                        //yield return new WaitForSeconds(UnityEngine.Random.Range(0.1F, 0.3F));
                        yield return new WaitForSeconds(bombSpeedPetern[loopCount]);
                    }

                    loopCount++;
                    if (loopCount >= bombSpeedPetern.Length) {
                        loopCount = 0;
                    }
                }
            }
        }
    }


    IEnumerator gameStart() {
        sendMessage("");
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
        startTime = DateTime.Now;
    }

    private string showNumber1 = "0";
    private string showNumber2 = "0";
    private string showNumber3 = "0";
    private void countdown() {
        cd = limitTime;
        int totalTime = 0;
        if (startFlg) {
            TimeSpan pastTime = DateTime.Now - startTime;
            totalTime = pastTime.Seconds + (pastTime.Minutes * 60);
            cd = limitTime - totalTime;
        }

        // カウントダウン機能
        String stTime = cd.ToString();
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

        SpriteRenderer timeSpriteRenderer;
        if (showNumber1 != number1) {
            showNumber1 = number1;
            timeObject[0].GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + number1);
        }

        if (showNumber2 != number2) {
            showNumber2 = number2;
            timeObject[1].GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + number2);
        }

        if (showNumber3 != number3) {
            showNumber3 = number3;
            timeObject[2].GetComponent<Image>().sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + number3);
        }

        if (totalTime >= limitTime) {
            startFlg = false;
            isEnd = true;

            networkPlayerScript.gameEnd();
            //タイムオーバーの場合は進んでいる方が勝利
            int myMVC = battleCharaScript.getMoveCount();
            int enemyMVC = battleEnemyScript.getMoveCount();
            sendMessage("タイムオーバー");
            if (myMVC <= enemyMVC) {
                StartCoroutine(gameEnd(false));
            } else {
                StartCoroutine(gameEnd(true));
            }
        }
    }

    IEnumerator gameEnd(bool winFlg=false) {
        //GameObjectを生成、生成したオブジェクトを変数に代入
        GameObject panelPrefab = (GameObject)Instantiate(panelObject); 
        panelPrefab.GetComponent<Image>().color = new Color(255f, 255f, 255f, 0);
        //Canvasの子要素として登録する 
        panelPrefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(0.5f);

        int coin = 0;
        if (winFlg) {
            coin = 10;
        } else {
            coin = 1;
        }

        //データベースに登録
        //お金アップデート
        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");
        string query = "update UserStatus set money = (money + " + coin + ") where id = 1";
        sqlDB.ExecuteNonQuery(query);

        //end object群を全部表示
        GameObject endPrefab;
        for (int i = 0; i < gameEndObject.Length; i++) {
            endPrefab = (GameObject)Instantiate(gameEndObject[i]);
            float scaleX = endPrefab.transform.localScale.x;
            float scaleY = endPrefab.transform.localScale.y;
            float scaleZ = endPrefab.transform.localScale.z;

            endPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));

            //Canvasの子要素として登録する 
            endPrefab.transform.SetParent (canvasObject.transform, false);

            if (endPrefab.GetComponent<Text>() != null) {
                if (winFlg) {
                    endPrefab.GetComponent<Text>().text = "勝利";
                } else {
                    endPrefab.GetComponent<Text>().text = "敗北";
                }
            }

            string prefabName = endPrefab.name;
            prefabName = prefabName.Replace("(Clone)", "");
            if (prefabName == "BattleResultDialog") {
                //coin設定
                GameObject coinChild1 = endPrefab.transform.FindChild("coin_number_1").gameObject;
                GameObject coinChild2 = endPrefab.transform.FindChild("coin_number_2").gameObject;

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
            }

            iTween.ScaleTo(endPrefab, iTween.Hash("x", scaleX, "y", scaleY, "z", scaleZ, "time", 0.3f));
            yield return new WaitForSeconds(0.5f);
        }

        networkPlayerScript.gameEnd();
    }

    public void goal() {
        if (startFlg) {
            startFlg = false;
            networkPlayerScript.goalFlg = true;

            StartCoroutine(gameEnd(true));
        }
    }

    public void win() {
        if (!isEnd) {
            startFlg = false;
            isEnd = true;
            StartCoroutine(gameEnd(true));
        }
    }

    public void connectError() {
        if (!isEnd && cd >= 5) {
            sendMessage("相手が逃げました。");
            startFlg = false;
            isEnd = true;
            StartCoroutine(gameEnd(true));
        }
    }

    public void lose() {
        sendMessage("相手がゴールしました。");
        messageBackground.transform.localScale = new Vector3(1, 1, 1);
        messageBackground.transform.SetAsLastSibling();

        if (!isEnd) {
            startFlg = false;
            isEnd = true;
            StartCoroutine(gameEnd(false));
        }
    }

    public void charaSetting(GameObject ch) {
        chara = ch;
        sr = chara.GetComponent<SpriteRenderer>();
        charaDefaultPositionX = chara.transform.localPosition.x;
    }

    public void instructionCreateFloor(int num) {
        StartCoroutine(createFloor(num));
    }

    public void instructionGameFailed() {
        networkPlayerScript.isGameEnd = true;
        int myMVC = battleCharaScript.getMoveCount();
        int enemyMVC = battleEnemyScript.getMoveCount();
        Debug.Log("MY MVC:" + myMVC);
        Debug.Log("ENEMY MVC:" + enemyMVC);
        if (myMVC <= enemyMVC) {
            startFlg = false;
            sendMessage("HPが0になりました。");
            messageBackground.transform.localScale = new Vector3(1, 1, 1);
            messageBackground.transform.SetAsLastSibling();
            if (!isEnd) {
                isEnd = true;
                StartCoroutine(gameEnd());
            }
        } else {
            sendMessage("HPが0になりました。待機中。");
            messageBackground.transform.localScale = new Vector3(1, 1, 1);
            myDieFlg = true;
            myLastMoveCount = myMVC;
        }
    }

    public void enemyDied() {
        int myMVC = battleCharaScript.getMoveCount();
        int enemyMVC = battleEnemyScript.getMoveCount();

        Debug.Log("MY MVC:" + myMVC);
        Debug.Log("ENEMY MVC:" + enemyMVC);
        if (myMVC <= enemyMVC) {
            sendMessage("相手が倒れました。追い越せば勝利。");
            messageBackground.transform.localScale = new Vector3(1, 1, 1);        
            enemyDieFlg = true;
            enemyLastMoveCount = enemyMVC;
        } else {
            sendMessage("相手が倒れました。");
            messageBackground.transform.localScale = new Vector3(1, 1, 1);
            messageBackground.transform.SetAsLastSibling();
            if (!isEnd) {
                startFlg = false;
                isEnd = true;
                StartCoroutine(gameEnd(true));
            }
        }
    }

    private void checkOvertake() {
        int myMVC = battleCharaScript.getMoveCount();
        int enemyMVC = battleEnemyScript.getMoveCount();
        if (myMVC > enemyMVC) {
            if (!isEnd) {
                startFlg = false;
                isEnd = true;
                StartCoroutine(gameEnd(true));
            }
        }
    }

    private void checkEnemyOvertake() {
        int myMVC = battleCharaScript.getMoveCount();
        int enemyMVC = battleEnemyScript.getMoveCount();
        if (myMVC <= enemyMVC) {
            if (!isEnd) {
                startFlg = false;
                isEnd = true;
                StartCoroutine(gameEnd(false));
            }
        }
    }

    public GameObject getMoveFloorObject(int mv) {
        //mvが何番目のフロアか調べる
        int floorNumber = 0;
        for (int i = 0; i <= mv; i++) {
            if (floorData[i] == -2) {
                floorNumber++;
                i += 2;
            }
        }

        return (GameObject) moveFloorObjectList[(floorNumber-1)];
    }

    public void sendMessage(string mes) {
        if (messageObject != null) {
            messageObject.text = mes;
            if (messageBackground != null && messageObject.text != "") {
                messageBackground.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void waitingRoomInit() {
        StartCoroutine(viewStart());
        //吹き出しリストを作成
        /*
        string[] fukidashiStringList = new string[] {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };
        */

        string[] fukidashiStringList = new string[] {
            "よろしくね", "がんばろう", "おはよう", "こんにちは", "こんばんは"
        };

        GameObject temporaryObject;
        foreach (string n in fukidashiStringList) {
            temporaryObject = GameObject.Instantiate(fukidashiBase) as GameObject;
            temporaryObject.transform.SetParent (scrollViewContent.transform, false);
            GameObject childObject = temporaryObject.transform.FindChild("Text").gameObject;
            if (childObject != null) {
                childObject.GetComponent<Text>().text = n;
            }
        }
    }

    //自分が送信したメッセージ
    public void showFukidashiMessage(string s) {
        GameObject temporaryObject = GameObject.Instantiate(fukidashiObject[0]) as GameObject;
        temporaryObject.transform.SetParent (canvasObject.transform, false);
        GameObject childObject = temporaryObject.transform.FindChild("Text").gameObject;
        if (childObject != null) {
            childObject.GetComponent<Text>().text = s;
        }

        if (networkPlayerScript != null) {
            networkPlayerScript.messageString = s;
        } else {
        }
    }

    //敵が送信したメッセージ
    public void sendFukidashiMessage(string s) {
        GameObject temporaryObject = GameObject.Instantiate(fukidashiObject[1]) as GameObject;
        temporaryObject.transform.SetParent (canvasObject.transform, false);
        GameObject childObject = temporaryObject.transform.FindChild("Text").gameObject;
        if (childObject != null) {
            childObject.GetComponent<Text>().text = s;
        }
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

    IEnumerator viewEnd() {
        sceneChangeObject[0].transform.SetAsLastSibling();
        sceneChangeObject[1].transform.SetAsLastSibling();

        iTween.MoveTo(sceneChangeObject[0], iTween.Hash(
                    "position", new Vector3(3, 159, -500),
                    "time", 2.0f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));
        iTween.MoveTo(sceneChangeObject[1], iTween.Hash(
                    "position", new Vector3(3, -159, -500),
                    "time", 2.0f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(2.1f);
    }

    private void charaInit() {
        SqliteDatabase sqlDB = new SqliteDatabase("UserStatus.db");

        //キャラクター系設定
        string selectQuery = "select * from Character where get_flg = 1";
        DataTable characterTable = sqlDB.ExecuteQuery(selectQuery);
        //現在選択中のキャラ
        for (int i = 0; i < characterTable.Rows.Count; i++) {
            if ((int)characterTable.Rows[i]["select_flg"] == 1) {
                charaNumber = (int) characterTable.Rows[i]["id"];
                //スキル設定
                skillNumber = (int) characterTable.Rows[i]["skill_number"];
                skillName = (string) characterTable.Rows[i]["skill_name"];
                skillType = (int) characterTable.Rows[i]["skill_type"];
                skillLevel = (int) characterTable.Rows[i]["get_count"];

                addSkillCount();

                break;
            }
        }

        //キャラ画像設定
        charaHeadImage.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + charaNumber + "/head");
    }

    public void sceneTop() {
        StartCoroutine(viewEnd());
        Application.LoadLevel("TopScene");
    }

    public void sceneRetry() {
        StartCoroutine(viewEnd());
        Application.LoadLevel("RaceBattleScene");
    }

    public void actionSkill() {
        if (!skillIntervalFlg) {
            if (skillCheck()) {
                networkPlayerScript.skillNumber = skillNumber;
                networkPlayerScript.skillLevel = skillLevel;
                networkPlayerScript.skillName = skillName;

                skillIntervalFlg = true;
                skillButtonCoverObject.GetComponent<Image>().fillAmount = 1.0f;
                StartCoroutine(showSkill());
            }
        } else {
            errorMessage("まだ使用できません");
        }
    }

    public void enemyUseSkill(int skn, int skl, string skName) {
        enemySkillNumber = skn;
        enemySkillLevel = skl;
        enemySkillName = skName;

        StartCoroutine(showEnemySkill());
    }

    private bool skillCheck() {
        if (skillCount == 0) {
            errorMessage("スキル残数がありません");
            return false;
        }

        if (skillNumber == 1) {
            if (battleCharaScript.hp >= 3) {
                errorMessage("HPが最大です");
                return false;
            } else {
                skillCount--;
                changeSkillCountText(skillCount);
                return true;
            }
        } else if (skillNumber == 2) {
                skillCount--;
                changeSkillCountText(skillCount);
                return true;
        } else if (skillNumber == 3) {
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

    IEnumerator showEnemySkill() {
        GameObject enemySkillCharaImage = enemySkillEffectFrameObject.transform.FindChild("SkillChara").gameObject;
        GameObject enemySkillNameObj = enemySkillEffectFrameObject.transform.FindChild("SkillText").gameObject;
        int enemyCharaNumber = battleEnemyScript.enemyCharaNumber;
        //スキルキャラ画像の差し替え
        enemySkillCharaImage.GetComponent<Image>().sprite = Resources.Load <Sprite> ("Image/Character/Chara" + enemyCharaNumber.ToString() + "/head");
        //スキル名差し替え
        enemySkillNameObj.GetComponent<Text>().text = enemySkillName;

        iTween.MoveTo(enemySkillEffectFrameObject, iTween.Hash(
                    "position", new Vector3(0, -10, -100),
                    "time", 0.5f, 
                    "islocal", true
                    ));
        yield return new WaitForSeconds(0.8f);

        iTween.MoveTo(enemySkillEffectFrameObject, iTween.Hash(
                    "position", new Vector3(-420, -10, -100),
                    "time", 0.5f, 
                    "islocal", true
                    ));

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(enemyEffectSkill());
    }

    IEnumerator effectSkill() {
        //回復スキル
        if (skillNumber == 1) {
            GameObject skEffectPrefab = (GameObject)Resources.Load("Effect/Heal");
            GameObject skEffectObj = GameObject.Instantiate(skEffectPrefab) as GameObject;
            skEffectObj.transform.SetParent (charaObject.transform, false);
            skEffectObj.transform.localPosition = new Vector3(
                skEffectObj.transform.localPosition.x,
                (skEffectObj.transform.localPosition.y - 200.0f),
                skEffectObj.transform.localPosition.z
                );

            battleCharaScript.recoveryHP();

            yield return new WaitForSeconds(0.5f);
        } else if (skillNumber == 3) {
            GameObject wind = (GameObject)Resources.Load("Effect/Wind");
            GameObject skEffectObj = GameObject.Instantiate(wind) as GameObject;
            skEffectObj.transform.SetParent (enemyObject.transform, false);

            battleEnemyScript.isMove = true;

            skEffectObj.transform.localPosition = new Vector3(
                enemyObject.transform.localPosition.x,
                (enemyObject.transform.localPosition.y - 200.0f),
                enemyObject.transform.localPosition.z
                );

            yield return new WaitForSeconds(1.0f);
            int backCount = 3 + (1 * skillLevel);

            battleEnemyScript.backSkill(backCount);
        }

        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator enemyEffectSkill() {
        //回復スキル
        if (enemySkillNumber == 1) {
            GameObject skEffectPrefab = (GameObject)Resources.Load("Effect/Heal");
            GameObject skEffectObj = GameObject.Instantiate(skEffectPrefab) as GameObject;
            skEffectObj.transform.SetParent (enemyObject.transform, false);
            skEffectObj.transform.localPosition = new Vector3(
                skEffectObj.transform.localPosition.x,
                (skEffectObj.transform.localPosition.y - 200.0f),
                skEffectObj.transform.localPosition.z
                );

            yield return new WaitForSeconds(0.5f);
        //相手の画面にお邪魔を追加
        } else if (enemySkillNumber == 2) {
            GameObject ojama = (GameObject)Resources.Load("Prefab/Canvas/Skill/Ojama");
            float destroyTime = 5.0f + (1.0f * enemySkillLevel);
            for (int i = 0; i < 6; i++) {
                float randomX = UnityEngine.Random.Range(-132.0f, 121.0f);
                float randomY = UnityEngine.Random.Range(-212.0f, 50.0f);

                GameObject ojamaObject = GameObject.Instantiate(ojama) as GameObject;
                ojamaObject.transform.SetParent (canvasObject.transform, false);
                ojamaObject.transform.localScale = new Vector3(0, 0, 0);
                ojamaObject.transform.localPosition = new Vector3(randomX, randomY, -500);

                iTween.ScaleTo(ojamaObject, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
                Destroy(ojamaObject, destroyTime);
                yield return new WaitForSeconds(0.1f);
            }
        //相手を戻すスキル
        } else if (enemySkillNumber == 3) {
            GameObject wind = (GameObject)Resources.Load("Effect/Wind");
            GameObject skEffectObj = GameObject.Instantiate(wind) as GameObject;
            skEffectObj.transform.SetParent (charaObject.transform, false);

            battleCharaScript.isMove = true;

            skEffectObj.transform.localPosition = new Vector3(
                charaObject.transform.localPosition.x,
                (charaObject.transform.localPosition.y - 200.0f),
                charaObject.transform.localPosition.z
                );

            yield return new WaitForSeconds(1.0f);
            int backCount = 3 + (1 * enemySkillLevel);

            battleCharaScript.backSkill(backCount);
        }

        enemySkillEffectFrameObject.transform.localPosition = new Vector3(410, -10, -100);
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
}
