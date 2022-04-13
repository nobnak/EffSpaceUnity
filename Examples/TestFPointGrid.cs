using EffSpace.Collections;
using EffSpace.Extensions;
using EffSpace.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace EffSpace.Examples {

    public class TestFPointGrid : MonoBehaviour {

		public Tuner tuner = new Tuner();
		public Link link = new Link();

		protected float2 screen;
		protected FPointGrid grid;
		protected float4x4 screenToWorld;
		protected List<Particle> particleList;

        private void OnEnable() {
            var c = Camera.main;
			screen = new float2(c.pixelWidth, c.pixelHeight);

			var hSize = c.orthographicSize;
			var aspect = c.aspect;
			screenToWorld = float4x4.TRS(
				new float3(-aspect * hSize, -hSize, 0f),
				quaternion.identity,
				new float3(2f * aspect * hSize / screen.x, 2f * hSize / screen.y, 1f)
				);

			FPointGridExt.RecommendGrid(screen, 1 << tuner.grid, out var cellCount, out var cellSize);
			var fieldSize = cellCount * cellSize;
			Debug.Log($"Screen: {screen}, Grid: n={cellCount}, field={fieldSize}");

			grid = new FPointGrid(cellCount, cellSize, float2.zero);

			var rand = Unity.Mathematics.Random.CreateFromIndex(31);
			particleList = new List<Particle>();
			for (var i = 0; i < tuner.count; i++) {
				var p = new Particle();
				var seed = rand.NextFloat2(float2.zero, screen);
				
				var go = Instantiate(link.fab);
				go.transform.SetParent(transform);
				go.transform.localPosition = math.transform(screenToWorld, new float3(seed, 0f));

				var e = grid.Insert(i, seed);
				if (e < 0)
					Debug.LogWarning($"Position not on screen: {seed}");

				p.id = i;
				p.element = e;
				p.go = go;
				p.seed = seed;
				p.pos = new float3(seed, 0f);
				particleList.Add(p);
			}

			foreach (var p in particleList) p.go.SetActive(true);
		}
		private void OnDisable() {
			for (var i = 0; i < particleList.Count; i++) {
				var p = particleList[i];
				Destroy(p.go);
			}
			particleList.Clear();
		}
		private void Update() {
			var t = Time.time * tuner.freq;
			var dt = Time.deltaTime * tuner.speed;
			var counter = 0;
			for (var i = 0; i < particleList.Count; i++) {
				var p = particleList[i];
				if (p.element >= 0) grid.Remove(p.element);

				var pos = p.pos.xy;
				pos += dt * screen * new float2(
					noise.snoise(new float3(p.seed, t)),
					noise.snoise(new float3(-p.seed, t)));
				pos -= screen * math.floor(pos / screen);

				p.pos = new float3(pos, p.pos.z);
				p.go.transform.localPosition = math.transform(screenToWorld, p.pos);

				p.element = grid.Insert(p.id, pos);
				if (p.element >= 0) counter++;
			}

			var cdiff = particleList.Count - counter;
			if (cdiff != 0) Debug.LogWarning($"Num particles outside of grid: {cdiff}");
		}

		[System.Serializable]
        public class Tuner {
            public int grid = 10;
			public int count = 10;
			public float speed = 0.1f;
			public float freq = 0.1f;
        }
		[System.Serializable]
		public class Link {
			public GameObject fab;
		}

		[System.Serializable]
		public class Particle {
			public int id;
			public int element;
			public float2 seed;
			public float3 pos;
			public GameObject go;

			public override string ToString() {
				return $"{GetType().Name}: {id}/{element}, pos={pos}";
			}
		}
    }

}
