using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameButtonScript : MonoBehaviour {

	private GameObject gameStartObj;
	private GameStartScript gameStartScript;
	private BattleGameStartScript battleGameStartScript;

	private GameObject gameTopObj;
	private GameTopScript gameTopScript;

	// Use this for initialization
	void Start () {
		gameStartObj = GameObject.Find("GameStartObj");
		if (gameStartObj != null) {
			if (gameStartObj.GetComponent<BattleGameStartScript>() != null) {
				battleGameStartScript = gameStartObj.GetComponent<BattleGameStartScript>();
			}

			if (gameStartObj.GetComponent<GameStartScript>() != null) {
				gameStartScript = gameStartObj.GetComponent<GameStartScript>();
			}
		}

		gameTopObj = GameObject.Find("GameTopManager");
		if (gameTopObj != null) {
			gameTopScript = gameTopObj.GetComponent<GameTopScript>();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void retryScene() {
		gameStartScript.retryGame();
	}

	public void topScene() {
		gameStartScript.gameTop();
	}

	public void battleScene() {
		gameTopScript.gameBattle();
	}

	public void sendFukidashiMessage() {
		GameObject childObject = this.gameObject.transform.FindChild("Text").gameObject;
		string s = "a";
		if (childObject != null) {
			s = childObject.GetComponent<Text>().text;
		}

		if (battleGameStartScript != null) {
			battleGameStartScript.showFukidashiMessage(s);
		}
	}

	public void battleGameTopScene() {
		battleGameStartScript.sceneTop();
	}

	public void battleGameRetryScene() {
		battleGameStartScript.sceneRetry();
	}

	public void actionSkill() {
		gameStartScript.actionSkill();
	}

	public void battleActionSkill() {
		battleGameStartScript.actionSkill();
	}
}
