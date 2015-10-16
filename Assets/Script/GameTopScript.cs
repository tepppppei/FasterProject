using UnityEngine;
using System.Collections;

public class GameTopScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//トップボタン一覧
	public GameObject[] topButtonList;

	public GameObject stageSelectPanel;
	public GameObject stageBase;
	public GameObject scrollViewContent;

	public void sceneStageSelect() {
		//ステージ選択パネルを表示する
		StartCoroutine(showStageSelect());
		StartCoroutine(showStageList());
	}

	//ステージ選択画面表示用
    IEnumerator showStageSelect() {
        iTween.MoveTo(stageSelectPanel, iTween.Hash(
                    "position", new Vector3(0, 0, 0),
                    "time", 0.2f, 
                    "islocal", true,
                    "oncomplete", "CompleteHandler", 
                    "oncompletetarget", gameObject
                    ));

        yield return new WaitForSeconds(1.0f);
    }

	//ステージ一覧表示
	private GameObject temporaryObject;
    IEnumerator showStageList() {
        yield return new WaitForSeconds(0.4f);
    	for(int i = 0; i < 15; i++) {
    		temporaryObject = GameObject.Instantiate(stageBase) as GameObject;
            temporaryObject.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        	//Canvasの子要素として登録する 
        	temporaryObject.transform.SetParent (scrollViewContent.transform, false);

        	iTween.ValueTo(gameObject, iTween.Hash(
        		"from", 0,
        		"to", 1.0f,
        		"time", 0.1f,
        		"onupdate", "fadeInObject"  // 毎フレーム SetAlpha() を呼びます。
            ));

        	yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.2f);
    }

    //ステージボタンを大きくする
    private void fadeInObject(float value) {
    	temporaryObject.GetComponent<RectTransform>().localScale = new Vector3 (value, value, value);
    }

    private void CompleteHandler() {
    }

}
