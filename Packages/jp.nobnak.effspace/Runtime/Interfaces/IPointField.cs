using Unity.Mathematics;

namespace EffSpace.Interfaces {

	public interface IPointField<T> {
		event AddRemoveHandler OnAdd;
		event AddRemoveHandler OnRemove;

		void Clear();
		int Insert(int id, T pos);
		void Remove(int element);
	}
}