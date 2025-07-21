using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    public static Battle instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public List<Vector3> spawnPoints = new List<Vector3>();
    public bool BattleTime = false;

    public void battleStart() {
        BattleTime = true;
        posSet();
        SpawnUnits();
    }

    void SpawnUnits()
    {
        for (int i = 0; i < GameManager.instance.turn; i++)
        {
            int unitIndex = Random.Range(0, GameManager.instance.unitSet.Length);
            GameObject selectedUnit = GameManager.instance.unitSet[unitIndex];

            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector3 spawnPos = spawnPoints[spawnIndex];

            GameObject unit = Instantiate(selectedUnit, spawnPos, Quaternion.identity);

            unit.GetComponent<Unit>().star = Random.Range(1, 4);
            unit.tag = "enemy";
        }
    }

    public void posSet() {
        Vector3 firstPoint = new Vector3(3.5f, -1.5f, 0);

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                spawnPoints.Add(firstPoint + new Vector3(x, y, 0));
            }
        }
    }

    void FixedUpdate()
    {
        
    }

    public GameObject bulletPrefab;
    public GameObject hitEffectPrefab;

    public IEnumerator attack(GameObject attacker, GameObject damager)
    {
        Unit attackerUnit = attacker.GetComponent<Unit>();
        Unit damagerUnit = damager.GetComponent<Unit>();

        attackerUnit.atkCool = false;

        if (attackerUnit.range == 1 && Battle.instance.bulletPrefab != null)
        {
            GameObject bullet = Instantiate(Battle.instance.bulletPrefab, attacker.transform.position, Quaternion.identity);

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(damager, attackerUnit.atk, hitEffectPrefab);
            }
        }
        else
        {
            damagerUnit.hp -= attackerUnit.atk;

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, damager.transform.position, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(attackerUnit.atkSpd);
        attackerUnit.atkCool = true;
    }

}
