using UnityEngine;
using System.Collections;

public class ButtonAnimationScript : MonoBehaviour {

	// Use this for initialization
	private float defaultX;
	private float defaultY;

	private float defaultPositionX;
	private float defaultPositionY;
	public int animationType = 1;

	void Start () {
		//回転系
		if (animationType == 1) {
			defaultX = this.gameObject.transform.localEulerAngles.x;
			defaultY = this.gameObject.transform.localEulerAngles.y;

	        iTween.ValueTo(this.gameObject, iTween.Hash(
	        	"from", 22,
	        	"to", -22,
	        	"time", 1.5f,
	        	"looptype","pingpong",
				"onupdate", "valueChange"
				));
	    //右横移動系
		} else if (animationType == 2) {
			defaultPositionX = transform.localPosition.x;
			defaultPositionY = transform.localPosition.y;

	        iTween.ValueTo(this.gameObject, iTween.Hash(
	        	"from", defaultPositionX,
	        	"to", (defaultPositionX + 5.0f),
	        	"time", 0.8f,
	        	"looptype","pingpong",
				"onupdate", "valueChangePosition"
				));
	    //左横移動系
		} else if (animationType == 3) {
			defaultPositionX = transform.localPosition.x;
			defaultPositionY = transform.localPosition.y;

	        iTween.ValueTo(this.gameObject, iTween.Hash(
	        	"from", defaultPositionX,
	        	"to", (defaultPositionX - 5.0f),
	        	"time", 0.8f,
	        	"looptype","pingpong",
				"onupdate", "valueChangePosition"
				));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void valueChange(float value){
        transform.rotation = Quaternion.Euler(defaultX, defaultY, value);
    }

    void valueChangePosition(float value){
        transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z);
    }

}
