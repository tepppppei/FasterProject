using UnityEngine;
using System.Collections;

public class AddFloorScript : MonoBehaviour {

    public GameObject gameStartObj;

    //GameStartObjectのScript
    NewGameStartScript newGameStartScript;

    // Use this for initialization
    void Start () {
        gameStartObj = GameObject.Find("GameStartObj");
        if (gameStartObj != null) {
            newGameStartScript = gameStartObj.GetComponent<NewGameStartScript>();
        }
    }

    void OnTriggerExit2D(Collider2D collider){
        if(collider.gameObject.tag == "Chara"){
            newGameStartScript.addFloor();
        }
    }
}
