using UnityEngine;
using System.Collections;

public class SpriteFlip : MonoBehaviour
{

    [HideInInspector]
    public bool flippedX = false;

    private SpriteRenderer m_Sprite;
    private bool m_Initialized = false;

    // Use this for initialization
    public void Initialize()
    {
        if (m_Initialized) return;
        m_Sprite = GetComponent<SpriteRenderer>();
        if(!m_Sprite) { Debug.LogError("Cannot find 'SpriteRenderer' component.");return; }
        m_Initialized = true;
    }

    void Start()
    {
        Initialize();
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode .G))
        {
            flip(true);
        }
        if(Input.GetKeyDown (KeyCode.F))
        {
            flip(false);
        }
    }

    public void flip(bool flipX)
    {
        if (!m_Sprite) m_Sprite = GetComponent<SpriteRenderer>();
        if (!m_Sprite) { Debug.LogError("Cannot find 'SpriteRenderer' component."); return; }

        flippedX = flipX;
#if UNITY_5_1_1

        int value = (flipX ? -1 : 1);
        Vector3 scale = m_Sprite.gameObject.transform.localScale;
        scale.x =Mathf.Abs ( scale.x) * value;
        m_Sprite.gameObject.transform.localScale = scale;
        m_Sprite.sharedMaterial.SetInt("_Flip", value);
#else
        m_Sprite.flipX = flipX;
        int value = (flipX ? -1 : 1);
        m_Sprite.sharedMaterial.SetInt("_Flip", value);
#endif
    }
}
