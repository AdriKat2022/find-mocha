using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float yDispawn;
    [SerializeField]
    private bool respawn;
    [SerializeField]
    private float respawnTime;

    [Header("Direction")]

    [SerializeField]
    private bool useSpecialDirection;
    [SerializeField]
    private Vector2 specialDirection;

    private Vector2 speedDirection;
    private float speed;
    private bool isFalling;
    private Vector2 startPosition;

    [Header("References")]
    [SerializeField]
    private Animator animator;
    private PlayerController playerController;
    private Rigidbody2D rb;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        isFalling = false;
        playerController = PlayerController.Instance;
        speed = 0;

        startPosition = transform.position;
        speedDirection = Vector2.down;

        if (useSpecialDirection)
            speedDirection = specialDirection.normalized;

    }

    private void Reset()
    {
        rb.velocity = Vector2.zero;
        speed = 0;
        isFalling = false;
        transform.position = startPosition;
        animator.SetBool("isFalling", false);
        animator.SetTrigger("reset");
    }

    private IEnumerator PlatformFall()
    {
        animator.SetBool("isFalling", true);

        while(transform.position.y > yDispawn)
        {
            rb.velocity = speedDirection * speed;

            speed += acceleration * Time.deltaTime;

            speed = Mathf.Clamp(speed, 0, maxSpeed);

            yield return null;
        }

        Debug.Log("going to respawn");

        if(!respawn)
            Destroy(gameObject);

        yield return new WaitForSeconds(respawnTime);

        Reset();

        yield break;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isFalling)
            return;
        if (collision?.gameObject == playerController.gameObject) {
            if (collision.gameObject.transform.position.y - collision.collider.bounds.size.y/2 > transform.position.y)
            {
                isFalling = true;
                StartCoroutine(PlatformFall());
            }
        }
    }
}
