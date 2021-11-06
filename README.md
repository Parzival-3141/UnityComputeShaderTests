# UnityComputeShaderTests
A few tests with compute shaders, including CPU Raytracing.

Most of this is in an unfinished state.

## Raytracing
Raytracing is a little slow on my machine, passing data to the GPU could probably be optimized.

Only really supports spheres at this point.

## Align Sun from HDRI
An attempt to automatically calculate a direction vector from an HDRI.

Converts the image to grayscale and blurs to filter out stray bright pixels. Blur algorithm is an attempt at Gaussian blur, however my implementation is incorrect.

After fixing the blur, the next step would be to find the brightest pixel(s) on the texture and project their XY position into 3D space.



### Built in Unity 2019.4
