using EffSpace.Models;
using Unity.Burst;
using Unity.Mathematics;

namespace EffSpace.Extensions {

    [BurstCompile]
    public static class AABBExt {

        [BurstCompile]
        public static bool Contains(in int4 aabb, in int2 pos)
            => math.all(aabb.xy <= pos & pos <= aabb.zw);

        [BurstCompile]
        public static bool IsIn(in int2 pos, in int2 min, in int2 max)
            => math.all(min <= pos & pos <= max);

        [BurstCompile]
        public static void RangeFromAABB(
            in int2 cellCount, in int2 cellSize,
            in int2 aabb_min, in int2 aabb_max, 
            out int2 imin, out int2 imax) {
            imin = math.clamp(aabb_min / cellSize, 0, cellCount - 1);
            imax = math.clamp(aabb_max / cellSize, 0, cellCount - 1);
        }
    }
}
