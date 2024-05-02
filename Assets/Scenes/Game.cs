using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GemInfo : MonoBehaviour
{
    public int GemType;
    public int X;
    public int Y;
}

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

    int[][] map;

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
                gem.GetComponent<Transform>().localPosition = Position(j % SIZE, j / SIZE);
                gem.SetActive(false);
            }
        }
        selectionIndicator.SetActive(false);
    }

    void Start()
    {
        map = new int[SIZE][];
        for (int i = 0; i < SIZE; i++)
        {
            map[i] = new int[SIZE];
            for (int j = 0; j < SIZE; j++)
                map[i][j] = random.Next(5);
        }
    }

    void Update()
    {
        for (int i = 0; i < SIZE * SIZE; i++)
        {
            for (int j = 0; j < 5; j++)
                gemPool[j][i].SetActive(map[i / SIZE][i % SIZE] == j);
        }
    }
}
