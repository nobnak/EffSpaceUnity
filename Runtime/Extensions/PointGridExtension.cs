using EfficientSpacialDataStructure.Collections;
using EfficientSpacialDataStructure.Constants;
using EfficientSpacialDataStructure.Models;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace EfficientSpacialDataStructure.Extensions.PointGridExt {

    public static class PointGridExtension {

        public static readonly ProfilerMarker P_Query_Cell = new ProfilerMarker("PGrid.Query_Cell");
        public static readonly ProfilerMarker P_IterateLeaves = new ProfilerMarker("PGrid.IterateLeaves");

        public static void Remove_Leaf(this PointGrid p, int2 index, int element) {
            var cell = p.grid[index.x, index.y];
            var curr = cell;

            while (curr != C.SENTRY && curr == cell) {
                var leaf = p.leaves[curr];
                if (leaf.element == element) {
                    cell = leaf.next;
                    p.leaves.Remove(curr);
                }
                curr = leaf.next;
            }
            p.grid[index.x, index.y] = cell;

            var prev = cell;
            while (curr != C.SENTRY) {
                var leaf = p.leaves[curr];
                if (leaf.element == element) {
                    var leaf_prev = p.leaves[prev];
                    leaf_prev.next = leaf.next;
                    p.leaves[prev] = leaf_prev;
                    p.leaves.Remove(curr);
                }
                prev = curr;
                curr = leaf.next;
            }
        }
        public static IEnumerable<int2> Query_Cell(this PointGrid p, int4 aabb) {
            P_Query_Cell.Begin();
            var imin = p.Clamp(p.GetCellIndex(aabb.xy));
            var imax = p.Clamp(p.GetCellIndex(aabb.zw));
            for (var ix = imin.x; ix <= imax.x; ix++)
                for (var iy = imin.y; iy <= imax.y; iy++)
                    yield return new int2(ix, iy);
            P_Query_Cell.End();
        }
        public static IEnumerable<LinkedElementNode> IterateLeaves(
            this PointGrid p, int ix, int iy) {
            P_IterateLeaves.Begin();
            var cell = p.grid[ix, iy];
            while (cell != C.SENTRY) {
                var leaf = p.leaves[cell];
                yield return leaf;
                cell = leaf.next;
            }
            P_IterateLeaves.End();
        }
        public static int2 Clamp(this PointGrid p, int2 cellIndex)
            => math.clamp(cellIndex, 0, p.cellCount - 1);
        public static int4 Clamp(this PointGrid p, int4 cellIndex)
            => math.clamp(cellIndex, 0, p.cellCount.xyxy - 1);
        public static bool TryGetCellIndex(this PointGrid p, int2 pos, out int2 index) {
            index = p.GetCellIndex(pos);
            return math.all(0 <= index & index < p.cellCount);
        }
        public static int2 GetCellIndex(this PointGrid p, int2 pos) => pos / p.cellSize;
    }
}
