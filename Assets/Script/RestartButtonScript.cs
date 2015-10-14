using UnityEngine;
using System.Collections;

public class RestartButtonScript : MonoBehaviour {

    // Use this for initialization
    void Start () {

    }

    public void restartScene() {
        Application.LoadLevel("RaceScene");
    }

    public void battleRestartScene() {
        Application.LoadLevel("RaceBattleScene");
    }

    public void topScene() {
        Application.LoadLevel("TopScene");
    }
}
