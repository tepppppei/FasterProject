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
    public int limitTime = 60;


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
    public GameObject[] endObject;
    //ゲームクリア系
    public GameObject clearDialogObject;
    public GameObject completeObject;
    public GameObject decTimeObject;
    public GameObject decHpObject;
    public GameObject[] starObject;
    public GameObject[] endButtonObject;

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

        if (!firstCreateFlg) {
            firstCreateFlg = true;
            StartCoroutine(createFloor(5));
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

/*
    void LateUpdate() {
    }
    */

    private void floorCompleteHandler() {

    }

    IEnumerator createFloor(int createCount) {
        Debug.Log("CREATE FLOOR");
        Debug.Log("create count:" + createCount);
        Debug.Log("chara move count:" + charaMoveCount);
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

                Debug.Log("------V COUNT------" + vCount);

                float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * blockCount));

                //動く床は３つ分
                if (isMoveFloor) {
                    blockCount += 3;
                    float posY = floorDefaultPositionY + System.Math.Abs(addCubePositionY * vCount);

                    GameObject floorObject;
                    floorObject = Instantiate(moveFloorPrefab, new Vector3(posX, 10, -1), Quaternion.identity) as GameObject;

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
        while (true) {
            for (int i = 1; i <= rockNumber; i++) {
                for (int j = 0; j < floorData.Length; j++) {
                    if (floorData[j] == -3) {
                        //落とす座標
                        float passX = charaDefaultPositionX + (addCubePositionX * -1 * j);
                        float passY = 5.5f;

                        Instantiate(rockPrefab, new Vector3(passX, passY, 0), Quaternion.identity);
                        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1F, 0.3F));
                    }
                }
            }

            float waitTime = UnityEngine.Random.Range(0.3F, 1.5F);

            yield return new WaitForSeconds(waitTime);
        }
    }


    IEnumerator gameStart() {
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
            cd = limitTime - pastTime.Seconds;

            totalTime = pastTime.Seconds + (pastTime.Minutes * 60);
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
            StartCoroutine(gameFailed());
        }
    }

    IEnumerator gameFailed() {
        //GameObjectを生成、生成したオブジェクトを変数に代入
        GameObject panelPrefab = (GameObject)Instantiate(panelObject); 
        panelPrefab.GetComponent<Image>().color = new Color(255f, 255f, 255f, 0);
        //Canvasの子要素として登録する 
        panelPrefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(0.5f);

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
            iTween.ScaleTo(endPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void goal() {
        if (startFlg) {
            startFlg = false;
            StartCoroutine(gameCleared());
        }
    }

    IEnumerator gameCleared() {
        int clearTime = cd;

        //GameObjectを生成、生成したオブジェクトを変数に代入
        GameObject panelPrefab = (GameObject)Instantiate(panelObject); 
        panelPrefab.GetComponent<Image>().color = new Color(255f, 255f, 255f, 0);
        //Canvasの子要素として登録する 
        panelPrefab.transform.SetParent (canvasObject.transform, false);
        yield return new WaitForSeconds(0.5f);

        //ダイアログの表示
        GameObject clearDialogPrefab = (GameObject)Instantiate(clearDialogObject); 
        float scaleX = clearDialogPrefab.transform.localScale.x;
        float scaleY = clearDialogPrefab.transform.localScale.y;
        float scaleZ = clearDialogPrefab.transform.localScale.z;
        clearDialogPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        //Canvasの子要素として登録する 
        clearDialogPrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(clearDialogPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.5f);

        //Complete
        GameObject completePrefab = (GameObject)Instantiate(completeObject); 
        scaleX = completePrefab.transform.localScale.x;
        scaleY = completePrefab.transform.localScale.y;
        scaleZ = completePrefab.transform.localScale.z;
        completePrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        //Canvasの子要素として登録する 
        completePrefab.transform.SetParent (canvasObject.transform, false);
        iTween.ScaleTo(completePrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.5f);

        //残り時間
        GameObject decTimePrefab = (GameObject)Instantiate(decTimeObject); 
        scaleX = decTimePrefab.transform.localScale.x;
        scaleY = decTimePrefab.transform.localScale.y;
        scaleZ = decTimePrefab.transform.localScale.z;
        decTimePrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        //Canvasの子要素として登録する 
        decTimePrefab.transform.SetParent (canvasObject.transform, false);
        GameObject decTimeChild1 = decTimePrefab.transform.FindChild("time_number_1").gameObject;
        GameObject decTimeChild2 = decTimePrefab.transform.FindChild("time_number_2").gameObject;
        GameObject decTimeChild3 = decTimePrefab.transform.FindChild("time_number_3").gameObject;

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

        SpriteRenderer timeSpriteRenderer;
        timeSpriteRenderer = decTimeChild1.GetComponent<SpriteRenderer>();
        timeSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + number1);
        timeSpriteRenderer = decTimeChild2.GetComponent<SpriteRenderer>();
        timeSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + number2);
        timeSpriteRenderer = decTimeChild3.GetComponent<SpriteRenderer>();
        timeSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + number3);

        iTween.ScaleTo(decTimePrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.5f);

        //残りHP
        GameObject decHpPrefab = (GameObject)Instantiate(decHpObject); 
        scaleX = decHpPrefab.transform.localScale.x;
        scaleY = decHpPrefab.transform.localScale.y;
        scaleZ = decHpPrefab.transform.localScale.z;
        decHpPrefab.transform.localScale = new Vector3((scaleX / 3), (scaleY / 3), (scaleZ / 3));
        //Canvasの子要素として登録する 
        decHpPrefab.transform.SetParent (canvasObject.transform, false);
        GameObject decHpChild = decHpPrefab.transform.FindChild("hp_number_1").gameObject;
        String stHp = hp.ToString();
        SpriteRenderer hpSpriteRenderer;
        hpSpriteRenderer = decHpChild.GetComponent<SpriteRenderer>();
        hpSpriteRenderer.sprite = Resources.Load <Sprite> ("Prefab/Number/" + "number4_red_" + stHp);
        iTween.ScaleTo(decHpPrefab, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3f));
        yield return new WaitForSeconds(0.5f);

        //star
        //星の数を計算する
        float decPer = (clearTime * 1.0f) / (limitTime * 1.0f);
        float decHpPer = (hp * 1.0f) / 2 * decPer;
        float totalPer = decPer + decHpPer;
        Debug.Log("DEC PER:" + decPer);
        Debug.Log("DEC HP PER:" + decHpPer);
        Debug.Log("TOTAL PER:" + totalPer);

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

            yield return new WaitForSeconds(0.5f);
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

            yield return new WaitForSeconds(0.5f);
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
        isEnd = true;
        StartCoroutine(gameFailed());
    }
}
