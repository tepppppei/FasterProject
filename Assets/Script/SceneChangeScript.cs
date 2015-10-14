using UnityEngine;
using System.Collections;

public class SceneChangeScript : MonoBehaviour {

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    public void startScene() {
        Application.LoadLevel("RaceScene");
    }

    public void battleScene() {
        Application.LoadLevel("RaceBattleScene");
    }
}
