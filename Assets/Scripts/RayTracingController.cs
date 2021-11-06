using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Profiling;
#endif

[RequireComponent(typeof(Camera))]
public class RayTracingController : MonoBehaviour
{
    public ComputeShader RTShader;
    public Texture skyboxTexture;

    [Header("Tracing")]
    public bool doTracing = true;
    [Range(0.001f, 1)] public float renderScale = 1f;
    [Range(0,7)] public int rayBounces;

    [Header("Lighting")]
    [Range(0, 1)] public float skyboxExposure = 1f;
    [Min(0)] public float sunIntensity = 1f;
    public Vector2 sunRotation;

    [Header("Scene Gen")]
    [Min(1)] public int maxSpheres = 1;
    [Min(1)] public int maxBoxes = 1;
    [Min(0)] public int placementRadius = 5;
    [Min(0)] public Vector2 sphereRadiusMinMax = new Vector2(0.25f, 1.5f);
    [Min(0)] public Vector2 boxRadiusMinMax = new Vector2(0.25f, 1.5f);
    [Range(0,1)] public float cameraSphereRadius = 0.25f;



    private RenderTexture target;
    private RenderTextureDescriptor descriptor;
    private int renderWidth, renderHeight;

    private new Camera camera;

    private Sphere[] spheres;
    private ComputeBuffer sphereBuffer;

    private Box[] boxes;
    private ComputeBuffer boxBuffer;

    #region Unity Methods
    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        CreateScene();
    }

    private void OnDisable()
    {
        sphereBuffer?.Release();
        boxBuffer?.Release();
    }
    #endregion

    #region Rendering
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!doTracing) { Graphics.Blit(source, destination); return; }

        renderWidth  = Mathf.CeilToInt(Screen.width  * renderScale);
        renderHeight = Mathf.CeilToInt(Screen.height * renderScale);

        SetShaderPerameters();
        RenderShaders(destination);
    }

    private void RenderShaders(RenderTexture destination)
    {
        RenderUtils.InitRenderTexture(ref target, GetDescriptor());

        RTShader.SetTexture(0, "Result", target);
        int threadGroupsX = Mathf.CeilToInt(renderWidth  / 8f);
        int threadGroupsY = Mathf.CeilToInt(renderHeight / 8f);
        RTShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);
    }

    private RenderTextureDescriptor GetDescriptor()
    {
        if(descriptor.Equals(default(RenderTextureDescriptor)) 
            || descriptor.width != renderWidth || descriptor.height != renderHeight)
        {
            descriptor = new RenderTextureDescriptor(renderWidth, renderHeight, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                sRGB = false
            };
        }

        return descriptor;
    }

    //private void InitRenderTexture()
    //{
    //    if(target == null || target.width != Screen.width || target.height != Screen.height)
    //    {
    //        if (target != null)
    //            target.Release();

    //        target = new RenderTexture(Screen.width, Screen.height, 0, 
    //            RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

    //        target.enableRandomWrite = true;
    //        target.Create();
    //    }
    //}

    private void SetShaderPerameters()
    {
        //@Refactor: find a way to optimize passing all this data

        Vector3 cPos = camera.transform.position;
        RTShader.SetVector("_CameraData", new Vector4(cPos.x, cPos.y, cPos.z, cameraSphereRadius));
        RTShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        RTShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);

        if (skyboxTexture == null)
            skyboxTexture = Texture2D.redTexture;
        //skyboxTexture ??= Texture2D.redTexture; 

        RTShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
        RTShader.SetFloat("_SkyboxExposure", skyboxExposure);

        RTShader.SetInt("_Bounces", rayBounces);
        
        Vector3 lForward = Quaternion.Euler(sunRotation) * Vector3.forward;
        RTShader.SetVector("_DirectionalLight", new Vector4(lForward.x, lForward.y, lForward.z, sunIntensity));
    }
    #endregion

    

    public void RegenerateScene()
    {
        sphereBuffer?.Release();
        boxBuffer?.Release();
        CreateScene();
    }

    private void CreateScene()
    {
        spheres = GenerateRandomSpheres();

        // Send data to the compute shader
        sphereBuffer = new ComputeBuffer(spheres.Length, sizeof(float) * 10, ComputeBufferType.Structured);
        sphereBuffer.SetData(spheres);
        RTShader.SetBuffer(0, "_SphereBuffer", sphereBuffer);

        //boxes = GenerateRandomBoxes();

        //boxBuffer = new ComputeBuffer(boxes.Length, sizeof(float) * 33, ComputeBufferType.Structured);
        //boxBuffer.SetData(boxes);
        //RTShader.SetBuffer(0, "_BoxBuffer", boxBuffer);

    }

    #region Spheres
    
    private struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }

    private Sphere[] GenerateRandomSpheres()
    {
        Sphere[] spheres = new Sphere[maxSpheres];

        for (int i = 0; i < spheres.Length; i++)
        {
            float randRadius = Random.Range(sphereRadiusMinMax.x, sphereRadiusMinMax.y);

            Vector2 randomXY = Random.insideUnitCircle * placementRadius/*maxSpheres / 4*/;
            Vector3 randomPosition = new Vector3(randomXY.x, randRadius, randomXY.y);

            foreach (Sphere other in spheres)
            {
                float overlapDist = other.radius + randRadius;

                if ((other.position - randomPosition).sqrMagnitude < overlapDist * overlapDist)
                {
                    goto SkipSphere;
                }
            }
            
            
            var randCol = RenderUtils.ColorToVector3(Random.ColorHSV());
            bool metal = Random.value < 0.5f; //@Refactor: Convert to more PBR-like workflow?

            var s = new Sphere()
            {
                position = randomPosition,
                radius   = randRadius,
                albedo   = metal ? Vector3.zero : randCol,
                specular = metal ? randCol : Vector3.one * Random.Range(2, 7) * 0.01f
            };

            spheres[i] = s;

            SkipSphere:
                continue;
        }
        
        return spheres;
    }
    #endregion

    private struct Box
    {
        public Matrix4x4 worldToBox;
        public Matrix4x4 boxToWorld;
        public float lengthRadius;
    }

    Box[] GenerateRandomBoxes()
    {
        Box[] boxes = new Box[maxBoxes];

        for(int i = 0; i < boxes.Length; i++)
        {
            float randRadius = Random.Range(boxRadiusMinMax.x, boxRadiusMinMax.y);
            
            Vector2 randomXY = Random.insideUnitCircle * placementRadius;
            Vector3 randomPos = new Vector3(randomXY.x, randRadius, randomXY.y);

            //foreach(Box other in boxes)
            //{
            //    // Abort this box if it intersects a previous one
            //    // idk what to do for this
            //}

            Quaternion randRot = Random.rotationUniform;
            Vector3 randScale = new Vector3(randRadius, randRadius, randRadius) * 2;

            Matrix4x4 matrix = Matrix4x4.TRS(randomPos, Quaternion.identity, randScale);

            var b = new Box
            {
                //@Incomplete: matrix stuff is incorrect
                worldToBox = matrix.inverse,
                boxToWorld = matrix,
                lengthRadius = randRadius
            };

            boxes[i] = b;
        }

        return boxes;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if(spheres != null)
            {
                foreach(Sphere s in spheres)
                {
                    Gizmos.DrawSphere(s.position, s.radius);
                }
            }
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(RayTracingController))]
public class RayTracingControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate Scene") && Application.isPlaying)
            (target as RayTracingController).RegenerateScene();
        
        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Only functions in Play Mode", MessageType.Warning, true);
    }
}
#endif