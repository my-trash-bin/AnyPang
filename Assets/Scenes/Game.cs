using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))] // automatically add atributes
public class Game : MonoBehaviour
{
    [SerializeField]
    GameObject bluePrefab, redPrefab, greenPrefab, yellowPrefab, blackPrefab;

    [SerializeField]
    AudioClip pangPrefab;
    AudioSource audioData;

    [SerializeField]
    GameObject effectPrefab;

    [SerializeField]
    GameObject ScoreText;

    [SerializeField]
    GameObject MainCamera;

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
    bool isFalling;
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
        UpdateCells();
        UpdateDragAnimation();
        UpdateEffects();
        if (isFalling)
            UpdateFallingAnimation();
        else
            UpdateGame();
    }

    void UpdateCells()
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

        if (currentSelection != -1)
        {
            int x = currentSelection % GameState.SIZE;
            int y = currentSelection / GameState.SIZE;
            Vector3 position = Position(x, y);
            position.z = -5;
        }
        for (int i = 0; i < 5; i++)
        {
            gemsForAnimation[i].SetActive(false);
        }
    }

    void UpdateEffects()
    {
        bool[] particleEnabled = new bool[GameState.SIZE * GameState.SIZE];
        if (removedCellGroups != null)
            foreach (GameState.Cell[] cells in removedCellGroups)
                foreach (GameState.Cell cell in cells)
                    particleEnabled[cell.Y * GameState.SIZE + cell.X] = true;
        for (int i = 0; i < GameState.SIZE * GameState.SIZE; i++)
            particles[i].SetActive(particleEnabled[i]);
    }

    void UpdateDragAnimation()
    {
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
    }

    void UpdateFallingAnimation()
    {
        animationElapsedTime += Time.deltaTime;

        if (animationElapsedTime > FALLING_ANIMATION_DURATION)
        {
            isFalling = false;
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

    void UpdateGame()
    {
        Tick();
    }

    int currentSelection = -1;
    Vector3 clickedPosition;

    public void onGemMouseDown(GemInfo gemInfo)
    {
        if (isFalling) return;
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

    int combo;

    private bool Tick()
    {
        removedCellGroups = state.Tick();
        if (removedCellGroups != null)
        {
            isFalling = true;
            audioData.volume = removedCellGroups.Length / 3.0f;
            audioData.Play(0);

            combo++;

            Debug.Log(string.Join('|', removedCellGroups.Select(group => string.Join(',', group.Select(cell => cell.X + ":" + cell.Y)))));

            long scoreIncrement = 0;
            foreach (GameState.Cell[] cells in removedCellGroups)
                scoreIncrement += cells.Length * cells.Length;
            Score += scoreIncrement * combo;

            MainCamera.GetComponent<LivelyCamera>().PushXZ(new Vector2(combo, scoreIncrement));
            MainCamera.GetComponent<LivelyCamera>().JostleY();

            animationElapsedTime = 0;
            return true;
        }
        else
        {
            combo = 0;
            return false;
        }
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
