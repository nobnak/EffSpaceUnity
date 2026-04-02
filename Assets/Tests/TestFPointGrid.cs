using EffSpace.Models;
using NUnit.Framework;
using Unity.Mathematics;

public class TestFPointGrid {

	static int QueryCount(FPointGrid grid, float2 aabbMin, float2 aabbMax) {
		var n = 0;
		foreach (var _ in grid.Query(aabbMin, aabbMax)) n++;
		return n;
	}

	static bool QueryContains(FPointGrid grid, float2 aabbMin, float2 aabbMax, int element) {
		foreach (var x in grid.Query(aabbMin, aabbMax))
			if (x == element) return true;
		return false;
	}

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

		Assert.IsTrue(QueryContains(grid, new float2(0.5f), new float2(1f), e1));
		Assert.IsTrue(QueryContains(grid, new float2(0.5f), new float2(1f), e2));
		Assert.AreEqual(2, QueryCount(grid, new float2(0.5f), new float2(1f)));

		Assert.AreEqual(0, QueryCount(grid, new float2(0.5f, 0f), new float2(1f, 0.5f)));

		grid.Remove(e0);
		Assert.AreEqual(2, QueryCount(grid, float2.zero, new float2(1f)));
		grid.Remove(e1);
		Assert.AreEqual(1, QueryCount(grid, float2.zero, new float2(1f)));
		grid.Remove(e2);
		Assert.AreEqual(0, QueryCount(grid, float2.zero, new float2(1f)));
	}
}

