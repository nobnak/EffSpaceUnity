using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EffSpace.Extensions.AABBExt {

    public static class AABBExtension {

        public static bool Contains(this int4 aabb, int2 pos)
            => math.all(aabb.xy <= pos) && math.all(pos <= aabb.zw);
    }
}
