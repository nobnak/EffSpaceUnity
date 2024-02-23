# An implementation (for Unity) of efficient spatial data structures from [Stackoverflow thread](https://stackoverflow.com/questions/41946007/efficient-and-well-explained-implementation-of-a-quadtree-for-2d-collision-det#)

[Unity project](https://github.com/nobnak/Test-EffSpaceUnity) of this module.

## Demo
[![Demo](http://img.youtube.com/vi/0fxcMapOaBQ/hqdefault.jpg)](https://www.youtube.com/shorts/0fxcMapOaBQ)
[![Demo](http://img.youtube.com/vi/_xvoNZ3kExc/hqdefault.jpg)](https://www.youtube.com/shorts/_xvoNZ3kExc)

## Usage

### Initialize
```csharp
public GameObject prefab;

float2 screen;
FPointGrid grid;
int2 cellCount;
float2 cellSize;
float2 fieldSize;

float4x4 screenToWorld;
List<Particle> particleList;

void Initialize() {
    var c = Camera.main;
    screen = new float2(c.pixelWidth, c.pixelHeight);

    var hSize = c.orthographicSize;
    var aspect = c.aspect;
    screenToWorld = float4x4.TRS(
        new float3(-aspect * hSize, -hSize, 0f),
        quaternion.identity,
        new float3(2f * aspect * hSize / screen.x, 2f * hSize / screen.y, 1f)
        );

    var grid = 1 << 5;
    FPointGridExt.RecommendGrid(screen, grid, out cellCount, out cellSize);
    fieldSize = cellCount * cellSize;
    grid = new FPointGrid(cellCount, cellSize, float2.zero);

    var rand = Unity.Mathematics.Random.CreateFromIndex(31);
    particleList = new List<Particle>();
    for (var i = 0; i < tuner.count; i++) {
        var p = new Particle();
        var pos = rand.NextFloat2(float2.zero, screen);

        var go = Instantiate(prefab);
        go.transform.localPosition = math.transform(screenToWorld, new float3(seed, 0f));

        var e = grid.Insert(i, pos);

        p.id = i;
        p.element = e;
        p.go = go;
        p.seed = seed;
        p.pos = new float3(pos, 0f);
        particleList.Add(p);
    }
}
```

### Update
```csharp
void Update() {
    var t = Time.time * 0.5f;
    var dt = Time.deltaTime * 0.1f;
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
    }
}
```

### Query
```csharp
int Neareset(Particle p) {
    var pos = p.pos.xy;
    var min_dist_sq = float.MaxValue;
    var min_id = -1;
    foreach (var e in grid.Query(pos - qrange, pos + qrange)) {
        if (e == p.element) continue;

        var eq = grid.grid.elements[e];
        var q = particleList[eq.id];

        var qpos = q.pos.xy;
        var dist_sq = math.distancesq(qpos, pos);
        if (dist_sq < min_dist_sq) {
            min_dist_sq = dist_sq;
            min_id = e;
        }
    }
    return min_id;
}
```

## Data Structures
- Uniform Grid : 
  - [PointGrid](Runtime/Models/PointGrid.cs) (int2) 
  - [FPointGrid](Runtime/Models/FPointGrid.cs) (float2) 
- QuadTree: TODO

## Performance
On Razer Blade stealth 13 (2020)
- Core i7-1165G7
- Unity 2020.3

### Point Grid
![Point Grid perf.](Images/PointGrid01.png)
![Float Point Grid perf.](Images/FPointGrid01.png)

## References
1. user4842163, [Efficient (and well explained) implementation of a Quadtree for 2D collision detection](https://stackoverflow.com/questions/41946007/efficient-and-well-explained-implementation-of-a-quadtree-for-2d-collision-det# ), Stackoveflow, 2018
2. Mark Farragher, [What Is Faster In C#: An int[] or an int[,]?](https://mdfarragher.medium.com/high-performance-arrays-in-c-2d55c04d37b5), 2019
