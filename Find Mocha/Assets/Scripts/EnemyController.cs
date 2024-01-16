using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageble
{
    [Header("Team")]
    [SerializeField]
    private Team team;

    [Header("Main properties")]
    [SerializeField]
    private float maxHp;
    [SerializeField]
    private float attack;
    [SerializeField]
    private float speed;

    [Header("Knockback")]
    [SerializeField]
    private float knockbackForce;

    [Header("Player Hitstun")]
    [SerializeField]
    private bool overridePlayerHitstun;
    [SerializeField]
    private float hitStun;


    [Header("Death Animation")]
    [SerializeField]
    private GameObject[] explosionPrefabs;


    private GameObject player;
    private PlayerController playerController;

    private Rigidbody2D rb;
    private Collider2D col;
    private PathController pathController;

    private bool isDead;
    private float hp;


    private void Awake()
    {
        player = GameObject.Find("Milk");
        player.TryGetComponent(out playerController);

        TryGetComponent(out pathController);
        TryGetComponent(out rb);
        TryGetComponent(out col);
    }

    private void Start()
    {
        hp = maxHp;
        pathController.SetSpeed(speed);
        pathController.Activate();
    }

    private DamageData BuildDamageData(Vector2 knockback)
    {
        DamageData data = new DamageData(attack, overridePlayerHitstun, hitStun);
        data.SetKnockback(knockback);
        return data;
    }

    public void Heal(float heal)
    {
        hp += heal;
        hp = Mathf.Clamp(hp, 0, maxHp);
    }
    public void Damage(IDamageble from, DamageData damageData)
    {

        hp -= damageData.damage;

        CheckAlive();

        if (damageData.damage > 0 && !isDead)
            SoundManager.Instance.PlaySound(SoundManager.Instance.monsterHit);
    }
    public Team GetTeam() => team;

    public void InstaKill(IDamageble from, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0)
    {
        pathController.Deactivate();
        isDead = true;
        Destroy(rb);
        Destroy(col);
        StartCoroutine(SuperDeathAnimation(knockback));
    }

    private IEnumerator SuperDeathAnimation(Vector2? knockback)
    {
        Vector3 direction = (Vector2)knockback;

        float baseAngleDir = Vector3.Angle(Vector3.right, direction);

        float angle = Mathf.PI * Random.Range(baseAngleDir - 15, baseAngleDir + 15) / 180;

        float timer = 0;

        direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

        Vector3 basePosition = transform.position;


        while (timer < 5f)
        {
            transform.SetPositionAndRotation(basePosition + 20 * timer * direction.normalized, Quaternion.Euler(0, 0, timer * 720));

            timer += Time.deltaTime;

            yield return null;
        }


        Destroy(gameObject);
    }


    private void CheckAlive()
    {
        if(hp <= 0)
            Death();
    }

    private void Death()
    {
        isDead = true;
        pathController.Deactivate();
        if (explosionPrefabs != null)
        {
            foreach (GameObject go in explosionPrefabs)
            {
                Instantiate(go, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(!isDead && collision.gameObject == player)
        {
            DamageData damageData = BuildDamageData(-collision.contacts[0].normal * knockbackForce);

            playerController.Damage(this, damageData);
        }
    }

    private IEnumerator IsDefeated()
    {
        yield return null;
    }

}
