using UnityEngine;
using System.Collections;

public class JumpBombScript : MonoBehaviour {

    //Effect系
    private GameObject bombEffectPrefab;

    // Use this for initialization
    void Start () {
        bombEffectPrefab = (GameObject)Resources.Load("Prefab/EffBurst1");
        StartCoroutine(bombJump());
    }

    void OnCollisionEnter2D(Collision2D collision){
        if(collision.gameObject.tag == "Chara"){
            // プレハブからインスタンスを生成
            Instantiate (bombEffectPrefab, this.gameObject.transform.localPosition, Quaternion.identity);
        }
    }

    IEnumerator bombJump() {
        while (true) {
            float waitTime = UnityEngine.Random.Range(0.3F, 1.0F);
            yield return new WaitForSeconds(waitTime);

            float posX = this.gameObject.transform.localPosition.x;
            float posY = this.gameObject.transform.localPosition.y + 2.0f;
            iTween.MoveTo(this.gameObject, iTween.Hash(
                        "position", new Vector3(posX, posY, 0),
                        "time", 0.2f,
                        "oncomplete", "CompleteHandler",
                        "oncompletetarget", gameObject,
                        "easeType", "linear"
            ));
        }
    }
}
