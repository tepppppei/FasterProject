using UnityEngine;
using System.Collections;

public class BombScript : MonoBehaviour {

	//Effect系
	private GameObject bombEffectPrefab;

	// Use this for initialization
	void Start () {
		bombEffectPrefab = (GameObject)Resources.Load("Prefab/EffBurst1");
	}
	
	void OnCollisionEnter2D(Collision2D collision){
		if(collision.gameObject.tag == "Chara"){
			// プレハブからインスタンスを生成
			Instantiate (bombEffectPrefab, this.gameObject.transform.localPosition, Quaternion.identity);
		}
	}
}
