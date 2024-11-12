using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitVFX : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float secondsPerFrame = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(PlayAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PlayAnimation()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(secondsPerFrame);
        }
        Destroy(this.gameObject);
    }
}
