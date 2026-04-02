using EffSpace.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Profiling;
using EffSpace.Extensions;

public class TestPointGrid {

    static bool QueryContains(PointGrid grid, int2 aabbMin, int2 aabbMax, int element) {
        foreach (var x in grid.Query(aabbMin, aabbMax))
            if (x == element) return true;
        return false;
    }

    static int QueryCount(PointGrid grid, int2 aabbMin, int2 aabbMax) {
        var n = 0;
        foreach (var _ in grid.Query(aabbMin, aabbMax)) n++;
        return n;
    }

    public static readonly ProfilerMarker P_AddElement = new ProfilerMarker("Add element");
    public static readonly ProfilerMarker P_RemoveElement = new ProfilerMarker("Remove element");

    public const int n = 10000;
    public const int q = 10000;

    [Test]
    public void TestPointGridSimplePasses() {
        var rand = Unity.Mathematics.Random.CreateFromIndex(31);
        var grid = new PointGrid(new int2(2, 2), new int2(1, 1));

        var points = new int2[] {
            new int2(0, 0),
            new int2(0, 1),
            new int2(0, 1)
        };
        var elements = new List<int>();
        var ids = new List<int>();
        for (var i = 0; i < points.Length; i++) {
            var id = rand.NextInt();
            var p = points[i];
            var e = grid.Insert(id, p);
            elements.Add(e);
            ids.Add(id);

            Assert.IsTrue(QueryContains(grid, p.xy, p.xy, e));
            Assert.AreEqual(id, grid.elements[e].id);
        }

        var first0 = int.MinValue;
        foreach (var x in grid.Query(points[0], points[0])) {
            first0 = x;
            break;
        }
        Assert.AreEqual(elements[0], first0);
        var q01 = new List<int>();
        foreach (var x in grid.Query(points[1], points[1])) q01.Add(x);
        foreach (var i in elements.Skip(1).Take(2))
            Assert.IsTrue(q01.Contains(i));

        while (elements.Count > 0) {
            var last = elements.Count - 1;
            var e = elements[last];
            var id = ids[last];
            var p = points[last];

            Assert.AreEqual(elements.Count, QueryCount(grid, new int2(0, 0), new int2(2, 2)));
            Assert.IsTrue(QueryContains(grid, p, p, e));
            grid.Remove(e);
            Assert.IsFalse(QueryContains(grid, p, p, e));

            elements.RemoveAt(last);
        }
    }

    [Test]
    public void TestRandom() {
        var cellSize = new int2(100);
        var cellCount = new int2(1 << 7);
        var fieldSize = cellSize * cellCount;

        var grid = new PointGrid(cellCount, cellSize);
        var rand = Unity.Mathematics.Random.CreateFromIndex(31);

        var points = new List<int2>();
        var ids = new List<int>();
        var elements = new List<int>();
        for (var i = 0; i < n; i++) {
            var id = rand.NextInt();
            var p = rand.NextInt2(0, fieldSize);

            var ie = grid.Insert(id, p);
            points.Add(p);
            ids.Add(id);
            elements.Add(ie);
        }

        for (var i = 0; i < n; i++) {
            var p = points[i];
            var e = elements[i];
            Assert.IsTrue(QueryContains(grid, p, p, e));
        }

        var samples = 1000;
        var sum = 0;
        for (var i = 0; i < samples; i++) {
            var j = rand.NextInt(0, grid.totalCellCount);
            var count = grid.IterateLeaves(j).Count();
            sum += count;
        }
        var avg = (float)sum / samples;
        var density = (float)n / grid.totalCellCount;
        Assert.Less(avg, 2f * density);

    }
}
