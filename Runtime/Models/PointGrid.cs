using EfficientSpacialDataStructure.Collections;
using EfficientSpacialDataStructure.Constants;
using EfficientSpacialDataStructure.Extensions.AABBExt;
using EfficientSpacialDataStructure.Extensions.PointGridExt;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Profiling;

namespace EfficientSpacialDataStructure.Models {

    public class PointGrid {

        public static ProfilerMarker P_RectIndex = new ProfilerMarker("PointGrid.RectIndex");

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
            if (this.TryGetCellIndex(pos, out var index)) {
                element = elements.Insert(new Element() { id = id, pos = pos });
                var cell = grid[index];
                cell = leaves.Insert(new LinkedElementNode() { element = element, next = cell });
                grid[index] = cell;
            }
            return element;
        }
        public void Remove(int element) {
            var e = elements[element];
            foreach (var index in this.Query_Cell(e.pos.xyxy))
                this.Remove_Leaf(index, element);
            elements.Remove(element);
        }
        public IEnumerable<int> Query(int4 aabb) {
            var mm = math.clamp(aabb / cellSize.xyxy, 0, cellCount.xyxy - 1);
            for (var iy = mm.y; iy <= mm.w; iy++) {
                var yoffset = iy * cellCount.x;
                for (var ix = mm.x; ix <= mm.z; ix++) {
                    var cell = grid[ix + yoffset];
                    while (cell != C.SENTRY) {
                        var leaf = leaves[cell];
                        var e = elements[leaf.element];
                        if (aabb.Contains(e.pos))
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
