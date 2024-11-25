using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChowMein : MonoBehaviour
{
    private enum AnimationState { idle, walk, hit, attack, dying }
    private AnimationState animationState;
    private enum AnimationType { looping, sequence }

    [SerializeField] private GameObject hitVFX;

    [SerializeField] private GameObject expParticle;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D objectCollider;
    private Rigidbody2D rig;
    private Transform playerTransform;
    private GameObject hitbox;

    [SerializeField] private int maxHealth;
    public int health;
    [SerializeField] private float detectionRange;

    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite[] hitSprites;
    [SerializeField] private Sprite[] attackSprites;
    [SerializeField] private Sprite[] dyingSprites;
    [SerializeField] private float secondsPerFrame;
    private int animationIndex = 0;
    private bool canBeDamaged;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<BoxCollider2D>();
        rig = GetComponent<Rigidbody2D>();
        hitbox = transform.Find("hitbox").gameObject;
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();

        animationState = AnimationState.idle;
        StartCoroutine(AnimateEnemy());
        canBeDamaged = true;
        health = maxHealth;
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
                    animationIndex = 0;
                    health--;
                    overlappingColliders[i].gameObject.SetActive(false);
                    if (health <= 0)
                    {
                        animationState = AnimationState.dying;
                        canBeDamaged = false;
                    }
                    else
                    {
                        animationState = AnimationState.hit;
                        spriteRenderer.color = Color.black;
                        canBeDamaged = false;
                    }
                    Vector3 VFXposition;
                    if (overlappingColliders[i].GetComponent<PlayerProjectile>() == null)
                    {
                        VFXposition = new Vector3((transform.position.x + overlappingColliders[i].transform.position.x) / 2, (transform.position.y + overlappingColliders[i].transform.position.y) / 2, 0f);
                    }
                    else
                    {
                        VFXposition = overlappingColliders[i].transform.position;
                    }
                        Instantiate(hitVFX, VFXposition, Quaternion.identity);
                    break;
                }
            }
        }

        if (animationState == AnimationState.idle && Mathf.Abs(transform.position.x - playerTransform.position.x) < detectionRange)
        {
            //Debug.Log("player detected");
            spriteRenderer.flipX = playerTransform.position.x > transform.position.x;
            animationState = AnimationState.attack;
        }

        rig.velocity = Vector3.zero;
    }

    private Sprite[] GetSpriteSheet(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.idle:
                return idleSprites;
            case AnimationState.walk:
                return walkSprites;
            case AnimationState.hit:
                return hitSprites;
            case AnimationState.attack:
                return attackSprites;
            case AnimationState.dying:
                return dyingSprites;
            default:
                return null;
        }
    }

    private AnimationType GetAnimationType(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.idle:
            case AnimationState.walk:
                return AnimationType.looping;
            case AnimationState.hit:
            case AnimationState.attack:
            case AnimationState.dying:
                return AnimationType.sequence;
            default:
                return AnimationType.sequence;
        }
    }

    private float GetAnimationSpeed(AnimationState state) //you can make certain animations play out faster
    {
        switch (state)
        {
            case AnimationState.idle:
                return 0.2f;
            case AnimationState.walk:
                return 0.1f;
            case AnimationState.hit:
                return 0.1f;
            case AnimationState.attack:
                return 0.06f;
            case AnimationState.dying:
                return 0.04f;
            default:
                return 0.1f;
        }
    }

    private void GenerateHitbox(AnimationState state, int index)
    {
        hitbox.SetActive(false);
        if (state == AnimationState.attack)
        {
            switch (index)
            {
                case 4:
                    hitbox.SetActive(true);
                    hitbox.transform.localPosition = new Vector3(spriteRenderer.flipX ? 0.4132f : -0.4132f, 0.318f, 0f);
                    hitbox.transform.localScale = new Vector3(0.83f, 0.5748f, 1f);
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator AnimateEnemy()
    {
        animationIndex = 0;
        while (true)
        {
            Sprite[] spriteSheet = GetSpriteSheet(animationState);
            AnimationState initialState = animationState;
            AnimationType type = GetAnimationType(animationState);
            secondsPerFrame = GetAnimationSpeed(animationState);
            while (animationState == initialState)
            {
                spriteRenderer.sprite = spriteSheet[animationIndex];
                GenerateHitbox(animationState, animationIndex);
                float timer = 0f;
                while (timer < secondsPerFrame)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                //Debug.Log(timer);
                animationIndex++;
                if (animationIndex == 1 && animationState == AnimationState.hit)
                { 
                    canBeDamaged = true;
                    spriteRenderer.color = Color.white;
                }
                if (animationIndex >= spriteSheet.Length)
                {
                    if (type == AnimationType.looping)
                    {
                        animationIndex = 0;
                    }
                    else if (type == AnimationType.sequence)
                    {
                        if (animationState == AnimationState.dying)
                        {
                            /*for (int i = 0; i < 6; i++)
                            {
                                Vector3 randomPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
                                Instantiate(expParticle, transform.position + randomPosition, Quaternion.identity);
                            }*/
                            Destroy(this.gameObject);
                        }
                        animationState = AnimationState.idle;
                        canBeDamaged = true;
                        spriteRenderer.color = Color.white;
                    }
                }

            }
            animationIndex = 0;
        }
    }
}
