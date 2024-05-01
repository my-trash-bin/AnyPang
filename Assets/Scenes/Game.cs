using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Transform bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab;

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

    Transform[][] gemPool;

    int[][] map;

    void Awake()
    {
        Transform[] gemType = { bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab };
        gemPool = new Transform[5][];
        for (int i = 0; i < 5; i++)
        {
            gemPool[i] = new Transform[SIZE * SIZE];
            for (int j = 0; j < SIZE * SIZE; j++)
            {
                Transform gem = gemPool[i][j] = Instantiate(gemType[i]);
                gem.localPosition = Position(j % SIZE, j / SIZE);
                gem.gameObject.SetActive(false);
            }
        }
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
                gemPool[j][i].gameObject.SetActive(map[i / SIZE][i % SIZE] == j);
        }
    }
}
