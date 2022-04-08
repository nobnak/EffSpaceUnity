using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EfficientSpacialDataStructure.Extensions.PointGridExt;
using EfficientSpacialDataStructure.Models;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class TestPointGrid {
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

            Assert.IsTrue(grid.Query(p.xyxy).Contains(e));
            Assert.AreEqual(id, grid.elements[e].id);
        }

        Assert.AreEqual(elements[0], grid.Query(points[0].xyxy).First());
        var q01 = grid.Query(points[1].xyxy).ToArray();
        foreach (var i in elements.Skip(1).Take(2))
            Assert.IsTrue(q01.Contains(i));

        while (elements.Count > 0) {
            var last = elements.Count - 1;
            var e = elements[last];
            var id = ids[last];
            var p = points[last];

            Assert.AreEqual(elements.Count, grid.Query(new int4(0, 0, 2, 2)).Count());
            Assert.IsTrue(grid.Query(p.xyxy).Contains(e));
            grid.Remove(e);
            Assert.IsFalse(grid.Query(p.xyxy).Contains(e));

            elements.RemoveAt(last);
        }
    }

    [Test]
    [Performance]
    public void Benchmark() {
        var markers = new string[] {
            "PGrid.Query_Cell",
            "PGrid.IterateLeaves",
            "PGrid.Contains",
        };

        var cellSize = new int2(100);
        var cellCount = new int2(1 << 10);
        var fieldSize = cellSize * cellCount;

        var grid = new PointGrid(cellCount, cellSize);
        var rand = Unity.Mathematics.Random.CreateFromIndex(31);
        Debug.Log($"Grid: ({grid.grid.GetLength(0)}, {grid.grid.GetLength(1)}), cellCount={cellCount}");

        var n = 10000;
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
            var cell = grid.Query(p.xyxy).ToArray();
            Assert.IsTrue(cell.Contains(e));
        }

        var lmin = int.MaxValue;
        var lmax = int.MinValue;
        for (var i = 0; i < cellCount.x; i++) {
            for (var j = 0; j < cellCount.y; j++) {
                var ic = new int2(i, j);
                var cell = EfficientSpacialDataStructure.Extensions.PointGridExt
                    .PointGridExtension.IterateLeaves(grid, ic);
                var count = cell.Count();
                if (count < lmin) lmin = count;
                if (lmax < count) lmax = count;
            }
        }
        Debug.Log($"Nodes in cell: [{lmin}, {lmax}]");

        lmin = int.MaxValue;
        lmax = int.MinValue;
        for (var i = 0; i < n; i++) {
            var p = points[i];
            var c = grid.Query_Cell(new int4(p.xy, p.xy + cellSize)).Count();
            if (c < lmin) lmin = c;
            if (lmax < c) lmax = c;
        }
        Debug.Log($"Per cell query: [{lmin}, {lmax}]");

        Measure.Method(() => {
            for (var i = 0; i < n; i++) {
                var p = points[i];
                grid.Query(new int4(p.xy, p.xy + cellSize)).Count();
            }
        }).SampleGroup("Query for grid({n},{n})")
        .ProfilerMarkers(markers)
        .Run();
    }
}
