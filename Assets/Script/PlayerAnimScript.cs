using UnityEngine;
using System.Collections;

public class PlayerAnimScript : MonoBehaviour
{
    //Animationの種類
    public enum AnimeName {
        idle,
        run,
        walk
    }

    private string[] AnimeNameArray = {
        "idle", "run", "walk",
    };
    public int AnimeNoNow, AnimeNoWas;
    public float AnimeSpeedNow, AnimeSpeedWas;

    //==========================================
    //設定
    //==========================================
    void Start () {
        AnimeNoNow = (int)AnimeName.idle;
        AnimeNoWas = -1;

        AnimeSpeedNow = 1.0f;
        AnimeSpeedWas = 1.0f;
    }

    //==========================================
    //アニメ切り替え
    //==========================================
    public void AnimeSet (int AnimeNo, float AnimeSpeed) {
        AnimeNoNow = AnimeNo;
        AnimeSpeedNow = AnimeSpeed;
    }

    void Update () {
        if (AnimeNoNow != AnimeNoWas || AnimeSpeedNow != AnimeSpeedWas) {
            foreach (AnimationState anim in GetComponent<Animation>()) {
                if (anim.name.IndexOf (AnimeNameArray [AnimeNoNow]) >= 0) {
                    GetComponent<Animation>().GetComponent<Animation>() [anim.name].speed = AnimeSpeedNow;
                    GetComponent<Animation>().CrossFade (anim.name);
                    break;
                }
            }

            AnimeNoWas = AnimeNoNow;
            AnimeSpeedWas = AnimeSpeedNow;
        }
    }
}
