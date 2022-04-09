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

        public static void Remove_Leaf(this PointGrid p, int index, int element) {
            var cell = p.grid[index];
            var curr = cell;

            while (curr != C.SENTRY && curr == cell) {
                var leaf = p.leaves[curr];
                if (leaf.element == element) {
                    cell = leaf.next;
                    p.leaves.Remove(curr);
                }
                curr = leaf.next;
            }
            p.grid[index] = cell;

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
        public static IEnumerable<int> Query_Cell(this PointGrid p, int4 aabb) {
            var minmax = p.CellRange(aabb);
            for (var iy = minmax.y; iy <= minmax.w; iy++) {
                var yoffset = iy * p.cellCount.x;
                for (var ix = minmax.x; ix <= minmax.z; ix++)
                    yield return ix + yoffset;
            }
        }
        public static IEnumerable<LinkedElementNode> IterateLeaves(
            this PointGrid p, int index) {
            var cell = p.grid[index];
            while (cell != C.SENTRY) {
                var leaf = p.leaves[cell];
                yield return leaf;
                cell = leaf.next;
            }
        }
        public static int2 Clamp(this PointGrid p, int2 cellIndex)
            => math.clamp(cellIndex, 0, p.cellCount - 1);
        public static int4 Clamp(this PointGrid p, int4 cellIndex)
            => math.clamp(cellIndex, 0, p.cellCount.xyxy - 1);
        public static bool TryGetCellIndex(this PointGrid p, int2 pos, out int index) {
            var i = pos / p.cellSize;
            index = math.csum(i * p.indexScaler);
            return math.all(0 <= i) && math.all(i < p.cellCount);
        }
        public static int GetCellIndex(this PointGrid p, int2 pos) {
            var i = math.clamp(pos / p.cellSize, 0, p.cellCount - 1);
            return math.csum(i * p.indexScaler);
        }
        public static int4 CellRange(this PointGrid p, int4 aabb)
            => math.clamp(aabb / p.cellSize.xyxy, 0, p.cellCount.xyxy - 1);
    }
}
