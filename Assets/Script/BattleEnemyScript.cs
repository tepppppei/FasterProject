﻿using UnityEngine;
using System.Collections;

public class BattleEnemyScript : Photon.MonoBehaviour {

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

    //動く床用
    private float moveFloorX = 0;
    private float charaX = 0;
    //動く床フラグ
    private bool isOnMoveFloor = false;

    //スタートフラグ
    private bool startFlg = false;
    //キャラの移動回数
    private int charaMoveCount = 0;
    //接地フラグ
    private bool isGrounded = true;
    //戻りフラグ
    private bool isBack = false;
    //キャラの初期Ｘ値
    private float charaDefaultPositionX;

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

        progressObject = GameObject.Find("cara_sprite_0");

        //photon network
        networkPlayerScript = this.gameObject.GetComponent <NetworkPlayerScript>();
    }

    void Update() {
        if (!photonView.isMine) {
            if (startFlg) {
                //moveProgress();
            }
        }

        updateStartFlg();
    }

    void actionMove() {
        float pass1x = this.gameObject.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = this.gameObject.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = this.gameObject.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        this.gameObject.GetComponent<Animation>().Play("Move");

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        iTween.MoveTo(this.gameObject, iTween.Hash(
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

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }
    }

    void actionJump() {
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

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        iTween.MoveTo(this.gameObject, iTween.Hash(
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

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

    }

    void actionDown() {
        float pass1x = this.gameObject.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = this.gameObject.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = this.gameObject.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        this.gameObject.GetComponent<Animation>().Play("Move");
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        iTween.MoveTo(this.gameObject, iTween.Hash(
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

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

    }

    void actionSliding() {
        int step = 2;
        float posX = this.gameObject.transform.localPosition.x + (addCubePositionX * step * -1);

        this.gameObject.GetComponent<Animation>().Play("Sliding");
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        iTween.MoveTo(this.gameObject, iTween.Hash(
                    "position", new Vector3(posX, this.gameObject.transform.localPosition.y, 0),
                    "time", moveSpeed, 
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        charaMoveCount += step;

        //同期処理
        networkPlayerScript.updateActionNumber(charaMoveCount);

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }
    }

    void badMove() {
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
        iTween.Stop(gameObject);
        Debug.Log("IS MOVEをFALSEにする");
    }

    void ValueChange(float value){
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, value));
    }

    private void CompleteHandler() {
        this.gameObject.GetComponent<Animation>().Play("Idle");
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (!photonView.isMine) {
            if(collision.gameObject.tag == "MoveFloor"){
                Debug.Log("動く床にぶつかった");
                isOnMoveFloor = true;
                moveFloorX = collision.gameObject.transform.localPosition.x;
                charaX = this.gameObject.transform.localPosition.x;
                //this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            } else if(collision.gameObject.tag == "Floor"){
                if (!isOnMoveFloor) {
                    correctCharaPositionX();
                }
            } else if(collision.gameObject.tag == "Goal"){
                gameStartScript.goal();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision){
        if (!photonView.isMine) {
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
    public void goBack(int bkn=0) {
        if (!isBack) {
            isBack = true;

            if (bkn > 0) {
                backNumber = bkn;
            }

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

            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;

            float posX = floorDefaultPositionX + (System.Math.Abs(addCubePositionX * charaMoveCount));
            float posY = floorDefaultPositionY + (System.Math.Abs(addCubePositionY * (vBlockCount + 1)));

            iTween.MoveTo(this.gameObject, iTween.Hash(
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
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false; 
        isBack = false;
        Debug.Log("IS MOVEをFALSEにする");
    }

    public void correctCharaPositionX() {
        float passX = charaDefaultPositionX + (addCubePositionX * -1 * charaMoveCount);
        float passY = this.gameObject.transform.localPosition.y;

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        this.gameObject.transform.localPosition = new Vector3(passX, passY, 0);
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
    }

    private void goBackProgress() {
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
                    "oncompletetarget", gameObject
                    ));

        int rotateZ = 360 * 3;
        iTween.RotateTo(progressObject, iTween.Hash("z", rotateZ, "time", 0.5f));
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

    public void enemyAction(int actionNumber) {
        Debug.Log("ACTION NUMBER:"+actionNumber);

        int loopCount = 0;
        while (charaMoveCount != actionNumber) {
            if (charaMoveCount <= actionNumber) {
                int nowFloor = floorData[(charaMoveCount)];
                int nextFloor = floorData[(charaMoveCount+1)];
                //通常移動
                if (nextFloor >= 1) {
                    if (nowFloor == nextFloor) {
                        actionMove();
                    } else if (nowFloor < nextFloor) {
                        actionJump();
                    } else if (nowFloor > nextFloor) {
                        actionDown();
                    }
                //その他移動
                } else {
                    if (nextFloor == -1) {
                        actionJump();
                    } else if (nextFloor == -2) {
                        //床に乗る処理

                        actionJump();
                    } else if (nextFloor == -3) {
                        actionMove();
                    } else if (nextFloor == -4) {
                        actionSliding();
                    } else if (nextFloor == -5) {
                        actionJump();
                    }
                }
            } else {
                goBack((charaMoveCount - actionNumber));
            }

            StartCoroutine(waitAction());

            loopCount++;
            if (loopCount >= 10) {
                charaMoveCount = actionNumber;
                break;
            }
        }
    }

    IEnumerator waitAction() {
        yield return new WaitForSeconds(0.5f);
    }
}
