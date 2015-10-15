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
    public int[] floorData = new int[] { 1, 1, 1, 1, -4, 1, 2, -5, 2, 1, 1, -3, 1, -3, 1, -1, 1, 2, -5, 2, 3, -1, 3, 2, -2, -2, -2, -2, -2, -2, 2,
        -4, 2, 1, 1, -1, 1, 2, 3, -5, 3, 4, 5, 1, 2, -1, 2, 3, 1, 2, -1, 2, 3, 3, -2, -2, -2, 3, 4, 5, 2, 2, 3, -3, 3, 3, -5, 3, 4, 4};
    //private int[] floorData = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
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
    public GameObject[] winObject;
    public GameObject[] failObject;

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
    }

    // Update is called once per frame
    private GameObject[] characters;
    void Update() {
        countdown();

        if (!startFlg) {
            if (!isStartAnimation && characters != null && characters.Length >= 2) {
                isStartAnimation = true;

                StartCoroutine(gameStart());
            } else if (!isStartAnimation) {
                characters = GameObject.FindGameObjectsWithTag("Chara");
            }
        }
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

                    battleCharaScript = characters[0].GetComponent<BattleCharaScript>();
                    battleEnemyScript = characters[0].GetComponent<BattleEnemyScript>();

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
            timeSpriteRenderer = timeObject[0].GetComponent<SpriteRenderer>();
            timeSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + number1);
        }

        if (showNumber2 != number2) {
            showNumber2 = number2;
            timeSpriteRenderer = timeObject[1].GetComponent<SpriteRenderer>();
            timeSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + number2);
        }

        if (showNumber3 != number3) {
            showNumber3 = number3;
            timeSpriteRenderer = timeObject[2].GetComponent<SpriteRenderer>();
            timeSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number_" + number3);
        }

        if (totalTime >= limitTime) {
            startFlg = false;
            isEnd = true;

            networkPlayerScript.gameEnd();

            //タイムオーバーの場合は進んでいる方が勝利
            int myMVC = battleCharaScript.getMoveCount();
            int enemyMVC = battleCharaScript.getMoveCount();
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

        GameObject[] endObject;
        //勝利した場合
        if (winFlg) {
            endObject = winObject;
        } else {
            endObject = failObject;
        }

        //end object群を全部表示
        GameObject endPrefab;
        for (int i = 0; i < endObject.Length; i++) {
            endPrefab = (GameObject)Instantiate(endObject[i]);
            float scaleX = endPrefab.transform.localScale.x;
            float scaleY = endPrefab.transform.localScale.y;
            float scaleZ = endPrefab.transform.localScale.z;

            endPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));

            //Canvasの子要素として登録する 
            endPrefab.transform.SetParent (canvasObject.transform, false);
            // 4秒かけて、y軸を3倍に拡大
            iTween.ScaleTo(endPrefab, iTween.Hash("x", scaleX, "y", scaleY, "z", scaleZ, "time", 0.3f));

            yield return new WaitForSeconds(0.5f);
        }

        networkPlayerScript.gameEnd();
    }

    public void goal() {
        if (startFlg) {
            startFlg = false;
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
        startFlg = false;
        isEnd = true;
        StartCoroutine(gameEnd());
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

    private void sendMessage(string mes) {
        if (messageObject != null) {
            messageObject.text = mes;
        }
    }
}
