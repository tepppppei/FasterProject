using UnityEngine;
using System.Collections;

public class ParticleScript : MonoBehaviour
{
    // Awake
    void Awake()
    {
        // mul scale to particles
        var particles = this.gameObject.GetComponentsInChildren<ParticleSystem>();
        for( int i=0; i < particles.Length; ++i )
        {
            particles[ i ].startSize *= m_Scale;
        }
    }

    void Start() {
        Destroy(this.gameObject, 1.0f);
    }

    // member
    public float        m_Scale = 1.0f;
    public float        l_Scale = 1.0f;
}