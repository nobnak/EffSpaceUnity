using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EffSpace.Extensions.PointGridExt;
using EffSpace.Models;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

public class BenchPointGrid {

    public static readonly ProfilerMarker P_AddElement = new ProfilerMarker("Add element");
    public static readonly ProfilerMarker P_RemoveElement = new ProfilerMarker("Remove element");

    public const int D = 10;
    public const int n = 100000;
    public const int q = 10000;
    [Test]
    [Performance]
    public void BenchmarkQuery() {
        var markers = new string[] {
            "PointGrid.RectIndex",
        };

        var emin = 2;
        var emax = 6;
        for (var e = emin; e <= emax; e++) {
            var n = (int)math.round(math.pow(10, e));
            var sg = new SampleGroup($"Query: {n} points in grid", SampleUnit.Millisecond);

            var cellSize = new int2(100);
            var cellCount = new int2(1 << D);
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

            var queryAABB = Enumerable.Range(0, q)
                .Select(v => points[rand.NextInt(points.Count)])
                .Select(p => new int4(p, p + cellSize)).ToArray();
            Measure.Method(() => {
                for (var i = 0; i < queryAABB.Length; i++) {
                    grid.Query(queryAABB[i]).Count();
                }
            })
            .SampleGroup(sg)
            .Run();
        }

    }
    [Test]
    [Performance]
    public void BenchmarkAddRemove() {
        var p_addElement = new SampleGroup("Add element", SampleUnit.Millisecond);
        var p_removeElement = new SampleGroup("Remove element", SampleUnit.Millisecond);

        var markers = new SampleGroup[] {
            p_addElement,
            p_removeElement,
        };

        var cellSize = new int2(100);
        var cellCount = new int2(1 << 7);
        var fieldSize = cellSize * cellCount;

        Measure.Method(() => {
            var grid = new PointGrid(cellCount, cellSize);
            var rand = Unity.Mathematics.Random.CreateFromIndex(31);
            var points = new List<int2>();
            var ids = new List<int>();
            var elements = new List<int>();

            for (var i = 0; i < n; i++) {
                var id = rand.NextInt();
                var p = rand.NextInt2(0, fieldSize);
                points.Add(p);
                ids.Add(id);
            }

            P_AddElement.Begin();
            for (var i = 0; i < n; i++) {
                var id = ids[i];
                var p = points[i];
                var ie = grid.Insert(id, p);
                elements.Add(ie);
            }
            P_AddElement.End();

            P_RemoveElement.Begin();
            for (var i = 0; i < n; i++) {
                var ie = elements[i];
                grid.Remove(ie);
            }
            P_RemoveElement.End();
        }).ProfilerMarkers(markers)
        .Run();
    }
}
