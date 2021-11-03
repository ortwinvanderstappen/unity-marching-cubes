# Marching Cubes, Unity implementation

## Project

This project was made with the purpose of learning the Marching Cubes algorithm.
Initially it started with creating all the different marching cube configurations in a single cube. 

## Stage 1
Initial chunk generation. The cube literally marches through the scene and generates a mesh.
![Chunk Scene](https://github.com/ortwinvanderstappen/unity-marching-cubes/blob/main/Images/Scene01.PNG?raw=true)

## Stage 2
Terrain generation. Noise generation was altered via a compute shader. The terrain is entirely customizable by tweaking noise values. 
Multiple chunks can be spawned depending on the player position, chunks are managed by a ChunkManager which allows for infinite terrain. 
![Terrain Scene](https://github.com/ortwinvanderstappen/unity-marching-cubes/blob/main/Images/Scene02.PNG?raw=true)

## What next?

### Noise values
The world of noise generation goes way beyond what I initially thought. I dipped my toes into altering terrain with noise and would love go dive deeper. Possibly generating mountains and cave systems.

### Terrain altering
It's possible to alter the vertex points noise values, a raycast "gun" that alters the noise values of certain points would allow for dynamic terrain alterations. Creating caves or hills by shooting a certain point.

### Visual shaders
The terrain currently has a very basic material, shading polygons depending on normal, height and position would be a great addition.

## Conclusions
I learned a lot and obtained the knowledge to dive deeper into the subject, this project has a good chance on being expanded upon in the future.
