
using System.Collections.Generic;

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