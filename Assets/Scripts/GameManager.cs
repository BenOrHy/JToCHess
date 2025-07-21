using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

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

    public GameObject[] unitSet = new GameObject[40];
    public GameObject[] selectUnit = new GameObject[5];
    public int turn = 0;

    public int cost = 0;

    public int count = 2;

    private float timer = 0f;
    private float timerSum = 0f;
    private int secondsPassed = 0;
    private bool isRunning = true;

    public Slider timeSet;
    public TextMeshProUGUI CostText; 

    void Update() {
        timeSet.value = timerSum;
        CostText.text = $"<b>Cost : {GameManager.instance.cost} </b>";

        if (isRunning)
        {
            timer += Time.deltaTime;
            timerSum += Time.deltaTime;

            if (timer >= 1f)
            {
                secondsPassed++;
                timer = 0f;

                if (secondsPassed >= 15)
                {
                    Debug.Log("타이머 종료");
                    timer = 0;
                    timerSum = 0;
                    secondsPassed = 0;
                    isRunning = false;

                    Battle.instance.battleStart();
                }
            }
        }
    }
    public void Timer()
    {
       isRunning = true;
    }

    void Start()
    {
        timeSet.maxValue = 15;
        timeSet.minValue = 0;

        turnChange();
        selectChange();
    }

    public void turnChange() {
        count++;

        turn++;

        cost += 10;

        UI.instance.unitCount = 0;

        Battle.instance.BattleTime = false;

        objectPointReset();
    }

    public void selectChange() {
        int[] indices = new int[unitSet.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        for (int i = indices.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[rand];
            indices[rand] = temp;
        }

        for (int i = 0; i < 5; i++)
        {
            selectUnit[i] = unitSet[indices[i]];

            UI.instance.select[i].text = selectUnit[i].name + " cost : " + selectUnit[i].GetComponent<Unit>().cost;

            Debug.Log("선택된 오브젝트 " + i + ": " + selectUnit[i].name);
        }
    }

    public void objectPointReset()
    {
        for (int i = 0; i < UI.instance.units.Count; i++)
        {
            GameObject unit = UI.instance.units[i];
            bool isInWaiting = false;

            foreach (Transform wait in UI.instance.waiting)
            {
                if (wait != null && Vector3.Distance(wait.position, unit.transform.position) <= 1.5f)
                {
                    isInWaiting = true;
                    break;
                }
            }

            if (isInWaiting) continue;

            unit.transform.position = unit.GetComponent<Unit>().mainPos;
        }
    }

}
