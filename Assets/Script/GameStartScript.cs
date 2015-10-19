using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class GameStartScript : MonoBehaviour {

    //------初期設定系------
    private float floorDefaultPositionX = -1.28f;
    private float floorDefaultPositionY = -3.86f;
    private float addCubePositionX = -1.05f;
    private float addCubePositionY = -1.05f;
    private float moveSpeed = 0.16f;
    private int backNumber = 5;
    private int limitTime = 60;

    //＋:段差
    //-1:ボム
    //-2:動く床
    //-3:岩
    //-4:壁
    //-5:跳ねるボム
    //-6:ランダム速度の動く床
    private int[] floorData = new int[] { 1, 1, 1, 1, -4, 1, 2, -5, 2, 1, 1, -3, 1, -3, 1, -1, 1, 2, -5, 2, 3, -1, 3, 2, -2, -2, -2, -2, -2, -2, 2,
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
    private bool startFlg = false;
    //キャラの移動回数
    private int charaMoveCount = 0;
    //入力受付
    private bool isMove = false;
    private Vector3 touchPos;
    //現在何番目までブロックを作ったか
    private int blockCount = 0;
    //初回ブロック作成フラグ
    private bool firstCreateFlg = false;
    //接地フラグ
    private bool isGrounded = true;
    //戻りフラグ
    private bool isBack = false;
    //岩フラグ
    private bool isRock = false;
    private int rockNumber = 0;
    //MAXどこまで進んだか
    private int maxMoveCount = 0;
    //キャラの初期Ｘ値
    private float charaDefaultPositionX;
    //時間
    private DateTime startTime;
    //残りHP
    private int hp = 3;
    //ゴール接地フラグ
    private bool goalAddFlg = false;
    private int cd;

    // Use this for initialization
    void Start () {
        string difficulty = PlayerPrefs.GetString ("difficulty");
        int stageNumber = PlayerPrefs.GetInt ("stage_number");
        Debug.Log("DIFFICULTY:" + difficulty);
        Debug.Log("STAGE NUMBER:" + stageNumber);

        // プレハブを取得
        floorPrefab = (GameObject)Resources.Load("Prefab/FloorCube");
        bombPrefab = (GameObject)Resources.Load("Prefab/Bomb");
        jumpBombPrefab = (GameObject)Resources.Load("Prefab/JumpBomb");
        moveFloorPrefab = (GameObject)Resources.Load("Prefab/FloorMove");
        rockPrefab = (GameObject)Resources.Load("Prefab/FloorRock");
        wallPrefab = (GameObject)Resources.Load("Prefab/FloorWall");
        goalStarPrefab = (GameObject)Resources.Load("Prefab/GoalStar");

        sr = chara.GetComponent<SpriteRenderer>();

        charaDefaultPositionX = chara.transform.localPosition.x;

        if (!firstCreateFlg) {
            firstCreateFlg = true;
            StartCoroutine(createFloor(5));
        }

        //ゲーム開始処理
        StartCoroutine(gameStart());
    }

    // Update is called once per frame
    void Update() {
        countdown();

        if (startFlg) {
            moveProgress();

            if (Input.GetMouseButtonDown(0)) {
                touchPos = Input.mousePosition;
            } else if (Input.GetMouseButtonUp(0)) {
                Vector3 releasePos = Input.mousePosition;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float swipeDistanceY = releasePos.y - touchPos.y;

                //接地判定
                isGrounded = Physics2D.Linecast(
                        chara.transform.localPosition, chara.transform.localPosition - chara.transform.up * 1.2f, groundlayer);

                Debug.Log("IS MOVE:" + isMove);
                if (!isMove && isGrounded) {
                    //タッチ判定
                    if (System.Math.Abs(swipeDistanceY) <= 35) {
                        if (checkMove(0)) {
                            touchTrueEffect(worldPos.x, worldPos.y);
                            actionMove();
                        } else {
                            touchFalseEffect(worldPos.x, worldPos.y);
                            badMove();
                        }
                        //スワイプ上判定
                    } else if (swipeDistanceY > 35) {
                        if (checkMove(1)) {
                            touchTrueEffect(worldPos.x, worldPos.y);
                            actionJump();
                        } else {
                            touchFalseEffect(worldPos.x, worldPos.y);
                            badMove();
                        }
                        //スワイプ下判定
                    } else if (swipeDistanceY < -35) {
                        if (checkMove(2)) {
                            touchTrueEffect(worldPos.x, worldPos.y);
                            if (floorData[(charaMoveCount+1)] == -4) {
                                actionSliding();
                            } else {
                                actionDown();
                            }
                        } else {
                            touchFalseEffect(worldPos.x, worldPos.y);
                            badMove();
                        }
                    } else {
                        touchFalseEffect(worldPos.x, worldPos.y);
                    }
                } else {
                    touchFalseEffect(worldPos.x, worldPos.y);
                }
            }
        }
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

    public bool checkMove(int moveType) {
        if (floorData[(charaMoveCount)] == -2) {
            return true;
        } else if (floorData[(charaMoveCount)] == -3) {
            if (moveType == 0) {
                return true;
            } else {
                return false;
            }
            //イレギュラーパターン
        } else if (floorData[(charaMoveCount + 1)] < 0) {
            if (floorData[(charaMoveCount + 1)] == -1) {
                return true;
            } else if (floorData[(charaMoveCount + 1)] == -2) {
                return true;
            } else if (floorData[(charaMoveCount + 1)] == -3) {
                if (moveType == 0) {
                    return true;
                } else {
                    return false;
                }
            } else if (floorData[(charaMoveCount + 1)] == -4) {
                if (moveType == 2) {
                    return true;
                } else {
                    return false;
                }
            } else if (floorData[(charaMoveCount + 1)] == -5) {
                return true;
            } else {
                return false;
            }
            //通常
        } else {
            //横移動チェック
            if (moveType == 0) {
                if (floorData[charaMoveCount] == floorData[(charaMoveCount + 1)]) {
                    return true;
                } else {
                    return false;
                }
                //上移動チェック
            } else if (moveType == 1) {
                if (floorData[charaMoveCount] < floorData[(charaMoveCount + 1)]) {
                    return true;
                } else {
                    return false;

                }
                //下移動チェック
            } else if (moveType == 2) {
                if (floorData[charaMoveCount] > floorData[(charaMoveCount + 1)]) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }
    }

    void actionMove() {
        isMove = true;

        float pass1x = chara.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = chara.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = chara.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = chara.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        chara.GetComponent<Animation>().Play("Move");

        iTween.MoveTo(chara, iTween.Hash(
                    "path", path,
                    "time", moveSpeed,
                    "easetype", iTween.EaseType.easeOutSine,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        int step = 1;
        //動く床の場合は3つ移動
        if (floorData[charaMoveCount] == -2) {
            charaMoveCount += 3;
            step = 0;
        } else if (floorData[(charaMoveCount + 1)] == -1 || floorData[(charaMoveCount + 1)] == -5) {
            charaMoveCount++;
            step = 0;
        } else {
            charaMoveCount++;
        }

        //ブロック追加
        if ((blockCount - charaMoveCount) <= 4) {
            step = 5 - (blockCount - charaMoveCount);
            StartCoroutine(createFloor(step));
        }

/*
        if (charaMoveCount >= maxMoveCount) {
            maxMoveCount = charaMoveCount;
        }

        if (charaMoveCount >= 1 && charaMoveCount == maxMoveCount) {
            StartCoroutine(createFloor(step));
        }
        */
    }

    void actionJump() {
        isMove = true;

        //次がボムの場合
        int step = 1;
        if (floorData[(charaMoveCount + 1)] == -1 || floorData[(charaMoveCount + 1)] == -5) {
            step = 2;
        }

        float pass1x = chara.transform.localPosition.x + ((addCubePositionX * step) / -2);
        float pass1y = chara.transform.localPosition.y + (addCubePositionY * -1.7f);
        float pass2x = chara.transform.localPosition.x + ((addCubePositionX * step) * -1);
        float pass2y = chara.transform.localPosition.y + (addCubePositionY * -1);

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        chara.GetComponent<Animation>().Play("Jump");

        iTween.MoveTo(chara, iTween.Hash(
                    "path", path,
                    "time", moveSpeed,
                    "easetype", iTween.EaseType.easeOutSine,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        //動く床の場合は3つ移動
        if (floorData[charaMoveCount] == -2) {
            charaMoveCount += 3;
        } else {
            charaMoveCount += step;
        }

        //ブロック追加
        if ((blockCount - charaMoveCount) <= 4) {
            step = 5 - (blockCount - charaMoveCount);
            StartCoroutine(createFloor(step));
        }

/*
        if (charaMoveCount >= maxMoveCount) {
            maxMoveCount = charaMoveCount;
        }

        if (charaMoveCount >= 1 && charaMoveCount == maxMoveCount) {
            StartCoroutine(createFloor(step));
        }
        */
    }

    void actionDown() {
        isMove = true;

        float pass1x = chara.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = chara.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = chara.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = chara.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        chara.GetComponent<Animation>().Play("Move");
        iTween.MoveTo(chara, iTween.Hash(
                    "path", path,
                    "time", moveSpeed,
                    "easetype", iTween.EaseType.easeOutSine,
                    "oncomplete", "CompleteHandler",
                    "oncompletetarget", gameObject
                    ));

        int step = 1;

        //動く床の場合は3つ移動
        if (floorData[charaMoveCount] == -2) {
            charaMoveCount += 3;
            step = 0;
        } else if (floorData[(charaMoveCount + 1)] == -1 || floorData[(charaMoveCount + 1)] == -5) {
            charaMoveCount++;
            step = 0;
        } else {
            charaMoveCount++;
        }

        //ブロック追加
        if ((blockCount - charaMoveCount) <= 4) {
            step = 5 - (blockCount - charaMoveCount);
            StartCoroutine(createFloor(step));
        }

/*
        if (charaMoveCount >= maxMoveCount) {
            maxMoveCount = charaMoveCount;
        }

        if (charaMoveCount >= 1 && charaMoveCount == maxMoveCount) {
            StartCoroutine(createFloor(step));
        }
        */
    }

    void actionSliding() {
        isMove = true;

        int step = 2;
        float posX = chara.transform.localPosition.x + (addCubePositionX * step * -1);

        chara.GetComponent<Animation>().Play("Sliding");
        iTween.MoveTo(chara, iTween.Hash(
                    "position", new Vector3(posX, chara.transform.localPosition.y, 0),
                    "time", moveSpeed, 
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        charaMoveCount += step;

        //ブロック追加
        if ((blockCount - charaMoveCount) <= 4) {
            step = 5 - (blockCount - charaMoveCount);
            StartCoroutine(createFloor(step));
        }
/*
        if (charaMoveCount >= maxMoveCount) {
            maxMoveCount = charaMoveCount;
        }

        if (charaMoveCount >= 1 && charaMoveCount == maxMoveCount) {
            StartCoroutine(createFloor(step));
        }
        */
    }

    void badMove() {
        isMove = true;
        iTween.ValueTo(gameObject,iTween.Hash(
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
        Debug.Log("IS MOVEをFALSEにする");
        isMove = false;
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

    //指定場所まで戻す
    public void goBack() {
        if (!isBack) {
            isBack = true;
            isMove = true;

            int vBlockCount = 0;
            bool isBreak = false;
            for (int i = backNumber; i <= (backNumber + backNumber) && isBreak == false; i++) {
                vBlockCount = floorData[(charaMoveCount - i)];
                if (vBlockCount >= 1) {
                    charaMoveCount -= i;
                    isBreak = true;
                    break;
                }
            }

            chara.GetComponent<BoxCollider2D>().isTrigger = true; 

            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (vBlockCount + 1)));

            iTween.MoveTo(chara, iTween.Hash(
                        "position", new Vector3(posX, posY, 0),
                        "time", 0.5f, 
                        "oncomplete", "goBackComplete", 
                        "oncompletetarget", gameObject, 
                        "easeType", "linear"
                        ));

            iTween.ValueTo(gameObject,iTween.Hash(
                        "from",0,
                        "to",1,
                        "time",0.2f,
                        "looptype","pingpong",
                        "onupdate","ValueChange"
                        ));

            //進捗バーも戻す
            goBackProgress();

            StartCoroutine(stopAction(1.0f));
        }
    }

    private void goBackComplete() {
        chara.GetComponent<BoxCollider2D>().isTrigger = false; 
        isBack = false;
        Debug.Log("IS MOVEをFALSEにする");
        isMove = false;
    }

    public void correctCharaPositionX() {
        float passX = charaDefaultPositionX + (addCubePositionX * -1 * charaMoveCount);
        float passY = chara.transform.localPosition.y;

        chara.GetComponent<BoxCollider2D>().isTrigger = true;
        chara.transform.localPosition = new Vector3(passX, passY, 0);
        chara.GetComponent<BoxCollider2D>().isTrigger = false;
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
        TimeSpan pastTime = DateTime.Now - startTime;
        cd = limitTime;

        int totalTime = 0;
        if (startFlg) {
            pastTime = DateTime.Now - startTime;
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
            StartCoroutine(gameFailed());
        }
    }

    private float latestProgressX = 0;
    private float defaultProgressX = 0;
    private void moveProgress() {
        int floorLength = floorData.Length;
        //何％まで進んでいるか
        float progressPercent = (charaMoveCount * 1.0f) / (floorLength * 1.0f);
        //x増加値
        float progressAddX = 286 * progressPercent;

        if (defaultProgressX == 0) {
            defaultProgressX = progressObject.transform.localPosition.x;
        }

        if (latestProgressX < progressAddX) {
            latestProgressX = progressAddX;

            float pasX = -141 + latestProgressX;
            float pasY = progressObject.transform.localPosition.y;
            float pasZ = progressObject.transform.localPosition.z;

            progressObject.transform.localPosition = new Vector3(pasX, pasY, pasZ);
        }
    }

    private void goBackProgress() {
        hp--;

        chara.GetComponent<Animation>().Play("Idle");

        int floorLength = floorData.Length;
        //何％まで進んでいるか
        float progressPercent = (charaMoveCount * 1.0f) / (floorLength * 1.0f);
        //x増加値
        float progressAddX = 286 * progressPercent;
        latestProgressX = progressAddX;

        float pasX = defaultProgressX + progressAddX;
        float pasY = progressObject.transform.localPosition.y;
        float pasZ = progressObject.transform.localPosition.z;

        iTween.MoveTo(progressObject, iTween.Hash(
                    "position", new Vector3(pasX, pasY, pasZ),
                    "time", 0.5f,
                    "islocal", true,
                    "oncomplete", "goBackComplete",
                    "oncompletetarget", gameObject
                    ));

        int rotateZ = 360 * 3;
        iTween.RotateTo(progressObject, iTween.Hash("z", rotateZ, "time", 0.5f));

        //HPをフェードアウト
        if (hp >= 0) {
            iTween.FadeTo(hpObject[hp],iTween.Hash ("a", 0, "time", 1.0f));
            Destroy(hpObject[hp], 1.0f);
        }

        if (hp == 0) {
            startFlg = false;
            StartCoroutine(gameFailed());
        }
    }

    private void goBackProgressComplete() {

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
}
