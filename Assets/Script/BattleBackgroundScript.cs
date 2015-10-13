using UnityEngine;
using System.Collections;

public class BattleBackgroundScript : MonoBehaviour {
    private GameObject chara;
    public Vector3 offset;
    private float defaultX;

    private GameObject[] charas;

    // Use this for initialization
    void Start () {
        this.offset = this.transform.position;
        defaultX = this.chara.transform.position.x;
    }

    // Update is called once per frame
    void Update () {
        if (chara != null) {
            float x = this.offset.x + ((chara.transform.position.x - defaultX) * 0.82f);
            this.transform.position = new Vector3 (x, this.offset.y ,1);
        } else {
            charas = GameObject.FindGameObjectsWithTag("Chara");
            for (int i = 0; i < charas.Length; i++) {
                if (charas[i].GetComponent<PhotonView>().isMine) {
                    chara = charas[i];
                }
            }
        }
    }
}
