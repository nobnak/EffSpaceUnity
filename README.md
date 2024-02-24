# An implementation (for Unity) of efficient spatial data structures from [Stackoverflow thread](https://stackoverflow.com/questions/41946007/efficient-and-well-explained-implementation-of-a-quadtree-for-2d-collision-det#)

[Unity project](https://github.com/nobnak/Test-EffSpaceUnity) of this module.

## Demo
[![Demo](http://img.youtube.com/vi/0fxcMapOaBQ/hqdefault.jpg)](https://www.youtube.com/shorts/0fxcMapOaBQ)
[![Demo](http://img.youtube.com/vi/_xvoNZ3kExc/hqdefault.jpg)](https://www.youtube.com/shorts/_xvoNZ3kExc)

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

## Usage

### Initialize
```csharp
var grid = 1 << 5;
var particleCount = 1000;
var screenSize = new int2(1920, 1080);
FPointGridExt.RecommendGrid(screenSize, grid, out var cellCount, out var cellSize);
for (var i = 0; i < particleCount; i++) {
    var pos = rand.NextFloat2(float2.zero, screenSize);
    element_ids[i] = grid.Insert(i, pos);
    particle_seeds[i] = pos;
    particle_pos[i] = pos;
    ...
}
```

### Update
```csharp
var t = Time.time;
for (var i = 0; i < particleCount; i++) {
    var element_id = element_ids[i];
    var seed = particle_seeds[i];
    var pos = particle_pos[i];
    if (element_id >= 0) grid.Remove(element_id);

    pos += dt * screenSize * new float2(
        noise.snoise(new float3(seed, t)),
        noise.snoise(new float3(-seed, t)));
    pos -= screenSize * math.floor(pos / screenSize);

    element_ids[i] = grid.Insert(i, pos);
    particle_pos[i] = pos;
}
```

### Query
```csharp
int Neareset(int i) {
    var pos = particle_pos[i];
    var element_id = element_ids[i];
    var min_dist_sq = float.MaxValue;
    var min_id = -1;
    foreach (var e in grid.Query(pos - qrange, pos + qrange)) {
        if (e == element_id) continue;

        var eq = grid.grid.elements[e];
        var pos1 = particle_pos[eq.id];
        var dist_sq = math.distancesq(pos1, pos);
        if (dist_sq < min_dist_sq) {
            min_dist_sq = dist_sq;
            min_id = e;
        }
    }
    return min_id;
}
```

## References
1. user4842163, [Efficient (and well explained) implementation of a Quadtree for 2D collision detection](https://stackoverflow.com/questions/41946007/efficient-and-well-explained-implementation-of-a-quadtree-for-2d-collision-det# ), Stackoveflow, 2018
2. Mark Farragher, [What Is Faster In C#: An int[] or an int[,]?](https://mdfarragher.medium.com/high-performance-arrays-in-c-2d55c04d37b5), 2019
