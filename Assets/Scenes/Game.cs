using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GameState
{
    public struct Cell
    {
        public int Type;
    }

    public const int SIZE = 10;

    System.Random random;
    Cell[] map;

    public GameState()
    {
        random = new();
        map = new Cell[SIZE * SIZE];
        for (int i = 0; i < SIZE * SIZE; i++)
            map[i].Type = next();
        Tick();
    }

    public Cell[][] Tick()
    {
        return null;
    }

    public int Get(int index)
    {
        return map[index].Type;
    }

    public void Swap(int a, int b)
    {
        (map[a], map[b]) = (map[b], map[a]);
        Tick();
    }

    int next()
    {
        return random.Next(5);
    }
}

class GameStateForGame
{
    GameState state;

    public GameStateForGame()
    {
        state = new();
    }

    public bool Tick()
    {
        return state != null;
    }

    public int Get(int index)
    {
        return state.Get(index);
    }

    public void Swap(int a, int b)
    {
        state.Swap(a, b);
    }
}

public class Game : MonoBehaviour
{
    [SerializeField]
    GameObject bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab;

    [SerializeField]
    GameObject selectionIndicator;

    static Vector3 Position(int x, int y)
    {
        Vector3 result;
        result.y = y * 2 - 9;
        result.x = x * 2 - 9;
        result.z = 0;
        return result;
    }

    GameObject[][] gemPool;

    GameStateForGame state = new();

    void Awake()
    {
        GameObject[] gemType = { bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab };
        gemPool = new GameObject[5][];
        for (int i = 0; i < 5; i++)
        {
            gemPool[i] = new GameObject[GameState.SIZE * GameState.SIZE];
            for (int j = 0; j < GameState.SIZE * GameState.SIZE; j++)
            {
                GameObject gem = gemPool[i][j] = Instantiate(gemType[i]);
                gem.AddComponent<GemInfo>();
                GemInfo info = gem.GetComponent<GemInfo>();
                info.GemType = i;
                info.X = j % GameState.SIZE;
                info.Y = j / GameState.SIZE;
                info.Game = this;
                gem.SetActive(false);
            }
        }
        selectionIndicator.SetActive(false);
    }

    void Update()
    {
        for (int i = 0; i < GameState.SIZE * GameState.SIZE; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int x = i % GameState.SIZE;
                int y = i / GameState.SIZE;
                gemPool[j][i].GetComponent<Transform>().localPosition = Position(x, y);
                gemPool[j][i].GetComponent<Collider2D>().enabled = true;
                gemPool[j][i].SetActive(state.Get(i) == j);
            }
        }
        if (currentSelection == -1)
        {
            selectionIndicator.SetActive(false);
        }
        else
        {
            int x = currentSelection % GameState.SIZE;
            int y = currentSelection / GameState.SIZE;
            Vector3 position = Position(x, y);
            position.z = -5;
            selectionIndicator.GetComponent<Transform>().localPosition = position;
            selectionIndicator.SetActive(true);
        }

        if (currentSelection != -1)
        {
            Vector3 originalWorldPosition = Camera.main.ScreenToWorldPoint(clickedPosition);
            Vector3 currentWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 originalToCurrent = currentWorldPosition - originalWorldPosition;
            int x = currentSelection % GameState.SIZE;
            int y = currentSelection / GameState.SIZE;
            GameObject gem = gemPool[state.Get(currentSelection)][currentSelection];

            if (originalToCurrent.x > 2) originalToCurrent.x = 2;
            if (originalToCurrent.x < -2) originalToCurrent.x = -2;
            if (originalToCurrent.y > 2) originalToCurrent.y = 2;
            if (originalToCurrent.y < -2) originalToCurrent.y = -2;

            bool movingX = Mathf.Abs(originalToCurrent.x) > Mathf.Abs(originalToCurrent.y);
            if (movingX)
                originalToCurrent.y = 0;
            else
                originalToCurrent.x = 0;
            if (x == 0 && originalToCurrent.x < 0)
                originalToCurrent.x = 0;
            if (x == GameState.SIZE - 1 && originalToCurrent.x > 0)
                originalToCurrent.x = 0;
            if (y == 0 && originalToCurrent.y < 0)
                originalToCurrent.y = 0;
            if (y == GameState.SIZE - 1 && originalToCurrent.y > 0)
                originalToCurrent.y = 0;

            gem.GetComponent<Transform>().localPosition += originalToCurrent + new Vector3(0, 0, -1);
            gem.GetComponent<Collider2D>().enabled = false;

            if (!(movingX && originalToCurrent.x == 0 || !movingX && originalToCurrent.y == 0))
            {
                int adjacentX = !movingX ? 0 : originalToCurrent.x > 0 ? 1 : -1;
                int adjacentY = movingX ? 0 : originalToCurrent.y > 0 ? 1 : -1;
                int adjacentIndex = (y + adjacentY) * GameState.SIZE + x + adjacentX;
                GameObject adjacentGem = gemPool[state.Get(adjacentIndex)][adjacentIndex];
                adjacentGem.GetComponent<Transform>().localPosition -= originalToCurrent;
            }
        }
    }

    int currentSelection = -1;
    Vector3 clickedPosition;

    public void onGemMouseDown(GemInfo gemInfo)
    {
        if (currentSelection == -1)
        {
            currentSelection = gemInfo.Y * GameState.SIZE + gemInfo.X;
            clickedPosition = Input.mousePosition;
        }
    }

    public void onGemMouseUp(GemInfo gemInfo)
    {
        if (currentSelection == -1) return;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitInformation = Physics2D.Raycast(worldPosition, Camera.main.transform.forward);
        if (hitInformation.collider != null)
        {
            GameObject clickedObject = hitInformation.transform.gameObject;
            GemInfo gem = clickedObject.GetComponent<GemInfo>();
            int newSelection = gem.Y * GameState.SIZE + gem.X;
            if (isAdjacent(currentSelection, newSelection))
                state.Swap(currentSelection, newSelection);
            currentSelection = -1;
        }
    }

    bool isAdjacent(int a, int b)
    {
        int ax = a % GameState.SIZE;
        int ay = a / GameState.SIZE;
        int bx = b % GameState.SIZE;
        int by = b / GameState.SIZE;
        int dx = ax - bx;
        int dy = ay - by;
        dx = dx < 0 ? -dx : dx;
        dy = dy < 0 ? -dy : dy;
        return (dx == 0 && dy == 1) || (dy == 0 && dx == 1);
    }
}
