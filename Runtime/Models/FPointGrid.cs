using EffSpace.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EffSpace.Models {

    public class FPointGrid : IPointField<float2> {
        public const int intCellScale = 100;

        public readonly int2 cellCount;
        public readonly int2 cellSize;

        public readonly float2 toIntScale;
        public readonly float2 toIntOffset;

        public readonly PointGrid grid;

        public FPointGrid(int2 cellCount, float2 fCellSize, float2 fieldOffset) {
            this.toIntScale = intCellScale / fCellSize;
            this.toIntOffset = toIntScale * fieldOffset;

            this.cellCount = cellCount;
            this.cellSize = (int2)(toIntScale * fCellSize);

            this.grid = new PointGrid(cellCount, cellSize);
        }

        public int Insert(int id, float2 pos) {
            var ipos = (int2)math.mad(pos, toIntScale, toIntOffset);
            return grid.Insert(id, ipos);
        }
        public void Remove(int element) => grid.Remove(element);
        public IEnumerable<int> Query(float2 aabb_min, float2 aabb_max) {
            var i_aabb_min = (int2)math.mad(aabb_min, toIntScale, toIntOffset);
            var i_aabb_max = (int2)math.mad(aabb_max, toIntScale, toIntOffset);
            return grid.Query(i_aabb_min, i_aabb_max);
        }
        public void Clear() {
            grid.Clear();
        }
    }
}
