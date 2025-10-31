using UnityEngine;

public class TakeDamageDie : MonoBehaviour
{
    public int health = 10;
    bool isDead = false;

    void Update()
    {
        if (!isDead && health <= 0) Die();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other) { TryApplyBullet(other.gameObject); }
    void OnCollisionEnter(Collision col) { TryApplyBullet(col.collider.gameObject); }

    void TryApplyBullet(GameObject go)
    {
        var bullet = go.GetComponent<BulletDamage>();
        if (bullet != null || go.name == "P_LPSP_PROJ_Bullet_01")
        {
            TakeDamage(bullet ? bullet.damage : 5);
            if (bullet == null) Destroy(go);
        }
    }
}
