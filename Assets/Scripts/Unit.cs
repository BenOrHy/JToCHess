using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public int star = 1;
    public int cost = 1; // 1 2 3 4 5
    public List<string> synergey = new List<string>();

    public int MaxHp = 100;
    public int hp = 100;
    public int atk = 20;
    public float atkSpd = 1f;
    public float spd = 0.9f;
    public int range = 0; //0 - 근거리, 1 - 원거리

    private bool isDragging = false;
    private Vector3 offset;

    public List<Vector3> snapPoints = new List<Vector3>();
    public float snapRange = 0.5f;

    public Vector3 mainPos;
    private SpriteRenderer spriteRenderer;

    public void UpdateColor()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        switch (cost)
        {
            case 1:
                spriteRenderer.color = Color.gray;
                break;
            case 2:
                spriteRenderer.color = Color.green;
                break;
            case 3:
                spriteRenderer.color = Color.blue;
                break;
            case 4:
                spriteRenderer.color = new Color(0.5f, 0f, 0.5f);
                break;
            case 5:
                spriteRenderer.color = Color.yellow;
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
        }
    }

    void Start()
    {
        if (range == 0)
        {
            MaxHp = 110;
            atk = 20;
        }
        else if (range == 1){
            MaxHp = 90;
            atk = 15;
        }

        MaxHp = MaxHp + cost * 10 + star * 30;
        hp = hp + cost * 10 + star * 30;
        atk = atk + cost + star * 3;
    }

    private GameObject targetEnemy;
    public float stopDistance = 1.5f;
    public bool atkCool = true;

    void Update()
    {
        if (!Battle.instance.BattleTime) return;

        foreach (Transform wait in UI.instance.waiting)
        {
            if (wait != null && Vector3.Distance(wait.position, transform.position) <= 1.5f)
                return;
        }

        if (range == 0)
        {
            FindClosestEnemy();

            if (targetEnemy != null)
            {
                float distance = Vector3.Distance(transform.position, targetEnemy.transform.position);

                if (distance > stopDistance)
                {
                    Vector3 dir = (targetEnemy.transform.position - transform.position).normalized;
                    transform.position += dir * spd * Time.deltaTime;
                }
                else
                {
                    if (atkCool)
                        StartCoroutine(Battle.instance.attack(gameObject, targetEnemy));

                    CheckEnemyDeath();
                }
            }
        }
        else if (range == 1)
        {
            FindClosestEnemy();

            if (targetEnemy != null)
            {
                if (atkCool)
                    StartCoroutine(Battle.instance.attack(gameObject, targetEnemy));

                CheckEnemyDeath();
            }
        }
    }

    private void CheckEnemyDeath()
    {
        if (targetEnemy == null) return;

        Unit enemyUnit = targetEnemy.GetComponent<Unit>();
        if (enemyUnit != null && enemyUnit.hp <= 0)
        {
            targetEnemy.SetActive(false);
            GameObject[] enemies;
            UI.instance.RemoveUnit(targetEnemy);

            if (tag == "allie")
            {
                enemies = GameObject.FindGameObjectsWithTag("enemy");
                if (enemies.Length == 0)
                {
                    Debug.Log("game clear");
                    GameManager.instance.turnChange();
                    GameManager.instance.Timer();
                }
            }
            else if (tag == "enemy")
            {
                enemies = GameObject.FindGameObjectsWithTag("allie");
                if (enemies.Length == 0)
                {
                    Debug.Log("game over");
                }
            }
        }
    }


    void FindClosestEnemy()
    {
        GameObject[] enemies = new GameObject[48];

        if (tag == "allie") {
            enemies = GameObject.FindGameObjectsWithTag("enemy");
        }
        else if(tag == "enemy"){
            enemies = GameObject.FindGameObjectsWithTag("allie");
        }

        float minDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            Vector3 enemyPos = enemy.transform.position;

            bool inWaitingRange = false;
            foreach (Transform wait in UI.instance.waiting)
            {
                if (wait != null && Vector3.Distance(wait.position, enemyPos) <= 1.5f)
                {
                    inWaitingRange = true;
                    break;
                }
            }
            if (inWaitingRange) continue;

            float distance = Vector3.Distance(transform.position, enemyPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        targetEnemy = closestEnemy;
    }

    void OnMouseDown()
    {
        if (!Battle.instance.BattleTime)
        {
            isDragging = true;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
        }
    }

    void OnMouseDrag()
    {
        if (!Battle.instance.BattleTime)
        {
            if (isDragging)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;
            }
        }
    }

    void OnMouseUp()
    {
        if (!Battle.instance.BattleTime)
        {
            isDragging = false;

            Vector3 closest = GetClosestSnapPointInRange();

            if (closest != Vector3.zero)
            {
                int count = 0;
                GameObject[] allUnits = GameObject.FindGameObjectsWithTag("allie");

                foreach (GameObject unit in allUnits)
                {
                    if (unit == gameObject) continue;

                    Unit u = unit.GetComponent<Unit>();
                    if (u != null && u.mainPos == closest)
                    {
                        count++;
                    }
                }

                if (count < 4)
                {
                    transform.position = closest;
                    mainPos = closest;
                    return;
                }
            }

            foreach (Transform wait in UI.instance.waiting)
            {
                bool occupied = false;

                foreach (GameObject unit in UI.instance.units)
                {
                    if (unit != gameObject && Vector3.Distance(unit.transform.position, wait.position) < 0.1f)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                {
                    transform.position = wait.position;
                    mainPos = wait.position;
                    return;
                }
            }

            transform.position = mainPos;
        }
    }



    Vector3 GetClosestSnapPointInRange()
    {
        Vector3 closest = new Vector3(0, 0, 0);
        float minDist = snapRange;
        Vector3 currentPos = transform.position;

        foreach (Vector3 point in snapPoints)
        {
            float dist = Vector3.Distance(currentPos, point);
            if (dist <= minDist)
            {
                minDist = dist;
                closest = point;
            }
        }

        return closest;
    }
}
