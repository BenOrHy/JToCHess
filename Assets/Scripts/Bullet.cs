using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private GameObject target;
    private int damage;
    private GameObject hitEffectPrefab;

    public float speed = 10f;

    public void SetTarget(GameObject targetObj, int dmg, GameObject hitEffect)
    {
        target = targetObj;
        damage = dmg;
        hitEffectPrefab = hitEffect;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.transform.position) < 0.2f)
        {
            Unit unit = target.GetComponent<Unit>();
            if (unit != null)
            {
                unit.hp -= damage;

                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, target.transform.position, Quaternion.identity);
                }
            }

            Destroy(gameObject);
        }
    }
}
