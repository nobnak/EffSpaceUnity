using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EffSpace.Extensions;
using EffSpace.Models;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class BenchFPointGrid {

	[Test]
	[Performance]
	public void Query() {

		var screen = new float2(1920, 1080);
		var hCellCount = 1 << BenchPointGrid.D;

		FPointGridExt.RecommendGrid(screen, hCellCount, out var cellCount, out var cellSize);
		var fieldSize = cellSize * cellCount;

		Debug.Log($"Screen: size={screen}\nCell: size={cellSize}, n={cellCount}\nField: {fieldSize}");
		Assert.IsTrue(math.all(fieldSize >= screen));

		var emin = 2;
		var emax = 6;
		for (var pow = emin; pow <= emax; pow++) {
			var n = (int)math.round(math.pow(10, pow));
			var sg = new SampleGroup($"Query: {n} points in grid", SampleUnit.Millisecond);

			var grid = new FPointGrid(cellCount, cellSize, float2.zero);
			var elements = new List<int>();
			var points = new List<float2>();
			var ids = new List<int>();

			var rand = Unity.Mathematics.Random.CreateFromIndex(31);
			for (var i = 0; i < n; i++) {
				var p = new float2(rand.NextFloat2(screen));
				var elm = grid.Insert(i, p);
				Assert.IsTrue(elm >= 0);
				elements.Add(elm);
				points.Add(p);
				ids.Add(i);
			}

			var query = Enumerable.Range(0, BenchPointGrid.q)
				.Select(v => points[rand.NextInt(points.Count)])
				.Select(p => (min: p, max: p + cellSize))
				.ToArray();
			Measure.Method(() => {
				for (var i = 0; i < query.Length; i++) {
					grid.Query(query[i].min, query[i].max).Count();
				}
			}).SampleGroup(sg)
			.Run();
		}

	}
}
