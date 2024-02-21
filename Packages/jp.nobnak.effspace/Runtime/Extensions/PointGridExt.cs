using EffSpace.Constants;
using EffSpace.Models;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace EffSpace.Extensions {

    [BurstCompile]
    public static class PointGridExt {

        public static IEnumerable<LinkedElementNode> IterateLeaves(
            this PointGrid p, int index) {
            var cell = p.grid[index];
            while (cell != C.SENTRY) {
                var leaf = p.leaves[cell];
                yield return leaf;
                cell = leaf.next;
            }
        }
        [BurstCompile]
        public static bool TryGetCellIndex(in int2 cellCount, in int2 cellSize, in int2 pos, 
            out int index) {
            var i = pos / cellSize;
            index = math.csum(i * new int2(1, cellCount.x));
            return math.all(0 <= i & i < cellCount);
        }
        [BurstCompile]
        public static int GetCellIndex(in int2 cellCount, in int2 cellSize, in int2 pos) {
            var i = math.clamp(pos / cellSize, 0, cellCount - 1);
            return math.csum(i * new int2(1, cellCount.x));
        }
    }
}
