using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace EffSpace.Extensions {

    [BurstCompile]
    public static class FPointGridExt {

        [BurstCompile]
        public static void ToIntPos(
            in float2 pos, in float2 toIntScale, in float2 toIntOffset,
            out int2 ipos) {
            ipos = (int2)math.mad(pos, toIntScale, toIntOffset);
        }

        [BurstCompile]
        public static void ToIntRangeFromAABB(
            in float2 aabb_min, in float2 aabb_max,
            in float2 toIntScale, in float2 toIntOffset,
            out int2 i_aabb_min, out int2 i_aabb_max) {
            i_aabb_min = (int2)math.mad(aabb_min, toIntScale, toIntOffset);
            i_aabb_max = (int2)math.mad(aabb_max, toIntScale, toIntOffset);
        }
    }
}
