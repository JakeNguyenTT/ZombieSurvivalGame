using System.Collections.Generic;
using UnityEngine;

// Grid over the XZ plane where every cell points toward the cheapest route to one target.
// One build serves any number of walkers, so hundreds of enemies can path around obstacles
// for the price of a single search.
public class FlowField
{
    private const float Diagonal = 1.41421356f;

    private static readonly int[] s_Dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
    private static readonly int[] s_Dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

    public readonly int Width;
    public readonly int Height;
    public readonly float CellSize;

    // World XZ of the min corner of cell (0, 0)
    public Vector2 Origin { get; set; }

    private readonly bool[] m_Blocked;
    private readonly float[] m_Cost;
    private readonly Vector2[] m_Direction;
    private readonly MinHeap m_Open;

    public FlowField(int width, int height, float cellSize)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        m_Blocked = new bool[width * height];
        m_Cost = new float[width * height];
        m_Direction = new Vector2[width * height];
        m_Open = new MinHeap(width * height);
    }

    // Moves the grid so `position` sits in the middle cell, snapped to whole cells
    public void CenterOn(Vector2 position)
    {
        float snappedX = Mathf.Floor(position.x / CellSize) * CellSize;
        float snappedY = Mathf.Floor(position.y / CellSize) * CellSize;
        Origin = new Vector2(snappedX - (Width / 2) * CellSize, snappedY - (Height / 2) * CellSize);
    }

    public bool TryGetCell(Vector2 position, out int x, out int y)
    {
        x = Mathf.FloorToInt((position.x - Origin.x) / CellSize);
        y = Mathf.FloorToInt((position.y - Origin.y) / CellSize);
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public Vector2 CellCenter(int x, int y) => Origin + new Vector2((x + 0.5f) * CellSize, (y + 0.5f) * CellSize);

    public void ClearBlocked() => System.Array.Clear(m_Blocked, 0, m_Blocked.Length);

    // Blocks every cell whose center lies within `radius` of `center`
    public void BlockCircle(Vector2 center, float radius)
    {
        int minX = Mathf.FloorToInt((center.x - radius - Origin.x) / CellSize);
        int maxX = Mathf.FloorToInt((center.x + radius - Origin.x) / CellSize);
        int minY = Mathf.FloorToInt((center.y - radius - Origin.y) / CellSize);
        int maxY = Mathf.FloorToInt((center.y + radius - Origin.y) / CellSize);
        float radiusSqr = radius * radius;
        for (int y = Mathf.Max(0, minY); y <= Mathf.Min(Height - 1, maxY); y++)
            for (int x = Mathf.Max(0, minX); x <= Mathf.Min(Width - 1, maxX); x++)
                if ((CellCenter(x, y) - center).sqrMagnitude <= radiusSqr)
                    m_Blocked[y * Width + x] = true;
        // A radius smaller than half a cell still blocks the cell it sits in
        if (TryGetCell(center, out int cx, out int cy))
            m_Blocked[cy * Width + cx] = true;
    }

    public bool IsBlocked(int x, int y) => m_Blocked[y * Width + x];

    // Dijkstra outward from the target, then each cell points at its cheapest neighbour
    public void Build(Vector2 target)
    {
        for (int i = 0; i < m_Cost.Length; i++)
        {
            m_Cost[i] = float.PositiveInfinity;
            m_Direction[i] = Vector2.zero;
        }
        if (!TryGetCell(target, out int tx, out int ty)) return;

        int targetIndex = ty * Width + tx;
        m_Blocked[targetIndex] = false; // a player hugging a rock must still be reachable
        m_Cost[targetIndex] = 0f;
        m_Open.Clear();
        m_Open.Push(targetIndex, 0f);

        while (m_Open.TryPop(out int index, out float cost))
        {
            if (cost > m_Cost[index]) continue; // stale entry
            int x = index % Width;
            int y = index / Width;
            for (int n = 0; n < 8; n++)
            {
                int nx = x + s_Dx[n];
                int ny = y + s_Dy[n];
                if (!CanStep(x, y, nx, ny)) continue;
                int neighbour = ny * Width + nx;
                float next = cost + (n < 4 ? 1f : Diagonal);
                if (next >= m_Cost[neighbour]) continue;
                m_Cost[neighbour] = next;
                m_Open.Push(neighbour, next);
            }
        }

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                m_Direction[y * Width + x] = BestStep(x, y, targetIndex);
    }

    // Zero outside the grid, on blocked or unreachable cells, and in the target cell
    public Vector2 GetDirection(Vector2 position)
    {
        if (!TryGetCell(position, out int x, out int y)) return Vector2.zero;
        return m_Direction[y * Width + x];
    }

    private Vector2 BestStep(int x, int y, int targetIndex)
    {
        int index = y * Width + x;
        if (index == targetIndex || m_Blocked[index] || float.IsPositiveInfinity(m_Cost[index])) return Vector2.zero;

        float best = m_Cost[index];
        Vector2 direction = Vector2.zero;
        for (int n = 0; n < 8; n++)
        {
            int nx = x + s_Dx[n];
            int ny = y + s_Dy[n];
            if (!CanStep(x, y, nx, ny)) continue;
            float cost = m_Cost[ny * Width + nx];
            if (cost < best)
            {
                best = cost;
                direction = new Vector2(s_Dx[n], s_Dy[n]).normalized;
            }
        }
        return direction;
    }

    // In bounds, not blocked, and diagonals may not squeeze between two blocked cells
    private bool CanStep(int x, int y, int nx, int ny)
    {
        if (nx < 0 || ny < 0 || nx >= Width || ny >= Height) return false;
        if (m_Blocked[ny * Width + nx]) return false;
        if (nx != x && ny != y && (m_Blocked[y * Width + nx] || m_Blocked[ny * Width + x])) return false;
        return true;
    }

    // Binary min-heap of (cell index, cost); duplicates allowed, stale ones skipped on pop
    private class MinHeap
    {
        private readonly List<int> m_Items;
        private readonly List<float> m_Keys;

        public MinHeap(int capacity)
        {
            m_Items = new List<int>(capacity);
            m_Keys = new List<float>(capacity);
        }

        public void Clear()
        {
            m_Items.Clear();
            m_Keys.Clear();
        }

        public void Push(int item, float key)
        {
            m_Items.Add(item);
            m_Keys.Add(key);
            int i = m_Items.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (m_Keys[parent] <= m_Keys[i]) break;
                Swap(i, parent);
                i = parent;
            }
        }

        public bool TryPop(out int item, out float key)
        {
            item = 0;
            key = 0f;
            if (m_Items.Count == 0) return false;
            item = m_Items[0];
            key = m_Keys[0];
            int last = m_Items.Count - 1;
            m_Items[0] = m_Items[last];
            m_Keys[0] = m_Keys[last];
            m_Items.RemoveAt(last);
            m_Keys.RemoveAt(last);

            int i = 0;
            while (true)
            {
                int left = 2 * i + 1;
                int right = left + 1;
                int smallest = i;
                if (left < m_Items.Count && m_Keys[left] < m_Keys[smallest]) smallest = left;
                if (right < m_Items.Count && m_Keys[right] < m_Keys[smallest]) smallest = right;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
            return true;
        }

        private void Swap(int a, int b)
        {
            (m_Items[a], m_Items[b]) = (m_Items[b], m_Items[a]);
            (m_Keys[a], m_Keys[b]) = (m_Keys[b], m_Keys[a]);
        }
    }
}
