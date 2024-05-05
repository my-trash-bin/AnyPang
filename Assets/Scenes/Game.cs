using System.Collections.Generic;
using TMPro;
using UnityEngine;

class GameState
{
    public struct Cell
    {
        public int Type;
        public int PreviousY;
        public int X;
        public int Y;
    }

    public const int SIZE = 10;

    System.Random random;
    Cell[] map;

    public GameState()
    {
        random = new();
        map = new Cell[SIZE * SIZE];
        for (int i = 0; i < SIZE * SIZE; i++)
        {
            map[i].Type = Next();
            map[i].PreviousY = i / SIZE;
            map[i].X = i % SIZE;
            map[i].Y = i / SIZE;
        }
    }

    public Cell[][] Tick()
    {
        for (int i = 0; i < SIZE * SIZE; i++)
            map[i].PreviousY = i / SIZE;

        List<Cell[]> result = new();
        bool[] isCleared = new bool[SIZE * SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE - 2; j++)
            {
                TickCheckRow(result, isCleared, j, i);
                TickCheckColumn(result, isCleared, i, j);
            }
        }

        for (int x = 0; x < SIZE; x++)
        {
            int additionalY = 0;
            for (int y = 0; y < SIZE; y++)
            {
                while (isCleared[y * SIZE + x])
                {
                    for (int i = y; i < SIZE - 1; i++)
                    {
                        map[i * SIZE + x] = map[(i + 1) * SIZE + x];
                        isCleared[i * SIZE + x] = isCleared[(i + 1) * SIZE + x];
                    }
                    map[(SIZE - 1) * SIZE + x] = new Cell
                    {
                        Type = Next(),
                        PreviousY = SIZE + additionalY++,
                    };
                    isCleared[(SIZE - 1) * SIZE + x] = false;
                }
            }
        }

        for (int i = 0; i < SIZE * SIZE; i++)
        {
            map[i].X = i % SIZE;
            map[i].Y = i / SIZE;
        }

