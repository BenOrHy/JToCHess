using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{

    public static UI instance = null;

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

    public List<GameObject> units = new List<GameObject>();
    public int unitCount = 0;

    public TextMeshProUGUI[] select = new TextMeshProUGUI[5];
    public Transform[] waiting = new Transform[5];
    public int waitingPannelCount = 0;

    public void Select1() => SelectUnit(0);
    public void Select2() => SelectUnit(1);
    public void Select3() => SelectUnit(2);
    public void Select4() => SelectUnit(3);
    public void Select5() => SelectUnit(4);

    private void SelectUnit(int index)
    {
        if (unitCount < 3 + GameManager.instance.turn) {
            GameObject baseUnitPrefab = GameManager.instance.selectUnit[index];
            string unitName = baseUnitPrefab.name;

            List<GameObject> sameUnits = new List<GameObject>();
            foreach (GameObject unit in units)
            {
                Unit u = unit.GetComponent<Unit>();
                if (u != null && unit.name.Contains(unitName))
                {
                    sameUnits.Add(unit);
                }
            }

            int oneStarCount = 0;
            int twoStarCount = 0;

            foreach (GameObject go in sameUnits)
            {
                Unit u = go.GetComponent<Unit>();
                if (u.star == 1) oneStarCount++;
                else if (u.star == 2) twoStarCount++;
            }

            int unitCost = baseUnitPrefab.GetComponent<Unit>().cost;
            if (GameManager.instance.cost < unitCost) return;

            if (waitingPannelCount >= waiting.Length) return;

            if (oneStarCount + 1 >= 2 && twoStarCount == 0)
            {
                DeleteMatchingUnits(unitName, 2, 0);
                SpawnUnit(baseUnitPrefab, 2);
            }

            else if (oneStarCount >= 1 && twoStarCount >= 1)
            {
                DeleteMatchingUnits(unitName, 1, 1);
                SpawnUnit(baseUnitPrefab, 3);
            }
            else
            {
                SpawnUnit(baseUnitPrefab, 1);
            }

            GameManager.instance.cost -= unitCost;
            GameManager.instance.selectChange();
        }
    }


    private void DeleteMatchingUnits(string baseName, int oneStarToRemove, int twoStarToRemove)
    {
        int oneRemoved = 0, twoRemoved = 0;

        for (int i = units.Count - 1; i >= 0; i--)
        {
            GameObject unit = units[i];
            if (!unit.name.Contains(baseName)) continue;

            Unit u = unit.GetComponent<Unit>();
            if (u.star == 1 && oneRemoved < oneStarToRemove)
            {
                Destroy(unit);
                units.RemoveAt(i);
                oneRemoved++;
            }
            else if (u.star == 2 && twoRemoved < twoStarToRemove)
            {
                Destroy(unit);
                units.RemoveAt(i);
                twoRemoved++;
            }

            if (oneRemoved == oneStarToRemove && twoRemoved == twoStarToRemove)
                break;
        }
    }

    private void SpawnUnit(GameObject basePrefab, int starLevel)
    {
        int spawnIndex = -1;
        for (int i = 0; i < waiting.Length; i++)
        {
            Vector3 pos = waiting[i].position;

            bool occupied = false;
            foreach (GameObject unit in units)
            {
                if (unit != null && Vector3.Distance(unit.transform.position, pos) < 0.1f)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                spawnIndex = i;
                break;
            }
        }

        if (spawnIndex == -1)
        {
            Debug.LogWarning("¸ðµç ´ë±â Ä­ÀÌ °¡µæ Ã¡½À´Ï´Ù!");
            return;
        }

        GameObject newUnit = Instantiate(basePrefab);
        Unit u = newUnit.GetComponent<Unit>();
        point(u);

        u.star = starLevel;
        u.UpdateColor();
        newUnit.name = basePrefab.name;

        newUnit.transform.position = waiting[spawnIndex].position;
        newUnit.tag = "allie";

        units.Add(newUnit);
        unitCount++;
    }

    public void RemoveUnit(GameObject unit)
    {
        if (units.Contains(unit))
        {
            units.Remove(unit);
            Destroy(unit);
            unitCount--;
        }
    }


    public void point(Unit u) {
        Vector3 firstPoint = new Vector3(-0.5f, -1.5f, 0);


        u.snapPoints.Clear();

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                u.snapPoints.Add(firstPoint + new Vector3(x, y, 0));
            }
        }
    }


    public void resetButton()
    {
        
    }

}
