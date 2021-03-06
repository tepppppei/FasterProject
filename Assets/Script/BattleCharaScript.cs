﻿using UnityEngine;
using System.Collections;

public class BattleCharaScript : Photon.MonoBehaviour {

    //public GameObject gameStartObj;
    //------初期設定系------
    private float floorDefaultPositionX;
    private float floorDefaultPositionY;
    private float addCubePositionX;
    private float addCubePositionY;
    private float moveSpeed;
    private int backNumber;
    private int[] floorData;

    public LayerMask groundlayer;
    public SpriteRenderer sr;

    //canvas系
    private GameObject progressObject;
    private ArrayList hpObject = new ArrayList();

    //動く床用
    private float moveFloorX = 0;
    private float charaX = 0;
    //動く床フラグ
    private bool isOnMoveFloor = false;

    //スタートフラグ
    private bool startFlg = false;
    //キャラの移動回数
    private int charaMoveCount = 0;
    //入力受付
    public bool isMove = false;
    private Vector3 touchPos;
    //接地フラグ
    private bool isGrounded = true;
    //戻りフラグ
    private bool isBack = false;
    //キャラの初期Ｘ値
    private float charaDefaultPositionX;
    //残りHP
    public int hp = 3;

    //GameStartObjectのScript
    BattleGameStartScript gameStartScript;

    //PhotonNetworkのScript
    private NetworkPlayerScript networkPlayerScript;

    // Use this for initialization
    void Start () {
        GameObject obj = GameObject.Find("GameStartObj");
        gameStartScript = obj.GetComponent<BattleGameStartScript>();
        updateDefaultSettings();

        sr = this.gameObject.GetComponent<SpriteRenderer>();
        charaDefaultPositionX = this.gameObject.transform.localPosition.x;

        progressObject = GameObject.Find("ProgressChara");
        GameObject hpObject1 = GameObject.Find("item_heart1");
        GameObject hpObject2 = GameObject.Find("item_heart2");
        GameObject hpObject3 = GameObject.Find("item_heart3");
        hpObject.Add(hpObject1);
        hpObject.Add(hpObject2);
        hpObject.Add(hpObject3);

        //photon network
        networkPlayerScript = this.gameObject.GetComponent <NetworkPlayerScript>();
    }

