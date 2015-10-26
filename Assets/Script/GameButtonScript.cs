using UnityEngine;
using System.Collections;

public class GameButtonScript : MonoBehaviour {

	private GameObject gameStartObj;
	private GameStartScript gameStartScript;

	// Use this for initialization
	void Start () {
		gameStartObj = GameObject.Find("GameStartObj");
		if (gameStartObj != null) {
			gameStartScript = gameStartObj.GetComponent<GameStartScript>();
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
}
