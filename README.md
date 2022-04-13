# An implementation (for Unity) of efficient spatial data structures from [Stackoverflow thread](https://stackoverflow.com/questions/41946007/efficient-and-well-explained-implementation-of-a-quadtree-for-2d-collision-det#)

[Unity project](https://github.com/nobnak/Test-EffSpaceUnity) of this module.

## Demo
[![Demo](http://img.youtube.com/vi/0fxcMapOaBQ/hqdefault.jpg)](https://www.youtube.com/shorts/0fxcMapOaBQ)

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
