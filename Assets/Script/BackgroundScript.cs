using UnityEngine;
using System.Collections;

public class BackgroundScript : MonoBehaviour {

/*	
	public GameObject chara;
    public Vector3 offset;
    private float defaultX;

	// スクロールするスピード
	public float speed = 50.00f;


    // Use this for initialization
    void Start () {
        this.offset = this.transform.position;
        defaultX = this.chara.transform.position.x;
    }

    // Update is called once per frame
    void Update () {
    	float x = this.offset.x + ((this.chara.transform.position.x - defaultX) * 0.82f);
        this.transform.position = new Vector3 (x, this.offset.y ,1);
    }

	void Update () {
		// 時間によってYの値が0から1に変化していく。1になったら0に戻り、繰り返す。

		float x = Mathf.Repeat (this.chara.transform.position.x * speed, 1);

		// Yの値がずれていくオフセットを作成
		Vector2 offset = new Vector2 (x, 0);

		// マテリアルにオフセットを設定する
		renderer.sharedMaterial.SetTextureOffset ("_MainTex", offset);
	}
	*/

    [Range(0, 10)]
    public float speed = 10;
    public int spriteCount = 3;
    Vector3 spriteSize;
    Vector3 spritePosition;

    void Start () {
        spriteSize = GetComponent<SpriteRenderer>().bounds.size;
        spritePosition = transform.localPosition;
        Debug.Log("SPRITE SIZE:" + spriteSize);
    }
    void Update () {
        transform.position += Vector3.left * speed * Time.deltaTime;
        var spritex = (transform.position + spriteSize / 2).x ;
        if( spritex < ScreenManager.Instance.screenRect.x ) {
            Debug.Log("ついかー");
            OnBecameInvisible();
        }
    }

    void OnBecameInvisible () {
        float width = GetComponent<SpriteRenderer> ().bounds.size.x;
        Debug.Log("WIDTH:" + (ScreenManager.Instance.screenRect.x + (width / 2)));
        //transform.position += Vector3.right * width * spriteCount;
        transform.position = new Vector3((ScreenManager.Instance.screenRect.x + (width * 0.9f)), transform.position.y, transform.position.z);
    }
}