    void Update() {
        if (photonView.isMine) {
            //scriptを削除
            if (this.gameObject.GetComponent<BattleEnemyScript>() != null) {
                BattleEnemyScript bcs = this.gameObject.GetComponent<BattleEnemyScript>();
                Destroy(bcs);
            }

            if (startFlg && !isBack && hp > 0) {
                moveProgress();
                if (Input.GetMouseButtonDown(0)) {
                    touchPos = Input.mousePosition;
                } else if (Input.GetMouseButtonUp(0)) {
                    Vector3 releasePos = Input.mousePosition;
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float swipeDistanceY = releasePos.y - touchPos.y;

                    //接地判定
                    isGrounded = Physics2D.Linecast(
                            this.gameObject.transform.localPosition, this.gameObject.transform.localPosition - this.gameObject.transform.up * 1.2f, groundlayer);

                    if (!isMove && isGrounded && worldPos.y <= 2.3f) {
                        //タッチ判定
                        if (System.Math.Abs(swipeDistanceY) <= 35) {
                            if (checkMove(0)) {
                                touchTrueEffect(worldPos.x, worldPos.y);
                                StartCoroutine(actionMove());
                            } else {
                                touchFalseEffect(worldPos.x, worldPos.y);
                                badMove();
                            }
                            //スワイプ上判定
                        } else if (swipeDistanceY > 35) {
                            if (checkMove(1)) {
                                touchTrueEffect(worldPos.x, worldPos.y);
                                StartCoroutine(actionJump());
                            } else {
                                touchFalseEffect(worldPos.x, worldPos.y);
                                badMove();
                            }
                            //スワイプ下判定
                        } else if (swipeDistanceY < -35) {
                            if (checkMove(2)) {
                                touchTrueEffect(worldPos.x, worldPos.y);
                                if (floorData[(charaMoveCount+1)] == -4) {
                                    StartCoroutine(actionSliding());
                                } else {
                                    StartCoroutine(actionDown());
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

        updateStartFlg();
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

    IEnumerator actionMove() {
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        iTween.Stop(this.gameObject);

        isMove = true;

        float pass1x = this.gameObject.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = this.gameObject.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = this.gameObject.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        this.gameObject.GetComponent<Animation>().Play("Move");

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

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        iTween.MoveTo(this.gameObject, iTween.Hash(
                    "path", path,
                    "time", moveSpeed,
                    //"easetype", iTween.EaseType.easeOutSine,
                    "easetype", iTween.EaseType.linear,
                    //"oncomplete", "CompleteHandler",
                    "oncompletetarget", this.gameObject
                    ));

        yield return new WaitForSeconds(moveSpeed);
        this.gameObject.GetComponent<Animation>().Play("Idle");
        //位置補正
        /*
        if (floorData[charaMoveCount] > 0 && floorData[charaMoveCount] <= floorData[(charaMoveCount + 1)]) {
            this.gameObject.GetComponent<Animation>().Play("Idle");
            //正しいx座標
            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            //正しいy座標
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (floorData[charaMoveCount] + 1)));
            this.gameObject.transform.localPosition = new Vector3(posX, posY, this.gameObject.transform.localPosition.z);
        }
        */

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        isMove = false;

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator actionJump() {
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        iTween.Stop(this.gameObject);

        isMove = true;

        //次がボムの場合
        int step = 1;
        if (floorData[(charaMoveCount + 1)] == -1 || floorData[(charaMoveCount + 1)] == -5) {
            step = 2;
        }

        float pass1x = this.gameObject.transform.localPosition.x + ((addCubePositionX * step) / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -1.7f);
        float pass2x = this.gameObject.transform.localPosition.x + ((addCubePositionX * step) * -1);
        float pass2y = this.gameObject.transform.localPosition.y + (addCubePositionY * -1);

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        this.gameObject.GetComponent<Animation>().Play("Jump");

        //動く床の場合は3つ移動
        if (floorData[charaMoveCount] == -2) {
            charaMoveCount += 3;
        } else {
            charaMoveCount += step;
        }

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);


        iTween.MoveTo(this.gameObject, iTween.Hash(
                    "path", path,
                    "time", moveSpeed,
                    //"easetype", iTween.EaseType.easeOutSine,
                    "easetype", iTween.EaseType.linear,
                    //"oncomplete", "CompleteHandler",
                    "oncompletetarget", this.gameObject
                    ));


        yield return new WaitForSeconds(moveSpeed);
        this.gameObject.GetComponent<Animation>().Play("Idle");
        //位置補正
        /*
        if (floorData[charaMoveCount] > 0 && floorData[charaMoveCount] <= floorData[(charaMoveCount + 1)]) {
            this.gameObject.GetComponent<Animation>().Play("Idle");
            //正しいx座標
            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            //正しいy座標
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (floorData[charaMoveCount] + 1)));
            this.gameObject.transform.localPosition = new Vector3(posX, posY, this.gameObject.transform.localPosition.z);
        }
        */

        isMove = false;

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator actionDown() {
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        iTween.Stop(this.gameObject);

        isMove = true;

        float pass1x = this.gameObject.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = this.gameObject.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = this.gameObject.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

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

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        this.gameObject.GetComponent<Animation>().Play("Move");
        iTween.MoveTo(this.gameObject, iTween.Hash(
                    "path", path,
                    "time", moveSpeed,
                    //"easetype", iTween.EaseType.easeOutSine,
                    "easetype", iTween.EaseType.linear,
                    //"oncomplete", "CompleteHandler",
                    "oncompletetarget", this.gameObject
                    ));

        yield return new WaitForSeconds(moveSpeed);
        this.gameObject.GetComponent<Animation>().Play("Idle");
        //位置補正
        /*
        if (floorData[charaMoveCount] > 0 && floorData[charaMoveCount] <= floorData[(charaMoveCount + 1)]) {
            this.gameObject.GetComponent<Animation>().Play("Idle");
            //正しいx座標
            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            //正しいy座標
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (floorData[charaMoveCount] + 1)));
            this.gameObject.transform.localPosition = new Vector3(posX, posY, this.gameObject.transform.localPosition.z);
        }
        */

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        isMove = false;

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator actionSliding() {
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        iTween.Stop(this.gameObject);

        isMove = true;

        int step = 2;
        float posX = this.gameObject.transform.localPosition.x + (addCubePositionX * step * -1);

        charaMoveCount += step;
        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

        this.gameObject.GetComponent<Animation>().Play("Sliding");
        iTween.MoveTo(this.gameObject, iTween.Hash(
                    "position", new Vector3(posX, this.gameObject.transform.localPosition.y, 0),
                    "time", moveSpeed, 
                    "easetype", iTween.EaseType.linear,
                    //"oncomplete", "CompleteHandler", 
                    "oncompletetarget", this.gameObject
                    ));

        yield return new WaitForSeconds(moveSpeed);

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;

        this.gameObject.GetComponent<Animation>().Play("Idle");
        //位置補正
        if (floorData[charaMoveCount] > 0 && floorData[charaMoveCount] <= floorData[(charaMoveCount + 1)]) {
            this.gameObject.GetComponent<Animation>().Play("Idle");
            //正しいx座標
            float positionX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            //正しいy座標
            float positionY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (floorData[charaMoveCount] + 1)));
            this.gameObject.transform.localPosition = new Vector3(positionX, positionY, this.gameObject.transform.localPosition.z);
        }

        isMove = false;

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

        yield return new WaitForSeconds(0.01f);
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
        //this.gameObject.GetComponent<SkinnedMeshRenderer>().material.tintColor = new Color(1, 1, 1, 1.0f);
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        iTween.Stop(this.gameObject);
        Debug.Log("IS MOVEをFALSEにする");
        isMove = false;
    }

    void ValueChange(float value){
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, value));
    }

    private void CompleteHandler() {
        this.gameObject.GetComponent<Animation>().Play("Idle");
        //位置補正
        if (floorData[charaMoveCount] > 0 && floorData[charaMoveCount] <= floorData[(charaMoveCount + 1)]) {
            this.gameObject.GetComponent<Animation>().Play("Idle");
            //正しいx座標
            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            //正しいy座標
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (floorData[charaMoveCount] + 1)));
            this.gameObject.transform.localPosition = new Vector3(posX, posY, this.gameObject.transform.localPosition.z);
        }

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        isMove = false;
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (photonView.isMine && !isBack) {
            if(collision.gameObject.tag == "Bomb"){
                goBack();
            } else if(collision.gameObject.tag == "Fall"){
                goBack();
            } else if(collision.gameObject.tag == "MoveFloor"){
                isOnMoveFloor = true;
                moveFloorX = collision.gameObject.transform.localPosition.x;
                charaX = this.gameObject.transform.localPosition.x;
                //this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            } else if(collision.gameObject.tag == "Floor"){
                if (!isOnMoveFloor) {
                    correctCharaPositionX();
                }
            } else if(collision.gameObject.tag == "Rock"){
                goBack();
            } else if(collision.gameObject.tag == "Goal"){
                gameStartScript.goal();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision){
        if (photonView.isMine) {
            if(collision.gameObject.tag == "MoveFloor"){
                float posX = charaX + (collision.gameObject.transform.localPosition.x - moveFloorX);
                this.gameObject.transform.localPosition = new Vector3(posX, this.gameObject.transform.localPosition.y, 0);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision){
        if(collision.gameObject.tag == "MoveFloor"){
            Debug.Log("動く床から離れた");
            isOnMoveFloor = false;
            //this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }

    private void updateStartFlg() {
        startFlg = gameStartScript.startFlg;
    }

    private void updateDefaultSettings() {
        floorDefaultPositionX = gameStartScript.floorDefaultPositionX;
        floorDefaultPositionY = gameStartScript.floorDefaultPositionY;
        addCubePositionX = gameStartScript.addCubePositionX;
        addCubePositionY = gameStartScript.addCubePositionY;
        moveSpeed = gameStartScript.moveSpeed;
        backNumber = gameStartScript.backNumber;
        floorData = gameStartScript.floorData;
    }

    //指定場所まで戻す
    public void goBack() {
        StartCoroutine(goBackAction());
    }


    IEnumerator goBackAction() {
        if (!isBack) {
            isBack = true;
            isMove = true;

            int vBlockCount = 0;
            bool isBreak = false;
            for (int i = backNumber; i <= (backNumber + backNumber) && isBreak == false; i++) {
                if ((charaMoveCount - i) >= 0) {
                    vBlockCount = floorData[(charaMoveCount - i)];
                    if (vBlockCount >= 1) {
                        charaMoveCount -= i;
                        isBreak = true;
                        break;
                    }
                }
            }

            //同期処理
            networkPlayerScript.updateActionNumber(charaMoveCount);

            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (vBlockCount + 1)));

            iTween.MoveTo(this.gameObject, iTween.Hash(
                        "position", new Vector3(posX, posY, 0),
                        "time", 0.5f,
                        //"oncomplete", "goBackComplete",
                        "oncompletetarget", this.gameObject,
                        "easeType", "linear"
                        ));

            iTween.ValueTo(gameObject,iTween.Hash(
                        "from",0,
                        "to",1,
                        "time",0.2f,
                        "looptype","pingpong",
                        "onupdate","ValueChange"
                        ));

            yield return new WaitForSeconds(0.8f);
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
            this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;

            isBack = false;
            isMove = false;
            Debug.Log("IS MOVEをFALSEにする");

            //進捗バーも戻す
            goBackProgress(false);
            StartCoroutine(stopAction(1.0f));
        }
    }

    private void goBackComplete() {
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false; 
        isBack = false;
        Debug.Log("IS MOVEをFALSEにする");
        isMove = false;
    }

    public void correctCharaPositionX() {
        /*
        float passX = charaDefaultPositionX + (addCubePositionX * -1 * charaMoveCount);
        float passY = this.gameObject.transform.localPosition.y;

        //this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        this.gameObject.transform.localPosition = new Vector3(passX, passY, 0);
        //this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        */
        //位置補正
        //if (floorData[charaMoveCount] > 0 && floorData[charaMoveCount] <= floorData[(charaMoveCount + 1)]) {
            //正しいx座標
            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            //正しいy座標
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (floorData[charaMoveCount] + 1)));
            float dec = (posX - this.gameObject.transform.localPosition.x);
            if (dec > 0.1f || dec < -0.1f) {
                this.gameObject.GetComponent<Animation>().Play("Idle");
                this.gameObject.transform.localPosition = new Vector3(posX, posY, this.gameObject.transform.localPosition.z);
            }
        //}
    }

    private void goBackProgress(bool hpFlg) {
        if (!hpFlg) {
            hp--;
            Debug.Log("残りHP:"+hp);
        }

        this.gameObject.GetComponent<Animation>().Play("Idle");

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
                    "oncompletetarget", this.gameObject
                    ));

        int rotateZ = 360 * 3;
        iTween.RotateTo(progressObject, iTween.Hash("z", rotateZ, "time", 0.5f));

        //HPをフェードアウト
        if (hp >= 0) {
            //iTween.FadeTo((GameObject)hpObject[hp],iTween.Hash ("a", 0, "time", 1.0f));
            iTween.ScaleTo((GameObject)hpObject[hp], iTween.Hash("x", 0, "y", 0, "z", 0, "time", 0.5f));
            //Destroy((GameObject)hpObject[hp], 1.0f);
        }

        if (hp == 0) {
            startFlg = false;
            gameStartScript.instructionGameFailed();
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

    private void goBackProgressComplete() {

    }

    public int getMoveCount() {
        return charaMoveCount;
    }

    public bool checkIsMine() {
        if (photonView.isMine) {
            return true;
        } else {
            return false;
        }
    }

    public void recoveryHP() {
        Debug.Log("HP RECOVERYYYYYYYYYYYYYYYYYYYY");
        Debug.Log("HP:" + hp);
        iTween.ScaleTo((GameObject)hpObject[hp], iTween.Hash("x", 29.3f, "y", 29.3f, "z", 29.3f, "time", 0.4f));
        hp++;
        Debug.Log("HP:" + hp);
    }

    public void backSkill(int backCount) {
        StartCoroutine(backSkillAction(backCount));
    }

    public void stopSkill(float effectTime) {
        StartCoroutine(stopSkillAction(effectTime));
    }

    IEnumerator stopSkillAction(float effectTime) {
        isMove = true;
        yield return new WaitForSeconds(effectTime);
        isMove = false;
    }

    IEnumerator backSkillAction(int backCount) {
        isBack = true;
        isMove = true;

        int vBlockCount = 0;
        bool isBreak = false;

        if ((charaMoveCount - backCount) < 0) {
            backCount = charaMoveCount;
        }

        int mvc = 0;
        for (int i = backCount; i <= (backCount + backCount) && isBreak == false; i++) {
            if ((charaMoveCount - i) >= 0) {
                vBlockCount = floorData[(charaMoveCount - i)];
                if (vBlockCount >= 1) {
                    mvc = i;
                    //charaMoveCount -= i;
                    isBreak = true;
                    break;
                }
            }
        }

        //networkPlayerScript.updateActionNumber(charaMoveCount);

        float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * (charaMoveCount - mvc)));
        float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (vBlockCount + 1)));

        //一旦上に飛ばす
        Debug.Log("----------------------");
        Debug.Log("A:" + this.gameObject.transform.localPosition);
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        iTween.MoveTo(this.gameObject, iTween.Hash(
            "position", new Vector3(this.gameObject.transform.localPosition.x, (this.gameObject.transform.localPosition.y + 17.0f), 0),
            "time", 0.4f,
            //"oncomplete", "goBackComplete",
            "oncompletetarget", this.gameObject,
            "easeType", "linear"
            ));

        yield return new WaitForSeconds(0.4f);

        iTween.MoveTo(this.gameObject, iTween.Hash(
            "position", new Vector3(posX, this.gameObject.transform.localPosition.y, 0),
            "time", 0.4f,
            //"oncomplete", "goBackComplete",
            "oncompletetarget", this.gameObject,
            "easeType", "linear"
            ));

        yield return new WaitForSeconds(0.4f);

        Debug.Log("B:" + this.gameObject.transform.localPosition);
        //this.gameObject.transform.localPosition = new Vector3(posX, this.gameObject.transform.localPosition.y, this.gameObject.transform.localPosition.z);
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;

        charaMoveCount -= mvc;

        Debug.Log("C:" + this.gameObject.transform.localPosition);
        Debug.Log("----------------------");

        goBackProgress(true);
        StartCoroutine(stopAction(1.0f));
    }

    public void forwardSkill(int fc) {
        StartCoroutine(forwardSkillAction(fc));
    }

    IEnumerator forwardSkillAction(int fc) {
        isMove = true;

        int vBlockCount = 0;
        bool isBreak = false;

        if ((charaMoveCount + fc) >= floorData.Length) {
            fc = (floorData.Length - 1) - charaMoveCount;
        }

        int mvc = 0;
        for (int i = fc; i >= 1 && isBreak == false; i--) {
            if ((charaMoveCount + i) >= 0) {
                vBlockCount = floorData[(charaMoveCount + i)];
                if (vBlockCount >= 1) {
                    mvc = i;
                    isBreak = true;
                    break;
                }
            }
        }

        //ブロック追加
        if ((gameStartScript.blockCount - (charaMoveCount + fc)) <= 4) {
            int step = 5 - (gameStartScript.blockCount - (charaMoveCount + fc));
            gameStartScript.instructionCreateFloor(step);
        }

        float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * (charaMoveCount + mvc)));
        float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (vBlockCount + 1)));

        //一旦上に飛ばす
        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        iTween.MoveTo(this.gameObject, iTween.Hash(
            "position", new Vector3(this.gameObject.transform.localPosition.x, (this.gameObject.transform.localPosition.y + 17.0f), 0),
            "time", 0.4f,
            "oncompletetarget", this.gameObject,
            "easeType", "linear"
            ));

        yield return new WaitForSeconds(0.4f);

        iTween.MoveTo(this.gameObject, iTween.Hash(
            "position", new Vector3(posX, this.gameObject.transform.localPosition.y, 0),
            "time", 0.4f,
            "oncompletetarget", this.gameObject,
            "easeType", "linear"
            ));

        yield return new WaitForSeconds(0.4f);

        this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;

        charaMoveCount += mvc;

        goBackProgress(true);
        StartCoroutine(stopAction(1.0f));
    }
}
