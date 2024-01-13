using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HeartProjectile : MonoBehaviour
{
    private float baseSpeed = 2;
    private float baseDamage = 1;
    private float baseLifetime = 1.5f;

    private const float baseAngle = 0;
    private const float randomAngle = 0;

    private const float baseScale = 4;
    private const float randomScale = 1;

    private Vector2 baseDirection;

    public void Launch(Vector2 direction)
    {
        baseDirection = direction;
        Initialize();
    }
    public void Launch(Vector2 direction, float speed, float dmg, float span)
    {
        baseSpeed = speed;
        baseDamage = dmg;
        baseDirection = direction;
        baseLifetime = span;
        Initialize();
    }

    private void Initialize()
    {
        float ranAngle = Random.Range(baseAngle - randomAngle , baseAngle + randomAngle);
        transform.Rotate(Vector3.forward, ranAngle);
        transform.localScale = Vector3.one * Random.Range(baseScale - randomScale, baseScale + randomScale);

        StartCoroutine(ManageProjectileLifetime());
    }

    private void MoveProjectileParticle()
    {
        transform.Translate(baseSpeed * Time.deltaTime * baseDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IDamageble enemy)){

            if (enemy.GetTeam() == Team.Player)
                return;

            DamageData dmg = new(baseDamage);
            enemy.Damage(null, dmg);

            Destroy(gameObject);
        }
    }

    private IEnumerator ManageProjectileLifetime()
    {
        float timer = 0;

        while(timer < baseLifetime) {

            MoveProjectileParticle();
            
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
