using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EffSpace.Models;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

public class TestFPointGrid {

	[Test]
	public void TestFPointGridSimplePasses() {

		var cellCount = new int2(2, 2);
		var cellSize = new float2(10f, 10f);
		var fieldSize = cellCount * cellSize;

		var grid = new FPointGrid(cellCount, cellSize, float2.zero);

		var e0 = grid.Insert(0, new float2(0.1f, 0.1f));
		var e1 = grid.Insert(0, new float2(0.9f, 0.9f));
		var e2 = grid.Insert(0, new float2(0.8f, 0.7f));

		Assert.AreEqual(3, grid.grid.elements.Capacity);
		Assert.AreEqual(3, grid.grid.leaves.Capacity);

		var q0 = grid.Query(new float2(0.5f), new float2(1f)).ToArray();
		Assert.IsTrue(q0.Contains(e1));
		Assert.IsTrue(q0.Contains(e2));
		Assert.AreEqual(2, q0.Length);

		var q1 = grid.Query(new float2(0.5f, 0f), new float2(1f, 0.5f)).ToArray();
		Assert.AreEqual(0, q1.Length);

		grid.Remove(e0);
		Assert.AreEqual(2, grid.Query(float2.zero, new float2(1f)).Count());
		grid.Remove(e1);
		Assert.AreEqual(1, grid.Query(float2.zero, new float2(1f)).Count());
		grid.Remove(e2);
		Assert.AreEqual(0, grid.Query(float2.zero, new float2(1f)).Count());
	}
}

