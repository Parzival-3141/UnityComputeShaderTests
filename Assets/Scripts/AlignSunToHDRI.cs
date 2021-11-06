using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignSunToHDRI : MonoBehaviour
{
    public Texture2D hdri;
    public ComputeShader computeShader;
    [Space]
    public bool doGrayscale = true;
    [Space]
    public bool doBlur = true;
    public int sigma = 4;
    [Range(0,1)]public float exposure = 1f;

    private RenderTexture renderTexture;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderPerameters();

        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        RenderUtils.InitRenderTexture(ref renderTexture, GetDescriptor());

        //Graphics.CopyTexture(hdri, grayscaled);
        Graphics.Blit(hdri, renderTexture);

        RunShader("ConvertToGrayscale", "_Texture", renderTexture);
        RunShader("Blur", "_Texture", renderTexture);
        RunShader("SetExposure", "_Texture", renderTexture);

        Graphics.Blit(renderTexture, destination);
    }


    private void RunShader(string kernelName, string RWTextureName, RenderTexture input)
    {
        int kernel = computeShader.FindKernel(kernelName);

        computeShader.SetTexture(kernel, RWTextureName, input);
        int threadGroupsX = Mathf.CeilToInt(input.width / 8f);
        int threadGroupsY = Mathf.CeilToInt(input.height / 8f);
        computeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
    }

    private void SetShaderPerameters()
    {
        computeShader.SetBool("_DoGrayscale", doGrayscale);
        computeShader.SetBool("_DoBlur", doBlur);

        computeShader.SetInt("_Sigma", sigma);
        computeShader.SetFloat("_Exposure", exposure);
    }

    private RenderTextureDescriptor GetDescriptor()
    {
        RenderTextureDescriptor descriptor;

        if (SystemInfo.IsFormatSupported(hdri.graphicsFormat, UnityEngine.Experimental.Rendering.FormatUsage.Render))
            descriptor = new RenderTextureDescriptor(hdri.width, hdri.height, hdri.graphicsFormat, 0, hdri.mipmapCount);
        else
            descriptor = new RenderTextureDescriptor(hdri.width, hdri.height, RenderTextureFormat.ARGBFloat, 0, hdri.mipmapCount);

        descriptor.sRGB = false;
        descriptor.enableRandomWrite = true;
        //descriptor.autoGenerateMips = false;
        //descriptor.useMipMap = true;

        return descriptor;
    }
}