        return result.Count == 0 ? null : result.ToArray();
    }

    private void TickCheckRow(List<Cell[]> result, bool[] isCleared, int x, int y)
    {
        if (map[y * SIZE + x].Type == map[y * SIZE + x + 1].Type && map[y * SIZE + x].Type == map[y * SIZE + x + 2].Type)
        {
            TickClear(result, isCleared, x, y);
        }
    }

    private void TickCheckColumn(List<Cell[]> result, bool[] isCleared, int x, int y)
    {
        if (map[y * SIZE + x].Type == map[(y + 1) * SIZE + x].Type && map[y * SIZE + x].Type == map[(y + 2) * SIZE + x].Type)
        {
            TickClear(result, isCleared, x, y);
        }
    }

    private void TickClear(List<Cell[]> result, bool[] isCleared, int x, int y)
    {
        bool[] isClearedNow = new bool[SIZE * SIZE];
        List<Cell> cells = new();
        TickFloodFill(result, isCleared, x, y, isClearedNow, cells);
        result.Add(cells.ToArray());
        for (int i = 0; i < SIZE * SIZE; i++)
            isCleared[i] |= isClearedNow[i];
    }

    private void TickFloodFill(List<Cell[]> result, bool[] isCleared, int x, int y, bool[] isClearedNow, List<Cell> cells)
    {
        int i = y * SIZE + x;
        if (isClearedNow[i]) return;
        isClearedNow[i] = true;
        cells.Add(map[i]);

        if (x >= 2)
            TickFloodFillCheckRow(result, isCleared, x - 2, y, isClearedNow, cells);
        if (x >= 1 && x + 1 < SIZE)
            TickFloodFillCheckRow(result, isCleared, x - 1, y, isClearedNow, cells);
        if (x + 2 < SIZE)
            TickFloodFillCheckRow(result, isCleared, x, y, isClearedNow, cells);
        if (y >= 2)
            TickFloodFillCheckColumn(result, isCleared, x, y - 2, isClearedNow, cells);
        if (y >= 1 && y + 1 < SIZE)
            TickFloodFillCheckColumn(result, isCleared, x, y - 1, isClearedNow, cells);
        if (y + 2 < SIZE)
            TickFloodFillCheckColumn(result, isCleared, x, y, isClearedNow, cells);
    }

    private void TickFloodFillCheckRow(List<Cell[]> result, bool[] isCleared, int x, int y, bool[] isClearedNow, List<Cell> cells)
    {
        if (isCleared[y * SIZE + x] || isCleared[y * SIZE + x + 1] || isCleared[y * SIZE + x + 2]) return;
        if (map[y * SIZE + x].Type == map[y * SIZE + x + 1].Type && map[y * SIZE + x].Type == map[y * SIZE + x + 2].Type)
        {
            TickFloodFill(result, isCleared, x, y, isClearedNow, cells);
            TickFloodFill(result, isCleared, x + 1, y, isClearedNow, cells);
            TickFloodFill(result, isCleared, x + 2, y, isClearedNow, cells);
        }
    }

    private void TickFloodFillCheckColumn(List<Cell[]> result, bool[] isCleared, int x, int y, bool[] isClearedNow, List<Cell> cells)
    {
        if (isCleared[y * SIZE + x] || isCleared[(y + 1) * SIZE + x] || isCleared[(y + 2) * SIZE + x]) return;
        if (map[y * SIZE + x].Type == map[(y + 1) * SIZE + x].Type && map[y * SIZE + x].Type == map[(y + 2) * SIZE + x].Type)
        {
            TickFloodFill(result, isCleared, x, y, isClearedNow, cells);
            TickFloodFill(result, isCleared, x, y + 1, isClearedNow, cells);
            TickFloodFill(result, isCleared, x, y + 2, isClearedNow, cells);
        }
    }

    public Cell Get(int index)
    {
        return map[index];
    }

    public void Swap(int a, int b)
    {
        (map[a], map[b]) = (map[b], map[a]);
        map[a].X = a % SIZE;
        map[a].Y = a / SIZE;
        map[b].X = b % SIZE;
        map[b].Y = b / SIZE;
    }

    int Next()
    {
        return random.Next(5);
    }
}
[RequireComponent(typeof(AudioSource))] // automatically add atributes
public class Game : MonoBehaviour
{
    [SerializeField]
    GameObject bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab;
    [SerializeField]
    AudioClip pangPrefab;
    AudioSource audioData;
    [SerializeField]
    GameObject selectionIndicator;

    [SerializeField]
    GameObject effectPrefab;

    [SerializeField]
    GameObject ScoreText;

    const float FALLING_ANIMATION_DURATION = 0.5f;

    static float FallingAnimationDistance(float elapsedTime)
    {
        float x = elapsedTime / FALLING_ANIMATION_DURATION;
        return x * x * 10;
    }

    static Vector3 Position(int x, float y)
    {
        Vector3 result;
        result.y = y * 2 - 9;
        result.x = x * 2 - 9;
        result.z = 0;
        return result;
    }

    GameObject[][] gemPool;
    GameObject[] gemsForAnimation;
    GameObject[] particles;

    GameState state = new();
    bool isAnimating;
    float animationElapsedTime;
    GameState.Cell[][] removedCellGroups = null;

    long score;

    long Score
    {
        get { return score; }
        set
        {
            score = value;
            ScoreText.GetComponent<TextMeshPro>().text = "Score\n" + score.ToString("N0");
        }
    }

