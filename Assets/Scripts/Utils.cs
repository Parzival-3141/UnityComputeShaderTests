using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderUtils
{
	public static void InitRenderTexture(ref RenderTexture texture, RenderTextureDescriptor descriptor)
	{
		if (texture == null || texture.width != descriptor.width || texture.height != descriptor.height)
		{
			if (texture != null)
				texture.Release();

			texture = new RenderTexture(descriptor);
			texture.Create();
		}
	}

	public static Vector3 ColorToVector3(Color color)
	{
		return new Vector3(color.r, color.g, color.b);
	}
}
