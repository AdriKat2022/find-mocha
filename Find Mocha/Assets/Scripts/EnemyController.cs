using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour, IDamageble
{
    [Header("Main properties")]
    public float maxHp;
    public float attack;
    public float speed;

    [Header("Knockback")]
    public float knockbackAngle;
    public float knockbackForce;

    [Header("Path")]
    public float endLag;
    public Vector2 startPath;
    public Vector2 endPath;
    public float precision = .2f;

    private GameObject player;
    private PlayerController playerController;

    private Rigidbody2D rb;
    private Collider2D col;


    private bool started = false;
    private Vector2 startingPosition;
    private Vector2 pointA;
    private Vector2 pointB;


    private bool isDead;
    private bool isSuperdead;

    private float hp;


#if UNITY_EDITOR

    private void OnValidate()
    {
        Vector2 pos;

        if(!started)
            pos = transform.position;
        else
            pos = startingPosition;

        pointB =  pos + endPath;
        pointA = pos + startPath;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pointA, pointB);
    }

#endif

    private void Awake()
    {
        player = GameObject.Find("Milk");
        player.TryGetComponent(out playerController);

        TryGetComponent(out rb);
        TryGetComponent(out col);

        startingPosition = transform.position;
        started = true;

        pointB = (Vector2)transform.position + endPath;
        pointA = (Vector2)transform.position + startPath;

        StartCoroutine(PathProceed());
    }

    private void Start()
    {
        hp = maxHp;
    }


    private IEnumerator PathProceed()
    {
        while (true)
        {
            rb.velocity = new Vector2(pointB.x - transform.position.x, pointB.y - transform.position.y).normalized * speed;


            if ( Mathf.Abs(pointB.x - transform.position.x) < precision && Mathf.Abs(pointB.y - transform.position.y) < precision)
            {
                (pointA, pointB) = (pointB, pointA);
                yield return new WaitForSeconds(endLag);
            }

            yield return null;

            if(isDead || isSuperdead)
                yield break;
        }
    }



    public void Heal(float heal)
    {
        hp += heal;
        hp = Mathf.Clamp(hp, 0, maxHp);
    }
    public void Damage(IDamageble from, float damage, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0)
    {
        hp -= damage;
        CheckAlive();
    }
    public void InstaKill(IDamageble from, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0)
    {
        isSuperdead = true;
        isDead = true;
        Destroy(rb);
        Destroy(col);
        StartCoroutine(SuperDeath(knockback));
    }

    private IEnumerator SuperDeath(Vector2? knockback)
    {
        //rb.bodyType = RigidbodyType2D.Static;

        //Vector3 direction = (Vector2.up * Random.Range(-1f, 1f) + Vector2.right * Random.Range(-1f, 1f)).normalized;

        Vector3 direction = (Vector2)knockback;

        float baseAngleDir = Vector3.Angle(Vector3.right, direction);

        float angle = Mathf.PI * Random.Range(baseAngleDir - 15, baseAngleDir + 15) / 180;

        direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

        float timer = 0;

        while(timer < 5f)
        {
            //transform.rotation = Quaternion.Euler(0, 0, timer * 720);

            //transform.position += 20 * Time.deltaTime * direction.normalized;

            transform.SetPositionAndRotation(20 * Time.deltaTime * direction.normalized, Quaternion.Euler(0, 0, timer * 720));

            timer += Time.deltaTime;

            yield return null;
        }


        Destroy(gameObject);
    }


    private void CheckAlive()
    {
        if(hp <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        isDead = true;
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(!isDead && collision.gameObject == player)
        {
            playerController.Damage(this, attack, -collision.contacts[0].normal * knockbackForce);
        }
    }

}
