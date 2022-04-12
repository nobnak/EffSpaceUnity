using EffSpace.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Profiling;
using EffSpace.Extensions;

public class TestPointGrid {

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

            Assert.IsTrue(grid.Query(p.xy, p.xy).Contains(e));
            Assert.AreEqual(id, grid.elements[e].id);
        }

        Assert.AreEqual(elements[0], grid.Query(points[0], points[0]).First());
        var q01 = grid.Query(points[1], points[1]).ToArray();
        foreach (var i in elements.Skip(1).Take(2))
            Assert.IsTrue(q01.Contains(i));

        while (elements.Count > 0) {
            var last = elements.Count - 1;
            var e = elements[last];
            var id = ids[last];
            var p = points[last];

            Assert.AreEqual(elements.Count, grid.Query(new int2(0, 0), new int2(2, 2)).Count());
            Assert.IsTrue(grid.Query(p, p).Contains(e));
            grid.Remove(e);
            Assert.IsFalse(grid.Query(p, p).Contains(e));

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
            var cell = grid.Query(p, p).ToArray();
            Assert.IsTrue(cell.Contains(e));
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
