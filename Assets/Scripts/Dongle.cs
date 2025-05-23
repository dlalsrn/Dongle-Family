using System.Collections;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rigidbody2d;
    private CircleCollider2D collider2d;
    private SpriteRenderer spriteRenderer;

    private float leftBorder = -4.7f;
    private float rightBorder = 4.7f;
    private float radius;

    [SerializeField]
    private int level;
    public int Level => level;
    private int maxLevel = 7;

    private float deadTime;

    private bool isDrag;
    private bool isMerge;
    public bool IsMerge => isMerge;
    private bool isAttach;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // X 좌표 최소, 최대 설정
            float newPosX = Mathf.Clamp(mousePos.x, leftBorder + radius, rightBorder - radius);
            mousePos.x = newPosX;
            mousePos.y = 7f;
            mousePos.z = 0f;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }

    public void Drag()
    {
        isDrag = true;
        radius = collider2d.radius * transform.localScale.x;
    }

    public void Drop()
    {
        isDrag = false;
        rigidbody2d.simulated = true;
    }

    public void Init(int level)
    {
        transform.position = transform.parent.transform.position;
        transform.localRotation = Quaternion.identity;

        rigidbody2d.simulated = false;
        rigidbody2d.linearVelocity = Vector2.zero;
        rigidbody2d.angularVelocity = 0f;
        collider2d.enabled = true;

        isMerge = false;
        isAttach = false;
        this.level = level;

        animator.SetInteger("Level", this.level);
    }

    // (목표 지점, 게임 오버 유무)
    public void Hide(Vector3 targetPos, bool isOver)
    {
        isMerge = true;

        rigidbody2d.simulated = false;
        collider2d.enabled = false;

        StartCoroutine(HideRoutine(targetPos, isOver));
    }

    IEnumerator HideRoutine(Vector3 targetPos, bool isOver)
    {
        for (int frameCnt = 0; frameCnt < 20; frameCnt++)
        {
            if (isOver)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
                yield return null;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
                yield return null;
            }
        }

        if (isOver)
        {
            PlayEffect();
        }

        GameManager.Instance.AddScore((int)Mathf.Pow(2, level));
        isMerge = false;

        ObjectPoolManager.Instance.Release(ObjectType.Dongle, gameObject);
    }

    public void LevelUp()
    {
        isMerge = true;

        rigidbody2d.linearVelocity = Vector2.zero;
        rigidbody2d.angularVelocity = 0f;

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        animator.SetInteger("Level", level + 1);
        PlayEffect();
        SoundManager.Instance.PlaySfx(Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        isMerge = false;
    }

    private void PlayEffect()
    {
        // Object Pool을 통해 Effect Get
        ParticleSystem levelUpFx = ObjectPoolManager.Instance.Get(ObjectType.LevelUpEffect).GetComponent<ParticleSystem>();
        levelUpFx.transform.position = transform.position;
        levelUpFx.transform.localScale = transform.localScale;
        levelUpFx.gameObject.SetActive(true);
        levelUpFx.Play();

        StartCoroutine(ReleaseRoutine(levelUpFx));
    }

    IEnumerator ReleaseRoutine(ParticleSystem levelUpFx)
    {
        yield return new WaitUntil(() => !levelUpFx.IsAlive(true));
        
        ObjectPoolManager.Instance.Release(ObjectType.LevelUpEffect, levelUpFx.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }

        isAttach = true;
        SoundManager.Instance.PlaySfx(Sfx.Attach);
        yield return new WaitForSeconds(0.2f);
        isAttach = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Dongle"))
        {
            Dongle otherDongle = collision.gameObject.GetComponent<Dongle>();

            // 같은 Level의 Dongle이고, 두 Dongle이 모두 Merge 중이 아닐 때, 게임이 진행 중일 때
            if (otherDongle.Level == level && !isMerge && !otherDongle.IsMerge && level < maxLevel && GameManager.Instance.IsLive)
            {
                float myPosX = transform.position.x;
                float myPosY = transform.position.y;
                float otherPosX = otherDongle.transform.position.x;
                float otherPosY = otherDongle.transform.position.y;

                // #1. 내가 아래에 있을 때
                // #2. 동일한 높이에서, 내가 오른쪽에 있을 때
                if ((myPosY < otherPosY) || (myPosY == otherPosY && myPosX > otherPosX))
                {
                    // 나는 레벨업, 상대방은 제거
                    otherDongle.Hide(transform.position, false);
                    LevelUp();
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Finish"))
        {
            deadTime += Time.deltaTime;

            if (deadTime >= 2f)
            {
                spriteRenderer.color = new Color(0.8f, 0.2f, 0.2f);
            }

            if (deadTime >= 5f)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Finish"))
        {
            deadTime = 0f;
            spriteRenderer.color = Color.white;
        }
    }
}
