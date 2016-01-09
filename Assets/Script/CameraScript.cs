using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

private GameObject chara;
    public Vector3 offset;
    public float moveOffsetX = 0;

    // Use this for initialization
    void Start () {
        this.offset = this.transform.position;
    }

    // Update is called once per frame
    void LateUpdate () {
        if (chara == null) {
            chara = GameObject.Find("Character");
        } else {
            this.transform.position = new Vector3 (this.transform.position.x + moveOffsetX, this.offset.y , this.chara.transform.position.z + this.offset.z);
        }
    }

    private Camera cam;
    private float width = 740f;
    private float height = 1136f;
    private float pixelPerUnit = 100f;

    void Awake () {
/*
        float screenRate = (float)1024 / Screen.height;
        if( screenRate > 1 ) screenRate = 1;
        int width = (int)(Screen.width * screenRate);
        int height = (int)(Screen.height * screenRate);
        Screen.SetResolution( width , height, true, 5);
*/

        //float aspect = (float)Screen.height / (float)Screen.width;
        float aspect = (float)1024 / (float)Screen.height;
        float bgAcpect = height / width;

        cam = GetComponent<Camera> ();
        cam.orthographicSize = (height / 2f / pixelPerUnit);


        if (bgAcpect > aspect) {
            float bgScale = height / Screen.height;
            float camWidth = width / (Screen.width * bgScale);
            cam.rect = new Rect ((1f - camWidth) / 2f, 0f, camWidth, 1f);
        } else {
            float bgScale = width / Screen.width;
            float camHeight = height / (Screen.height * bgScale);
            cam.rect = new Rect (0f, (1f - camHeight) / 2f, 1f, camHeight);
        }
    }
}
