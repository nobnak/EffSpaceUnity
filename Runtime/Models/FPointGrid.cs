using EffSpace.Extensions;
using EffSpace.Interfaces;
using System.Collections.Generic;
using Unity.Mathematics;

namespace EffSpace.Models {

	public class FPointGrid : IPointField<float2> {
        public const int intCellSize = 100;

        public readonly int2 cellCount;
        public readonly int2 cellSize;

        public readonly float2 toIntScale;
        public readonly float2 toIntOffset;

        public readonly PointGrid grid;

        public FPointGrid(int2 cellCount, float2 fCellSize, float2 fieldOffset) {
            this.toIntScale = intCellSize / fCellSize;
            this.toIntOffset = toIntScale * fieldOffset;

            this.cellCount = cellCount;
			this.cellSize = new int2(intCellSize);

            this.grid = new PointGrid(cellCount, cellSize);
        }

        public int Insert(int id, float2 pos) {
            FPointGridExt.ToIntPos(pos, toIntScale, toIntOffset, out var ipos);
            return grid.Insert(id, ipos);
        }
        public void Remove(int element) => grid.Remove(element);
        public IEnumerable<int> Query(float2 aabb_min, float2 aabb_max) {
            FPointGridExt.ToIntRangeFromAABB(
                aabb_min, aabb_max, toIntScale, toIntOffset,
                out var i_aabb_min, out var i_aabb_max);
            return grid.Query(i_aabb_min, i_aabb_max);
        }
        public void Clear() {
            grid.Clear();
        }
    }
}
