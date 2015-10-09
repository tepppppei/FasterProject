using UnityEngine;
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
    private GameObject[] hpObject;

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
    private bool isMove = false;
    private Vector3 touchPos;
    //接地フラグ
    private bool isGrounded = true;
    //戻りフラグ
    private bool isBack = false;
    //キャラの初期Ｘ値
    private float charaDefaultPositionX;
    //残りHP
    private int hp = 3;

	//GameStartObjectのScript
	BattleGameStartScript gameStartScript;

	// Use this for initialization
	void Start () {
		GameObject obj = GameObject.Find("GameStartObj");
		gameStartScript = obj.GetComponent<BattleGameStartScript>();
		updateDefaultSettings();

        sr = this.gameObject.GetComponent<SpriteRenderer>();
        charaDefaultPositionX = this.gameObject.transform.localPosition.x;

       	progressObject = GameObject.Find("cara_sprite_0");
       	hpObject = GameObject.FindGameObjectsWithTag("HpHeart");
	}

	void Update() {
        if (photonView.isMine) {
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
	                    this.gameObject.transform.localPosition, this.gameObject.transform.localPosition - this.gameObject.transform.up * 1.2f, groundlayer);

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

    void actionMove() {
        isMove = true;

        float pass1x = this.gameObject.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = this.gameObject.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = this.gameObject.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        this.gameObject.GetComponent<Animation>().Play("Move");

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

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }
    }

    void actionJump() {
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

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

    }

    void actionDown() {
        isMove = true;

        float pass1x = this.gameObject.transform.localPosition.x + (addCubePositionX / -2);
        float pass1y = this.gameObject.transform.localPosition.y + (addCubePositionY * -0.8f);
        float pass2x = this.gameObject.transform.localPosition.x + (addCubePositionX * -1);
        float pass2y = this.gameObject.transform.localPosition.y;

        Vector3[] path = new Vector3[2];
        path[0] = new Vector3(pass1x, pass1y, 0);
        path[1] = new Vector3(pass2x, pass2y, 0);

        this.gameObject.GetComponent<Animation>().Play("Move");
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

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }

    }

    void actionSliding() {
        isMove = true;

        int step = 2;
        float posX = this.gameObject.transform.localPosition.x + (addCubePositionX * step * -1);

        this.gameObject.GetComponent<Animation>().Play("Sliding");
        iTween.MoveTo(this.gameObject, iTween.Hash(
                    "position", new Vector3(posX, this.gameObject.transform.localPosition.y, 0),
                    "time", moveSpeed, 
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        charaMoveCount += step;

        //ブロック追加
        if ((gameStartScript.blockCount - charaMoveCount) <= 4) {
            step = 5 - (gameStartScript.blockCount - charaMoveCount);
            gameStartScript.instructionCreateFloor(step);
        }
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
        iTween.Stop(gameObject);
        Debug.Log("IS MOVEをFALSEにする");
        isMove = false;
    }

    void ValueChange(float value){
        this.gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, value));
    }

    private void CompleteHandler() {
        this.gameObject.GetComponent<Animation>().Play("Idle");
        isMove = false;
    }

	void OnCollisionEnter2D(Collision2D collision){
		if(collision.gameObject.tag == "Bomb"){
			goBack();
		} else if(collision.gameObject.tag == "Fall"){
			goBack();
		} else if(collision.gameObject.tag == "MoveFloor"){
			Debug.Log("動く床にぶつかった");
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

	void OnCollisionStay2D(Collision2D collision){
		if(collision.gameObject.tag == "MoveFloor"){
			float posX = charaX + (collision.gameObject.transform.localPosition.x - moveFloorX);
			this.gameObject.transform.localPosition = new Vector3(posX, this.gameObject.transform.localPosition.y, 0);
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
        isMove = false;
    }

    public void correctCharaPositionX() {
        float passX = charaDefaultPositionX + (addCubePositionX * -1 * charaMoveCount);
        float passY = this.gameObject.transform.localPosition.y;

        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        this.gameObject.transform.localPosition = new Vector3(passX, passY, 0);
        this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
    }

    private void goBackProgress() {
        hp--;

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

        //HPをフェードアウト
        if (hp >= 0) {
            iTween.FadeTo(hpObject[hp],iTween.Hash ("a", 0, "time", 1.0f));
            Destroy(hpObject[hp], 1.0f);
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

}
