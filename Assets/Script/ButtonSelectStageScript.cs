using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonSelectStageScript : MonoBehaviour {

	GameTopScript gameTopScript;

	// Use this for initialization
	void Start () {
        GameObject obj = GameObject.Find("GameTopManager");
        gameTopScript = obj.GetComponent<GameTopScript>();
	}
	
	// Update is called once per frame
	void Update () {


	}

	public void selectStage() {
		//ステージ番号を取得
		GameObject childObject = this.gameObject.transform.FindChild("Text").gameObject;
		int stageNumber = 0;
		if (childObject != null) {
			stageNumber = int.Parse(childObject.GetComponent<Text>().text);
		}

		gameTopScript.stageSelect(stageNumber);
	}
}
