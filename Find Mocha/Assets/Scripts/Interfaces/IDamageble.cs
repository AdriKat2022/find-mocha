using UnityEngine;

public struct DamageData
{
    public float damage;
    public Vector2? knockback;
    public float knockbackAngle;
    public float knockbackForce;

    public bool overrideHitstun;
    public float hitstun;

    public DamageData(float dmg, bool overrideHitstun = false, float hitstun = 0)
    {
        damage = dmg;

        knockback = null;
        knockbackAngle = 0;
        knockbackForce = 0;

        this.overrideHitstun = overrideHitstun;
        this.hitstun = hitstun;
    }

    public void SetKnockback(Vector2 knockback)
    {
        this.knockback = knockback;
        knockbackAngle = Vector2.Angle(Vector2.right, knockback);
        knockbackForce = knockback.magnitude;
    }
    /// <param name="force"></param>
    /// <param name="angle">In degrees</param>
    public void SetKnockback(float force, float angle)
    {
        knockbackForce = force;
        knockbackAngle = angle;
        knockback = new Vector2(Mathf.Cos(angle * Mathf.PI / 180), Mathf.Sin(angle * Mathf.PI / 180)) * force;
    }
}

public enum Team
{
    Player,
    Enemy,
    NeutralObstacle
}

public interface IDamageble
{
    void Damage(IDamageble from, DamageData damageData);
    void Heal(float heal, bool isSilent = false);
    void InstaKill(IDamageble from, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0);

    Team GetTeam();
}
