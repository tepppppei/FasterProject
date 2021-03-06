﻿using UnityEngine;
using System.Collections;

public class BattleCameraScript : MonoBehaviour {
    private GameObject chara;
    public Vector3 offset;

    private GameObject[] charas;

    public bool cameraStartFlg = false;

    // Use this for initialization
    void Start () {
        this.offset = this.transform.position;
    }

    // Update is called once per frame
    void LateUpdate () {
        if (chara != null) {
            if (cameraStartFlg) {
                this.transform.position = new Vector3 (this.chara.transform.position.x + this.offset.x, 0.78f, this.chara.transform.position.z + this.offset.z);
            }
        } else {
            charas = GameObject.FindGameObjectsWithTag("Chara");
            for (int i = 0; i < charas.Length; i++) {
                if (charas[i].GetComponent<PhotonView>().isMine) {
                    chara = charas[i];
                }
            }
        }
    }

    private Camera cam;
    private float width = 740f;
    private float height = 1136f;
    private float pixelPerUnit = 100f;

    void Awake () {
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
