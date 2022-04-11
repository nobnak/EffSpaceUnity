using System.Collections.Generic;
using Unity.Mathematics;

namespace EffSpace.Interfaces {
    public interface IPointField<T> {
        void Clear();
        int Insert(int id, T pos);
        IEnumerable<int> Query(T aabb_min, T aabb_max);
        void Remove(int element);
    }
}