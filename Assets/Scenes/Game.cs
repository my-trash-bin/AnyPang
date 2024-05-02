using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    GameObject bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab;

    [SerializeField]
    GameObject selectionIndicator;

    const int SIZE = 10;
    System.Random random = new();

    static Vector3 Position(int x, int y)
    {
        Vector3 result;
        result.y = y * 2 - 9;
        result.x = x * 2 - 9;
        result.z = 0;
        return result;
    }

    GameObject[][] gemPool;

    int[] map;

    void Awake()
    {
        GameObject[] gemType = { bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab };
        gemPool = new GameObject[5][];
        for (int i = 0; i < 5; i++)
        {
            gemPool[i] = new GameObject[SIZE * SIZE];
            for (int j = 0; j < SIZE * SIZE; j++)
            {
                GameObject gem = gemPool[i][j] = Instantiate(gemType[i]);
                gem.AddComponent<GemInfo>();
                GemInfo info = gem.GetComponent<GemInfo>();
                info.GemType = i;
                info.X = j % SIZE;
                info.Y = j / SIZE;
                info.Game = this;
                gem.GetComponent<Transform>().localPosition = Position(j % SIZE, j / SIZE);
                gem.SetActive(false);
            }
        }
        selectionIndicator.SetActive(false);
    }

    void Start()
    {
        map = new int[SIZE * SIZE];
        for (int i = 0; i < SIZE * SIZE; i++)
            map[i] = random.Next(5);
    }

    void Update()
    {
        for (int i = 0; i < SIZE * SIZE; i++)
        {
            for (int j = 0; j < 5; j++)
                gemPool[j][i].SetActive(map[i] == j);
        }
        if (currentSelection == -1)
        {
            selectionIndicator.SetActive(false);
        }
        else
        {
            int x = currentSelection % SIZE;
            int y = currentSelection / SIZE;
            Vector3 position = Position(x, y);
            position.z = -5;
            selectionIndicator.GetComponent<Transform>().localPosition = position;
            selectionIndicator.SetActive(true);
        }
    }

    int currentSelection = -1;

    public void onGemClick(GemInfo gemInfo)
    {
        if (currentSelection == -1)
        {
            currentSelection = gemInfo.Y * SIZE + gemInfo.X;
        }
        else
        {
            int newSelection = gemInfo.Y * SIZE + gemInfo.X;
            if (isAdjacent(currentSelection, newSelection))
                (map[currentSelection], map[newSelection]) = (map[newSelection], map[currentSelection]);
            currentSelection = -1;
        }
    }

    bool isAdjacent(int a, int b)
    {
        int ax = a % SIZE;
        int ay = a / SIZE;
        int bx = b % SIZE;
        int by = b / SIZE;
        int dx = ax - bx;
        int dy = ay - by;
        dx = dx < 0 ? -dx : dx;
        dy = dy < 0 ? -dy : dy;
        return (dx == 0 && dy == 1) || (dy == 0 && dx == 1);
    }
}