    void Awake()
    {

        audioData = GetComponent<AudioSource>();
        audioData.clip = pangPrefab;
        GameObject[] gemType = { bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab };
        gemPool = new GameObject[5][];
        gemsForAnimation = new GameObject[5];
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
            GameObject forAnimation = gemsForAnimation[i] = Instantiate(gemType[i]);
            forAnimation.GetComponent<Collider2D>().enabled = false;
            forAnimation.SetActive(false);
        }
        selectionIndicator.SetActive(false);
        particles = new GameObject[GameState.SIZE * GameState.SIZE];
        for (int i = 0; i < GameState.SIZE * GameState.SIZE; i++)
        {
            GameObject particle = particles[i] = Instantiate(effectPrefab);
            particle.GetComponent<Transform>().localPosition = Position(i % GameState.SIZE, i / GameState.SIZE);
            particle.SetActive(false);
        }
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
                gemPool[j][i].GetComponent<SpriteRenderer>().enabled = true;
                gemPool[j][i].SetActive(state.Get(i).Type == j);
            }
        }

        bool[] particleEnabled = new bool[GameState.SIZE * GameState.SIZE];
        if (removedCellGroups != null)
            foreach (GameState.Cell[] cells in removedCellGroups)
                foreach (GameState.Cell cell in cells)
                    particleEnabled[cell.Y * GameState.SIZE + cell.X] = true;
        for (int i = 0; i < GameState.SIZE * GameState.SIZE; i++)
            particles[i].SetActive(particleEnabled[i]);

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
        for (int i = 0; i < 5; i++)
        {
            gemsForAnimation[i].SetActive(false);
        }

        if (currentSelection != -1)
        {
            Vector3 originalWorldPosition = Camera.main.ScreenToWorldPoint(clickedPosition);
            Vector3 currentWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 originalToCurrent = currentWorldPosition - originalWorldPosition;
            int x = currentSelection % GameState.SIZE;
            int y = currentSelection / GameState.SIZE;
            GameObject gem = gemPool[state.Get(currentSelection).Type][currentSelection];

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

            gem.GetComponent<Transform>().localPosition += originalToCurrent + new Vector3(0, 0, -2);
            gem.GetComponent<Collider2D>().enabled = false;

            if (originalToCurrent.x != 0 || originalToCurrent.y != 0)
            {
                int adjacentX = x + (!movingX ? 0 : originalToCurrent.x > 0 ? 1 : -1);
                int adjacentY = y + (movingX ? 0 : originalToCurrent.y > 0 ? 1 : -1);
                int adjacentIndex = adjacentY * GameState.SIZE + adjacentX;
                GameObject gemForAnimation = gemsForAnimation[state.Get(adjacentIndex).Type];
                gemForAnimation.GetComponent<Transform>().localPosition = Position(adjacentX, adjacentY) - originalToCurrent + new Vector3(0, 0, -1);
                gemForAnimation.SetActive(true);
                GameObject adjacentGem = gemPool[state.Get(adjacentIndex).Type][adjacentIndex];
                adjacentGem.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        if (isAnimating)
        {
            animationElapsedTime += Time.deltaTime;

            if (animationElapsedTime > FALLING_ANIMATION_DURATION)
            {
                isAnimating = false;
            }
            else
            {
                for (int i = 0; i < GameState.SIZE * GameState.SIZE; i++)
                {
                    float currentPosition = state.Get(i).PreviousY - FallingAnimationDistance(animationElapsedTime);
                    float realPosition = Mathf.Max(i / GameState.SIZE, currentPosition);
                    gemPool[state.Get(i).Type][i].GetComponent<Transform>().localPosition = Position(i % GameState.SIZE, realPosition);
                }
            }
        }
        else
            Tick();
    }

    int currentSelection = -1;
    Vector3 clickedPosition;

    public void onGemMouseDown(GemInfo gemInfo)
    {
        if (isAnimating) return;
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
            {
                state.Swap(currentSelection, newSelection);
                if (!Tick())
                    state.Swap(currentSelection, newSelection);
            }
            currentSelection = -1;
        }
    }

    private bool Tick()
    {
        removedCellGroups = state.Tick();
        if (removedCellGroups != null)
        {
            isAnimating = true;
            audioData.volume = removedCellGroups.Length / 3.0f;
            audioData.Play(0);

            long scoreIncrement = 0;
            foreach (GameState.Cell[] cells in removedCellGroups)
                scoreIncrement += cells.Length * cells.Length;
            Score += scoreIncrement;

            animationElapsedTime = 0;
            return true;
        }
        return false;
    }

    private static bool isAdjacent(int a, int b)
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
