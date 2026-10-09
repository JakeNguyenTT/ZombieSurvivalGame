using UnityEngine;

// Keeps one flow field around the player so every enemy can walk around arena obstacles.
// Rebuilt only when the player moves into another cell; obstacles never move.
public class EnemyNavigation : MonoBehaviour
{
    private const int GridSize = 96;      // cells per side
    private const float CellSize = 1.5f;  // 144 x 144 units around the player
    private const float EnemyClearance = 0.6f;
    // Within this angle of the direct line, enemies walk straight (smooth) instead of grid-stepping
    private const float StraightLineAngle = 35f;

    public static EnemyNavigation Instance { get; private set; }

    private readonly FlowField m_Field = new FlowField(GridSize, GridSize, CellSize);
    private Vector2Int m_LastPlayerCell = new Vector2Int(int.MinValue, int.MinValue);
    private bool m_Ready;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (ArenaProps.Obstacles.Count == 0) return; // open field: straight lines are already right
        Vector3 player = GameManager.Instance.GetPlayerPosition();
        var cell = new Vector2Int(Mathf.FloorToInt(player.x / CellSize), Mathf.FloorToInt(player.z / CellSize));
        if (m_Ready && cell == m_LastPlayerCell) return;
        m_LastPlayerCell = cell;
        Rebuild(new Vector2(player.x, player.z));
    }

    private void Rebuild(Vector2 player)
    {
        m_Field.CenterOn(player);
        m_Field.ClearBlocked();
        float reach = GridSize * CellSize; // anything farther can't touch the grid
        foreach (ArenaObstacle obstacle in ArenaProps.Obstacles)
            if ((obstacle.Position - player).sqrMagnitude < reach * reach)
                m_Field.BlockCircle(obstacle.Position, obstacle.Radius + EnemyClearance);
        m_Field.Build(player);
        m_Ready = true;
    }

    // Direction an enemy at `position` should walk, given the straight line to the player
    public Vector3 Steer(Vector3 position, Vector3 direct)
    {
        if (!m_Ready) return direct;
        Vector2 flow = m_Field.GetDirection(new Vector2(position.x, position.z));
        if (flow == Vector2.zero) return direct; // off-grid, at the player, or boxed in
        var path = new Vector3(flow.x, 0f, flow.y);
        return Vector3.Angle(path, direct) <= StraightLineAngle ? direct : path;
    }
}
