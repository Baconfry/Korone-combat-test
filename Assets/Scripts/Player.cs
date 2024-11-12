using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum AnimationState { idle, walk, run, hurt, punch1, punch2, airKick, jump, shoot } //this tracks what animation the player should use
    public AnimationState animationState;

    public enum AnimationType { looping, sequence, holdLastFrame } //this determines what happens when the animation finishes
    private int animationIndex = 0;

    //these are to be set in the inspector. Use duplicates for frames that you wish to extend
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private Sprite[] hurtSprites;
    [SerializeField] private Sprite[] jumpSprites;
    [SerializeField] private Sprite[] kickSprites;
    [SerializeField] private Sprite[] punch1Sprites;
    [SerializeField] private Sprite[] punch2Sprites;

    [SerializeField] private float secondsPerFrame;
    private GameObject hitbox; //this child object provides the collider for melee attacks

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D playerCollider;
    private Rigidbody2D rig;

    [SerializeField] private float walkSpeed;
    private bool isJumping;
    private bool isAttacking;
    private KeyCode dashDirection;
    private float dashTimer;
    private bool dashEnabled;
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        rig = GetComponent<Rigidbody2D>();
        hitbox = transform.Find("hitbox").gameObject;
        hitbox.SetActive(false);
        StartCoroutine(AnimatePlayer(AnimationState.idle));
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAttacking && animationState != AnimationState.hurt)
        {
            if (!isJumping && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
            {
                animationState = AnimationState.jump;
                isJumping = true;
                rig.velocity = new Vector3(rig.velocity.x, 5f, 0f);
            }

            if (Input.GetKey(KeyCode.A))
            {
                spriteRenderer.flipX = true;
                if (dashDirection == KeyCode.A && dashEnabled)
                {
                    if (!isJumping) animationState = AnimationState.run;

                    rig.velocity = new Vector3(-walkSpeed * 2, rig.velocity.y, 0f);
                }
                else
                {
                    if (!isJumping) animationState = AnimationState.walk;

                    rig.velocity = new Vector3(-walkSpeed, rig.velocity.y, 0f);
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                spriteRenderer.flipX = false;
                if (dashDirection == KeyCode.D && dashEnabled)
                {
                    if (!isJumping) animationState = AnimationState.run;
                    rig.velocity = new Vector3(walkSpeed * 2, rig.velocity.y, 0f);
                }
                else
                {
                    if (!isJumping) animationState = AnimationState.walk;
                    rig.velocity = new Vector3(walkSpeed, rig.velocity.y, 0f);
                }
            }
            else if (!isJumping)
            {
                animationState = AnimationState.idle;
                rig.velocity = new Vector3(0f, rig.velocity.y, 0f);
            }

            if (Input.GetKeyUp(KeyCode.A) && IsTouchingSurface())
            {
                dashDirection = KeyCode.A;
                if (!dashEnabled) 
                { 
                    StartCoroutine(EnableDash()); 
                }
                else { dashTimer = 0.15f; }
            }
            else if (Input.GetKeyUp(KeyCode.D) && IsTouchingSurface())
            {
                dashDirection = KeyCode.D;
                if (!dashEnabled)
                {
                    StartCoroutine(EnableDash());
                }
                else { dashTimer = 0.15f; }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (isJumping)
                {
                    animationState = AnimationState.airKick;
                    isAttacking = true;
                }
                else 
                {
                    animationState = AnimationState.punch1;
                    isAttacking = true;
                    rig.velocity = new Vector3(0f, rig.velocity.y, 0f);
                }
            }      
        }

        if (!IsTouchingSurface() && !isAttacking) //if player walked off a ledge without manually jumping
        {
            isJumping = true;
            animationState = AnimationState.jump;
        }

        if (OverlappedHurtbox())
        {
            animationState = AnimationState.hurt;
        }
        if (animationState == AnimationState.hurt) rig.velocity = new Vector3(0f, rig.velocity.y, 0f);
    }

    IEnumerator EnableDash()
    {
        dashEnabled = true;
        dashTimer = 0.15f;
        while (dashTimer > 0)
        {
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) dashTimer -= Time.deltaTime;
            yield return null;
        }
        dashEnabled = false;
    }

    private bool IsTouchingSurface()
    {
        bool returnValue = false;
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> overlappingColliders = new List<Collider2D>();
        if (Physics2D.OverlapCollider(playerCollider, filter, overlappingColliders) > 0)
        {
            //Debug.Log(overlappingColliders[0]);
            for (int i = 0; i < overlappingColliders.Count; i++)
            {
                if (!overlappingColliders[i].isTrigger)
                {
                    returnValue = true;
                    break;
                }
            }
        }
        return returnValue;
    }

    private bool OverlappedHurtbox()
    {
        if (animationState == AnimationState.hurt) return false;
        bool returnValue = false;
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> overlappingColliders = new List<Collider2D>();
        if (Physics2D.OverlapCollider(playerCollider, filter, overlappingColliders) > 0)
        {
            //Debug.Log(overlappingColliders[0]);
            for (int i = 0; i < overlappingColliders.Count; i++)
            {
                if (overlappingColliders[i].gameObject.tag == "EnemyAttack")
                {
                    returnValue = true;
                    spriteRenderer.flipX = transform.position.x > overlappingColliders[i].transform.parent.transform.position.x;
                    break;
                }
            }
        }
        return returnValue;
    }

    private Sprite[] GetSpriteSheet(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.idle:
                return idleSprites;
            case AnimationState.walk:
                return walkSprites;
            case AnimationState.run:
                return runSprites;
            case AnimationState.hurt:
                return hurtSprites;
            case AnimationState.jump:
                return jumpSprites;
            case AnimationState.airKick:
                return kickSprites;
            case AnimationState.punch1:
                return punch1Sprites;
            case AnimationState.punch2:
                return punch2Sprites;
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
            case AnimationState.run:
                return AnimationType.looping;
            case AnimationState.jump:
                return AnimationType.holdLastFrame;
            case AnimationState.punch1:
            case AnimationState.punch2:
            case AnimationState.hurt:
            case AnimationState.airKick:
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
                return 0.1f;
            case AnimationState.walk:
            case AnimationState.run:
                return 0.07f;
            case AnimationState.jump:
                return 0.1f;
            case AnimationState.punch1:
            case AnimationState.punch2:
                return 0.04f;
            default:
                return 0.1f;
        }
    }

    private void SetState(AnimationState state)
    {

        switch (state)
        {
            case AnimationState.idle:
            case AnimationState.walk:
            case AnimationState.run:
            case AnimationState.hurt:
                isJumping = false;
                isAttacking = false;
                break;
            case AnimationState.jump:
                isJumping = true;
                isAttacking = false;
                break;
            case AnimationState.airKick:
                isJumping = true;
                isAttacking = true;
                break;
            case AnimationState.punch1:
            case AnimationState.punch2:
                isJumping = false;
                isAttacking = true;
                break;
            default:
                isJumping = false;
                isAttacking = false;
                break;
        }
    }

    private void GenerateHitbox(AnimationState state, int index)
    {
        hitbox.SetActive(false);
        if (state == AnimationState.punch1)
        {
            switch (index)
            {
                case 2:
                    hitbox.SetActive(true);
                    //move the hitbox to the position and scale you want in the scene, then paste the values here
                    hitbox.transform.localPosition = new Vector3(spriteRenderer.flipX ? -0.573f : 0.573f, 0.641f, 0f); 
                    hitbox.transform.localScale = new Vector3(0.5125f, 0.5748f, 1f);
                    break;
                default:
                    hitbox.SetActive(false);
                    break;
            }
        }
        else if (state == AnimationState.punch2)
        {
            switch (index)
            {
                case 1:
                    hitbox.SetActive(true);
                    hitbox.transform.localPosition = new Vector3(spriteRenderer.flipX ? -0.711f : 0.711f, 0.494f, 0f);
                    hitbox.transform.localScale = new Vector3(0.5125f, 0.5748f, 1f);
                    break;
                default:
                    hitbox.SetActive(false);
                    break;
            }
        }
        else if (state == AnimationState.airKick)
        {
            switch (index)
            {
                case 1:
                    hitbox.SetActive(true);
                    hitbox.transform.localPosition = new Vector3(spriteRenderer.flipX ? -0.2902f : 0.2902f, 0.3122f, 0f);
                    hitbox.transform.localScale = new Vector3(0.6288f, 0.374f, 1f);
                    break;
                default:
                    hitbox.SetActive(false);
                    break;
            }
        }
    }

    IEnumerator AnimatePlayer(AnimationState state) //handles player animations
    {
        animationState = state;
        animationIndex = 0;
        while (true)
        {
            Sprite[] spriteSheet = GetSpriteSheet(animationState);
            AnimationState initialState = animationState;
            AnimationType type = GetAnimationType(animationState);
            secondsPerFrame = GetAnimationSpeed(animationState);
            bool clickedDuringAnimation = false;
            while (animationState == initialState)
            {
                spriteRenderer.sprite = spriteSheet[animationIndex];
                GenerateHitbox(animationState, animationIndex);
                float timer = 0f;
                while (timer < secondsPerFrame)
                {
                    if (Input.GetMouseButtonDown(0)) clickedDuringAnimation = true;
                    timer += Time.deltaTime;
                    yield return null;
                }
                //Debug.Log(timer);
                animationIndex++;
                if (animationIndex >= spriteSheet.Length)
                {
                    if (type == AnimationType.looping)
                    {
                        animationIndex = 0;
                    }
                    else if (type == AnimationType.sequence)
                    {
                        if (initialState == AnimationState.punch1 && clickedDuringAnimation) { animationState = AnimationState.punch2; }
                        else { animationState = AnimationState.idle; }
                    }
                    else if (type == AnimationType.holdLastFrame)
                    {
                        animationIndex--;
                    }
                }

            }
            SetState(animationState);
            animationIndex = 0;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.otherCollider.gameObject.tag != "Bumper") 
        {
            isJumping = false;
            animationState = AnimationState.idle;
            spriteRenderer.sprite = idleSprites[0];
            animationIndex = 0;
            //Debug.Log(other.collider.gameObject);
        } 
    }

    /*void OnCollisionExit2D(Collision2D other)
    {
        animationState = AnimationState.jump;
        isJumping = true;
    }*/
}
