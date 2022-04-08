using EfficientSpacialDataStructure.Collections;
using EfficientSpacialDataStructure.Constants;
using EfficientSpacialDataStructure.Extensions.AABBExt;
using EfficientSpacialDataStructure.Extensions.PointGridExt;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Profiling;

namespace EfficientSpacialDataStructure.Models {

    public class PointGrid {
        public static ProfilerMarker P_Contains = new ProfilerMarker("PGrid.Contains");

        public readonly FreeList<Element> elements;
        public readonly FreeList<LinkedElementNode> leaves;
        public readonly int[,] grid;

        public readonly int2 cellCount;
        public readonly int2 cellSize;
        public readonly int2 fieldSize;

        public PointGrid(int2 cellCount, int2 cellSize) {
            this.cellCount = cellCount;
            this.cellSize = cellSize;
            this.fieldSize = cellCount * cellSize;

            elements = new FreeList<Element>();
            leaves = new FreeList<LinkedElementNode>();
            grid = new int[cellCount.x, cellCount.y];

            Clear();
        }

        public int Insert(int id, int2 pos) {
            var element = C.SENTRY;
            if (this.TryGetCellIndex(pos, out var index)) {
                element = elements.Insert(new Element() { id = id, pos = pos });
                var cell = grid[index.x, index.y];
                cell = leaves.Insert(new LinkedElementNode() { element = element, next = cell });
                grid[index.x, index.y] = cell;
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
            for (var ix = mm.x; ix <= mm.z; ix++) {
                for (var iy = mm.y; iy <= mm.w; iy++) {
                    var cell = grid[ix, iy];
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
            for (var ix = 0; ix < cellCount.x; ix++)
                for (var iy = 0; iy < cellCount.y; iy++)
                    grid[ix, iy] = -1;
        }
    }
}
