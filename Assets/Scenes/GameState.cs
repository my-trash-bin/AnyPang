
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

    /// <summary>
    /// Processes the game state
    /// </summary>
    /// <returns>Removed cell groups, or null if nothing removed</returns>
    public Cell[][] Tick()
    {
        for (int i = 0; i < SIZE * SIZE; i++)
            map[i].PreviousY = i / SIZE;

        List<Cell[]> result = new();
        bool[] isRemovedInThisTick = new bool[SIZE * SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE - 2; j++)
            {
                TickCheckRow(result, isRemovedInThisTick, j, i);
                TickCheckColumn(result, isRemovedInThisTick, i, j);
            }
        }

        for (int x = 0; x < SIZE; x++)
        {
            int additionalY = 0;
            for (int y = 0; y < SIZE; y++)
            {
                while (isRemovedInThisTick[y * SIZE + x])
                {
                    for (int i = y; i < SIZE - 1; i++)
                    {
                        map[i * SIZE + x] = map[(i + 1) * SIZE + x];
                        isRemovedInThisTick[i * SIZE + x] = isRemovedInThisTick[(i + 1) * SIZE + x];
                    }
                    map[(SIZE - 1) * SIZE + x] = new Cell
                    {
                        Type = Next(),
                        PreviousY = SIZE + additionalY++,
                    };
                    isRemovedInThisTick[(SIZE - 1) * SIZE + x] = false;
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

    private void TickCheckRow(List<Cell[]> result, bool[] isRemovedInThisTick, int x, int y)
    {
        if (map[y * SIZE + x].Type == map[y * SIZE + x + 1].Type && map[y * SIZE + x].Type == map[y * SIZE + x + 2].Type)
        {
            TickRemoveGroup(result, isRemovedInThisTick, x, y);
        }
    }

    private void TickCheckColumn(List<Cell[]> result, bool[] isRemovedInThisTick, int x, int y)
    {
        if (map[y * SIZE + x].Type == map[(y + 1) * SIZE + x].Type && map[y * SIZE + x].Type == map[(y + 2) * SIZE + x].Type)
        {
            TickRemoveGroup(result, isRemovedInThisTick, x, y);
        }
    }

    private void TickRemoveGroup(List<Cell[]> result, bool[] isRemovedInThisTick, int x, int y)
    {
        bool[] isRemovedInThisGroup = new bool[SIZE * SIZE];
        List<Cell> cells = new();
        TickFloodFill(result, isRemovedInThisTick, x, y, isRemovedInThisGroup, cells);
        if (cells.Count != 0)
            result.Add(cells.ToArray());
        for (int i = 0; i < SIZE * SIZE; i++)
            isRemovedInThisTick[i] |= isRemovedInThisGroup[i];
    }

    private void TickFloodFill(List<Cell[]> result, bool[] isRemovedInThisTick, int x, int y, bool[] isRemovedInThisGroup, List<Cell> cells)
    {
        int i = y * SIZE + x;
        if (isRemovedInThisGroup[i] || isRemovedInThisTick[i]) return;
        isRemovedInThisGroup[i] = true;
        cells.Add(map[i]);

        if (x >= 2)
            TickFloodFillCheckRow(result, isRemovedInThisTick, x - 2, y, isRemovedInThisGroup, cells);
        if (x >= 1 && x + 1 < SIZE)
            TickFloodFillCheckRow(result, isRemovedInThisTick, x - 1, y, isRemovedInThisGroup, cells);
        if (x + 2 < SIZE)
            TickFloodFillCheckRow(result, isRemovedInThisTick, x, y, isRemovedInThisGroup, cells);
        if (y >= 2)
            TickFloodFillCheckColumn(result, isRemovedInThisTick, x, y - 2, isRemovedInThisGroup, cells);
        if (y >= 1 && y + 1 < SIZE)
            TickFloodFillCheckColumn(result, isRemovedInThisTick, x, y - 1, isRemovedInThisGroup, cells);
        if (y + 2 < SIZE)
            TickFloodFillCheckColumn(result, isRemovedInThisTick, x, y, isRemovedInThisGroup, cells);
    }

    private void TickFloodFillCheckRow(List<Cell[]> result, bool[] isRemovedInThisTick, int x, int y, bool[] isRemovedInThisGroup, List<Cell> cells)
    {
        if (isRemovedInThisTick[y * SIZE + x] || isRemovedInThisTick[y * SIZE + x + 1] || isRemovedInThisTick[y * SIZE + x + 2]) return;
        if (map[y * SIZE + x].Type == map[y * SIZE + x + 1].Type && map[y * SIZE + x].Type == map[y * SIZE + x + 2].Type)
        {
            TickFloodFill(result, isRemovedInThisTick, x, y, isRemovedInThisGroup, cells);
            TickFloodFill(result, isRemovedInThisTick, x + 1, y, isRemovedInThisGroup, cells);
            TickFloodFill(result, isRemovedInThisTick, x + 2, y, isRemovedInThisGroup, cells);
        }
    }

    private void TickFloodFillCheckColumn(List<Cell[]> result, bool[] isRemovedInThisTick, int x, int y, bool[] isRemovedInThisGroup, List<Cell> cells)
    {
        if (isRemovedInThisTick[y * SIZE + x] || isRemovedInThisTick[(y + 1) * SIZE + x] || isRemovedInThisTick[(y + 2) * SIZE + x]) return;
        if (map[y * SIZE + x].Type == map[(y + 1) * SIZE + x].Type && map[y * SIZE + x].Type == map[(y + 2) * SIZE + x].Type)
        {
            TickFloodFill(result, isRemovedInThisTick, x, y, isRemovedInThisGroup, cells);
            TickFloodFill(result, isRemovedInThisTick, x, y + 1, isRemovedInThisGroup, cells);
            TickFloodFill(result, isRemovedInThisTick, x, y + 2, isRemovedInThisGroup, cells);
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