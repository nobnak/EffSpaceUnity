using EffSpace.Collections;
using EffSpace.Constants;
using EffSpace.Extensions;
using EffSpace.Interfaces;
using Unity.Mathematics;

namespace EffSpace.Models {

    public class PointGrid : IPointField<int2> {

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

		#region interface

		#region events
		public event AddRemoveHandler OnAdd;
		public event AddRemoveHandler OnRemove;
		#endregion

		public int Insert(int id, int2 pos) {
            var element = C.SENTRY;
            if (PointGridExt.TryGetCellIndex(cellCount, cellSize, pos, out var index)) {
                element = elements.Insert(new Element() { id = id, pos = pos });
                var cell = grid[index];
                cell = leaves.Insert(new LinkedElementNode() { element = element, next = cell });
                grid[index] = cell;
				OnAdd?.Invoke(index, element);
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
			OnRemove?.Invoke(index, element);
		}
        public QueryEnumerable Query(int2 aabb_min, int2 aabb_max)
            => new QueryEnumerable(this, aabb_min, aabb_max);
        public void Clear() {
            elements.Clear();
            leaves.Clear();
            for (var i = 0; i < totalCellCount; i++)
                grid[i] = -1;
        }
		#endregion

		public readonly struct QueryEnumerable {
			readonly PointGrid grid;
			readonly int2 aabb_min;
			readonly int2 aabb_max;
			readonly int2 bmin;
			readonly int2 bmax;

			internal QueryEnumerable(PointGrid grid, int2 aabb_min, int2 aabb_max) {
				this.grid = grid;
				this.aabb_min = aabb_min;
				this.aabb_max = aabb_max;
				AABBExt.RangeFromAABB(grid.cellCount, grid.cellSize, aabb_min, aabb_max, out bmin, out bmax);
			}

			public QueryEnumerator GetEnumerator()
				=> new QueryEnumerator(grid, aabb_min, aabb_max, bmin, bmax);
		}

		public struct QueryEnumerator {
			PointGrid grid;
			int2 aabb_min;
			int2 aabb_max;
			int2 bmin;
			int2 bmax;
			int ix;
			int iy;
			int curr;
			int current;

			internal QueryEnumerator(PointGrid grid, int2 aabb_min, int2 aabb_max, int2 bmin, int2 bmax) {
				this.grid = grid;
				this.aabb_min = aabb_min;
				this.aabb_max = aabb_max;
				this.bmin = bmin;
				this.bmax = bmax;
				ix = bmin.x;
				iy = bmin.y;
				curr = grid.grid[ix + iy * grid.cellCount.x];
				current = default;
			}

			public int Current => current;

			public bool MoveNext() {
				while (iy <= bmax.y) {
					while (curr != C.SENTRY) {
						var leaf = grid.leaves[curr];
						var elemIndex = leaf.element;
						curr = leaf.next;
						var e = grid.elements[elemIndex];
						if (AABBExt.IsIn(e.pos, aabb_min, aabb_max)) {
							current = elemIndex;
							return true;
						}
					}
					ix++;
					if (ix > bmax.x) {
						ix = bmin.x;
						iy++;
					}
					if (iy > bmax.y)
						break;
					curr = grid.grid[ix + iy * grid.cellCount.x];
				}
				return false;
			}
		}
	}
}
