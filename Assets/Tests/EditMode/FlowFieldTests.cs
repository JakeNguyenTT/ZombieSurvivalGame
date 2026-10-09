using NUnit.Framework;
using UnityEngine;

public class FlowFieldTests
{
    // 10x10 grid of 1-unit cells with its min corner at the world origin
    private static FlowField OpenField()
    {
        var field = new FlowField(10, 10, 1f);
        field.Origin = Vector2.zero;
        return field;
    }

    [Test]
    public void OpenField_PointsStraightAtTarget()
    {
        FlowField field = OpenField();
        field.Build(new Vector2(5.5f, 5.5f));
        Vector2 direction = field.GetDirection(new Vector2(8.5f, 5.5f));
        Assert.That(direction.x, Is.LessThan(-0.9f));
        Assert.That(Mathf.Abs(direction.y), Is.LessThan(0.1f));
    }

    [Test]
    public void Wall_IsRoutedAroundThroughTheGap()
    {
        FlowField field = OpenField();
        // Wall down column x=5 except the top row (gap at y=9)
        for (int y = 0; y < 9; y++) field.BlockCircle(new Vector2(5.5f, y + 0.5f), 0.1f);
        field.Build(new Vector2(8.5f, 0.5f));

        Vector2 direction = field.GetDirection(new Vector2(1.5f, 0.5f));
        Assert.That(direction.y, Is.GreaterThan(0.5f), "Should head up toward the gap, not into the wall");
    }

    [Test]
    public void OutsideGrid_ReturnsZero()
    {
        FlowField field = OpenField();
        field.Build(new Vector2(5.5f, 5.5f));
        Assert.AreEqual(Vector2.zero, field.GetDirection(new Vector2(-3f, 5f)));
        Assert.AreEqual(Vector2.zero, field.GetDirection(new Vector2(5f, 12f)));
    }

    [Test]
    public void BlockedCell_ReturnsZero()
    {
        FlowField field = OpenField();
        field.BlockCircle(new Vector2(2.5f, 2.5f), 0.1f);
        field.Build(new Vector2(7.5f, 7.5f));
        Assert.AreEqual(Vector2.zero, field.GetDirection(new Vector2(2.5f, 2.5f)));
    }

    [Test]
    public void TargetCell_ReturnsZero_SoEnemiesWalkStraightIn()
    {
        FlowField field = OpenField();
        field.Build(new Vector2(5.5f, 5.5f));
        Assert.AreEqual(Vector2.zero, field.GetDirection(new Vector2(5.2f, 5.8f)));
    }

    [Test]
    public void Diagonal_DoesNotCutBlockedCorners()
    {
        FlowField field = OpenField();
        // Block the cells right of and above (2,2); target up-right of it
        field.BlockCircle(new Vector2(3.5f, 2.5f), 0.1f);
        field.BlockCircle(new Vector2(2.5f, 3.5f), 0.1f);
        field.Build(new Vector2(6.5f, 6.5f));
        Vector2 direction = field.GetDirection(new Vector2(2.5f, 2.5f));
        // Squeezing diagonally between the two blocks would be (0.7, 0.7)
        Assert.IsFalse(direction.x > 0.5f && direction.y > 0.5f, $"Cut the corner: {direction}");
    }

    [Test]
    public void CenterOn_SnapsGridAroundPosition()
    {
        var field = new FlowField(10, 10, 1f);
        field.CenterOn(new Vector2(0.7f, 0.2f));
        Assert.IsTrue(field.TryGetCell(new Vector2(0.7f, 0.2f), out int x, out int y));
        Assert.AreEqual(5, x);
        Assert.AreEqual(5, y);
    }

    [Test]
    public void TargetInsideObstacle_StillProducesAField()
    {
        FlowField field = OpenField();
        field.BlockCircle(new Vector2(5.5f, 5.5f), 0.1f);
        field.Build(new Vector2(5.5f, 5.5f));
        Assert.AreNotEqual(Vector2.zero, field.GetDirection(new Vector2(1.5f, 5.5f)));
    }
}
