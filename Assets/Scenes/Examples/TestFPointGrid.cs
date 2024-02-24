using EffSpace.Collections;
using EffSpace.Constants;
using EffSpace.Extensions;
using EffSpace.Models;
using LLGraphicsUnity;
using LLGraphicsUnity.Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace EffSpace.Examples {

	public class TestFPointGrid : MonoBehaviour {

		public Tuner tuner = new Tuner();
		public Link link = new Link();

		protected float2 screen;
		protected FPointGrid grid;
		protected int2 cellCount;
		protected float2 cellSize;
		protected float2 fieldSize;

		protected float4x4 screenToWorld;
		protected List<Particle> particleList;

		protected int[] countsOnCell;
		protected GLMaterial gl;

		private void OnEnable() {
			gl = new GLMaterial();

			var c = Camera.main;
			screen = new float2(c.pixelWidth, c.pixelHeight);

			var hSize = c.orthographicSize;
			var aspect = c.aspect;
			screenToWorld = float4x4.TRS(
				new float3(-aspect * hSize, -hSize, 0f),
				quaternion.identity,
				new float3(2f * aspect * hSize / screen.x, 2f * hSize / screen.y, 1f)
				);

			FPointGridExt.RecommendGrid(screen, 1 << tuner.grid, out cellCount, out cellSize);
			fieldSize = cellCount * cellSize;
			Debug.Log($"Screen: {screen}, Grid: n={cellCount}, field={fieldSize}");

			countsOnCell = new int[cellCount.x * cellCount.y];
			grid = new FPointGrid(cellCount, cellSize, float2.zero);

			grid.OnAdd += (i, e) => countsOnCell[i]++;
			grid.OnRemove += (i, e) => countsOnCell[i]--;

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

			if (gl != null) {
				gl.Dispose();
				gl = null;
			}
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
		private void OnRenderObject() {
			var data = new GLProperty() {
				Color = Color.white,
				MainTex = null,
				ZWriteMode = false,
				ZTestMode = CompareFunction.Always,
				SrcBlend = BlendMode.SrcAlpha,
				DstBlend = BlendMode.One,
			};
			var qrange = 0.1f * screen.yy;
			var search_limit_dist_sq = 2f * qrange.y * qrange.y;
			var colorLerp = new float2(0, 10f * (float)particleList.Count / (cellCount.x * cellCount.y));
			var color0 = Color.clear;
			var color1 = Color.cyan;

			using (new GLMatrixScope())
			using (gl.GetScope(data)) {
				GL.LoadIdentity();
				GL.LoadPixelMatrix();
				var modelview = GL.modelview;

				switch (tuner.visualMode) {
					case Tuner.VisualMode.Grid:
					for (var y = 0; y < cellCount.y; y++) {
						var countOffset = y * cellCount.x;
						for (var x = 0; x < cellCount.x; x++) {
							var count = countsOnCell[x + countOffset];

							var t = (float)(count - colorLerp.x) / colorLerp.y;
							var c = Color.Lerp(color0, color1, t);
							using (gl.GetScope(new GLProperty(data) { Color = c })) {
								var mv = Matrix4x4.TRS(
									new float3(cellSize * new float2(x + 0.5f, y + 0.5f), 0f),
									quaternion.identity,
									new float3(cellSize, 1f));
								GL.modelview = modelview * mv;
								Quad.TriangleStrip();
							}
						}
					}
					break;

					case Tuner.VisualMode.Circle:
					for (var i = 0; i < particleList.Count; i++) {
						var p = particleList[i];
						var pos = p.pos.xy;
						var min_dist_sq = float.MaxValue;
						var min_pos = float2.zero;
						foreach (var e in grid.Query(pos - qrange, pos + qrange)) {
							if (e == p.element) continue;

							var eq = grid.grid.elements[e];
							var q = particleList[eq.id];

							var qpos = q.pos.xy;
							var dist_sq = math.distancesq(qpos, pos);
							if (dist_sq < min_dist_sq) {
								min_dist_sq = dist_sq;
								min_pos = qpos;
							}
						}
						if (min_dist_sq > search_limit_dist_sq) continue;

						var d = math.sqrt(min_dist_sq);
						var mv = Matrix4x4.TRS(p.pos, quaternion.identity, Vector3.one);
						GL.modelview = modelview * mv;
						Circle.LineStrip(0.5f * d, 20);
					}
					break;
				}
			}
		}

		[System.Serializable]
		public class Tuner {
			public int grid = 10;
			public int count = 10;
			public float speed = 0.1f;
			public float freq = 0.1f;

			public VisualMode visualMode = VisualMode.Grid;

			public enum VisualMode { None = 0, Circle, Grid }
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
