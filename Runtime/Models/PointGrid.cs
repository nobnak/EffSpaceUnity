using EffSpace.Collections;
using EffSpace.Constants;
using EffSpace.Extensions;
using EffSpace.Interfaces;
using System.Collections.Generic;
using Unity.Mathematics;

namespace EffSpace.Models {

    public struct PointGrid {

        public readonly FreeList<Element> elements;
        public readonly FreeList<LinkedElementNode> leaves;
        public readonly int[] grid;

        public readonly int2 cellCount;
        public readonly int2 cellSize;
        public readonly int2 fieldSize;

        public readonly int2 indexScaler;
        public readonly int totalCellCount;

        public PointGrid(int2 cellCount, int2 cellSize) {
            this.cellCount = cellCount;
            this.cellSize = cellSize;
            this.fieldSize = cellCount * cellSize;

            this.indexScaler = new int2(1, cellCount.x);
            this.totalCellCount = cellCount.x * cellCount.y;

            elements = new FreeList<Element>();
            leaves = new FreeList<LinkedElementNode>();
            grid = new int[totalCellCount];

            Clear();
        }

        public int Insert(int id, int2 pos) {
            var element = C.SENTRY;
            if (PointGridExt.TryGetCellIndex(cellCount, cellSize, pos, out var index)) {
                element = elements.Insert(new Element() { id = id, pos = pos });
                var cell = grid[index];
                cell = leaves.Insert(new LinkedElementNode() { element = element, next = cell });
                grid[index] = cell;
            }
            return element;
        }
        public void Remove(int element) {
            var e = elements[element];
            var mm = e.pos / cellSize;
            var index = math.csum(mm * indexScaler);
            var cell = grid[index];
            var curr = cell;

            while (curr != C.SENTRY && curr == cell) {
                var leaf = leaves[curr];
                if (leaf.element == element) {
                    cell = leaf.next;
                    leaves.Remove(curr);
                }
                curr = leaf.next;
            }
            grid[index] = cell;

            var prev = cell;
            while (curr != C.SENTRY) {
                var leaf = leaves[curr];
                if (leaf.element == element) {
                    var leaf_prev = leaves[prev];
                    leaf_prev.next = leaf.next;
                    leaves[prev] = leaf_prev;
                    leaves.Remove(curr);
                }
                prev = curr;
                curr = leaf.next;
            }
            elements.Remove(element);
        }
        public IEnumerable<int> Query(int2 aabb_min, int2 aabb_max) {
#if false
            var bmin = math.clamp(aabb_min / cellSize, 0, cellCount - 1);
            var bmax = math.clamp(aabb_max / cellSize, 0, cellCount - 1);
#else
            AABBExt.RangeFromAABB(cellCount, cellSize, aabb_min, aabb_max, out var bmin, out var bmax);
#endif
            for (var iy = bmin.y; iy <= bmax.y; iy++) {
                var yoffset = iy * cellCount.x;
                for (var ix = bmin.x; ix <= bmax.x; ix++) {
                    var cell = grid[ix + yoffset];
                    while (cell != C.SENTRY) {
                        var leaf = leaves[cell];
                        var e = elements[leaf.element];
                        if (AABBExt.IsIn(e.pos, aabb_min, aabb_max))
                            yield return leaf.element;
                        cell = leaf.next;
                    }
                }
            }
        }
        public void Clear() {
            elements.Clear();
            leaves.Clear();
            for (var i = 0; i < totalCellCount; i++)
                grid[i] = -1;
        }
    }
}
