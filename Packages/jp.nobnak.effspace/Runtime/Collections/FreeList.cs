using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EffSpace.Collections {

    public class FreeList<T> {

        protected List<T> data = new List<T>();
        protected Stack<int> free = new Stack<int>();

        #region interface
        public int Capacity { get => data.Count; }
        public T this[int index] {
            get => data[index];
            set => data[index] = value;
        }
        public int Insert(T item) {
            int index;
            if (free.Count > 0) {
                index = free.Pop();
                data[index] = item;
            } else {
                index = data.Count;
                data.Add(item);
            }
            return index;
        }
        public void Remove(int index) {
            free.Push(index);
        }
        public void Clear() {
            data.Clear();
            free.Clear();
        }
        #endregion
    }
}
