//Set child object from fractal positions group to the all reference points and make them based on the reference points.
// like :1234 2345 3456...


using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

public class AllDIrectionFractal : MonoBehaviour
{

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor
    {

        public NativeArray<float3> positions;

        public NativeArray<float> fractalSizes;

        public NativeArray<float3> fractalPositions;

        public float scale;

        [WriteOnly]
        public NativeArray<float3x4> matrices;

        public void Execute(int i)
        {

            float3 p = positions[i];

            quaternion rotation = Quaternion.LookRotation(p, Vector3.up);

            float3 f = fractalPositions[i];

             p = p + mul(rotation, f);

            float3x3 r = float3x3(rotation) * scale * fractalSizes[i];
            matrices[i] = float3x4(r.c0, r.c1, r.c2, p);

        }
    }

    static readonly int matricesId = Shader.PropertyToID("_Matrices");

    static MaterialPropertyBlock propertyBlock;


    public int num = 4; //Fractal child number.


    public Mesh mesh; //Fractal mesh.


    public Material material;//Fractal material URP.

    NativeArray<float3> positions; //Original reference positions.
    int numVertices; //Original reference vertices number.

    NativeArray<float3> fractalPositions; //Fractal pattern mesh positions.

    NativeArray<float> fractalSizes; //Each level sizes.

    [SerializeField]
    Mesh meshRef; //Original reference mesh.

    [SerializeField]
    float scale; //Original mesh scale.

    [SerializeField]
    float ScaleRef; //Original mesh scale.



    NativeArray<float3x4> matrices;

    ComputeBuffer matricesBuffers;


    public virtual void SetMesh()
    {
        SetFractalMesh();
    }

    void SetFractalMesh()
    {
        mesh = AvatarMeshNow.avatarMesh;
    }




    void CreateMeshFractalStart()
    {
        numVertices = meshRef.vertices.Length;

        num = Positions.fractalPositions.Length;

        fractalSizes = new NativeArray<float>(numVertices * num, Allocator.Persistent);

        positions = new NativeArray<float3>(numVertices * num, Allocator.Persistent);



        fractalPositions = new NativeArray<float3>(numVertices * num, Allocator.Persistent);


        //Set original position to each fractal group position. (0000 1111 2222...)
        for (int i = 0; i < numVertices; i++)
        {
            for(int j = 0; j< num; j++)
            {
               
                Vector3 m = meshRef.vertices[i];
                positions[i * num + j] = new float3(m.x, m.y, m.z) * ScaleRef;
            }
            
        }


        //Get each child from fractal group positions and set it to array. (1234 1234 1234...)
        for(int i = 0; i< numVertices; i++)
        {
            for(int j = 0; j< num; j++)
            {
               
                Vector3 m = Positions.fractalPositions[j];
                fractalPositions[i * num + j] = m * 0.04f;

                float s = Sizes.fractalSizes[j];
                fractalSizes[i * num + j] = s;
            }
        }


            matrices = new NativeArray<float3x4>();

        int stride = 12 * 4;


        matrices = new NativeArray<float3x4>(numVertices * num, Allocator.Persistent);
        matricesBuffers = new ComputeBuffer(numVertices * num, stride);


        propertyBlock ??= new MaterialPropertyBlock();


    }

    void OnDisable()
    {
        positions.Dispose();

        fractalPositions.Dispose();

        fractalSizes.Dispose();

        matricesBuffers.Release();

        matrices.Dispose();

    }

    public virtual void CreateMeshFractal()
    {
        CreateMeshFractalStart();
    }

    public virtual void EnableJobMeshFractal()
    {
        DoMeshFractalJob();
    }


    void DoMeshFractalJob()
    {

        JobHandle jobHandle = default;

   
            
                
                jobHandle = new UpdateFractalLevelJob
                {
                    scale  = scale,
                    positions = positions,
                    fractalPositions = fractalPositions,
                    fractalSizes = fractalSizes,
                    matrices = matrices
                }.ScheduleParallel(matrices.Length, 1, jobHandle);
            

        jobHandle.Complete();


        var bounds = new Bounds(this.transform.position, 3f * Vector3.one);

        ComputeBuffer buffer = matricesBuffers;
        buffer.SetData(matrices);
        propertyBlock.SetBuffer(matricesId, buffer);
        Graphics.DrawMeshInstancedProcedural(
            mesh, 0, material, bounds, buffer.count, propertyBlock
        );

    }


}