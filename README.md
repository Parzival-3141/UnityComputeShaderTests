# UnityComputeShaderTests
![Raytracing screenshot 1](/Screenshots/ComputeShaderRayTracerTest2.png)
A few tests with compute shaders, including GPU Raytracing.

Most of this is in an unfinished state.

## Raytracing
![Raytracing screenshot 2](/Screenshots/ComputeShaderRayTracerTest1.png)
Raytracing is a little slow on my machine, passing data to the GPU could probably be optimized.

Only really supports spheres at this point. 

(Yes, the player is also a sphere :slightly_smiling_face:)

## Align Sun from HDRI
An attempt to automatically calculate a direction vector from an HDRI.

Converts the image to grayscale and blurs to filter out stray bright pixels. Blur algorithm is an attempt at Gaussian blur, however my implementation is incorrect.

After fixing the blur, the next step would be to find the brightest pixel(s) on the texture and project their XY position into 3D space.



### Built in Unity 2019.4
