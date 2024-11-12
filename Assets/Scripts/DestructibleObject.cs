using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject hitVFX;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D objectCollider;
    private bool canBeDamaged;
    private int damageState;
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<BoxCollider2D>();
        damageState = 0;
        spriteRenderer.sprite = sprites[damageState];
        canBeDamaged = true;
    }

    // Update is called once per frame
    void Update()
    {
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> overlappingColliders = new List<Collider2D>();
        if (Physics2D.OverlapCollider(objectCollider, filter, overlappingColliders) > 0)
        {
            for (int i = 0; i < overlappingColliders.Count; i++)
            {
                if (overlappingColliders[i].gameObject.tag == "PlayerAttack" && canBeDamaged)
                {
                    overlappingColliders[i].gameObject.SetActive(false);
                    Vector3 VFXposition = new Vector3((transform.position.x + overlappingColliders[i].transform.position.x) / 2, (transform.position.y + overlappingColliders[i].transform.position.y) / 2, 0f);
                    Instantiate(hitVFX, VFXposition, Quaternion.identity);
                    StartCoroutine(PlayDamageAnimation());
                    break;
                } 
            }
        }
    }

    /*void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "PlayerAttack" && canBeDamaged)
        {
            StartCoroutine(PlayDamageAnimation());
            Debug.Log(this.gameObject);
        }
    }*/

    IEnumerator PlayDamageAnimation()
    {
        canBeDamaged = false;
        spriteRenderer.color = Color.black;
        damageState++;
        if (damageState < sprites.Length)
        {
            spriteRenderer.sprite = sprites[damageState];
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Destroy(this.gameObject);
        }
        canBeDamaged = true;
        spriteRenderer.color = Color.white;
    }
}
