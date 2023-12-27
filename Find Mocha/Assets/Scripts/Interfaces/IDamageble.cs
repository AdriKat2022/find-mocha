using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageble
{
    void Damage(IDamageble from, float damage, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0);
    void Heal(float heal);
    void InstaKill(IDamageble from, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0);
}
