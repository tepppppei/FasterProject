using UnityEngine;
using System.Collections;

public class DestroyScript : MonoBehaviour {

	public float destroyTime = 1.0f;

	// Use this for initialization
	void Start () {
		Destroy(this.gameObject, destroyTime);
	}
}
