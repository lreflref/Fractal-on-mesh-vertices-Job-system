//Create multiple levels fractal pattern.
//Set all fractal child positions and sizes in positions and sizes group.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using System;

public class Fractal : MonoBehaviour {

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	struct UpdateFractalLevelJob : IJobFor {

		public float scale;

		[ReadOnly]
		public NativeArray<FractalPart> parents;

		public NativeArray<FractalPart> parts;

		[WriteOnly]
		public NativeArray<float3x4> matrices;

		public void Execute (int i) {
			FractalPart parent = parents[i / 4];
			FractalPart part = parts[i];
			part.worldRotation = quaternion.identity;
			part.worldPosition =
				parent.worldPosition +
				mul(parent.worldRotation, 1.75f * scale * part.direction);
			parts[i] = part;

			float3x3 r = float3x3(part.worldRotation) * scale;
			matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
		}
	}

	struct FractalPart {
		public float3 direction, worldPosition;
		public quaternion rotation, worldRotation;
		public float size;
	
	}


	static float3[] directions = {

		new float3(1,1,1),
		new float3(1,-1,1),
		new float3(-1, 1, 1),
		new float3(-1,-1, 1)
	};


	[SerializeField, Range(1, 8)]
	int depth = 4;


	[SerializeField]
	float scaleNum;

	NativeArray<FractalPart>[] parts;

	NativeArray<float3x4>[] matrices;

	ComputeBuffer[] matricesBuffers;


	List<Vector3> postions;
	List<float> sizes;

	void EnableFractal() {
		parts = new NativeArray<FractalPart>[depth];
		matrices = new NativeArray<float3x4>[depth];
		matricesBuffers = new ComputeBuffer[depth];
		int stride = 12 * 4;
		for (int i = 0, length = 1; i < parts.Length; i++, length *= 4) {
			parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
			matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
			matricesBuffers[i] = new ComputeBuffer(length, stride);
		}

		parts[0][0] = CreatePart(0,0);
		for (int li = 1; li < parts.Length; li++) {
			NativeArray<FractalPart> levelParts = parts[li];
			for (int fpi = 0; fpi < levelParts.Length; fpi += 4) {

				for (int ci = 0; ci < 4; ci++) {
					levelParts[fpi + ci] = CreatePart(ci, li);


                }
			}
		}

	}

	void OnDisable () {
		for (int i = 0; i < matricesBuffers.Length; i++) {
			matricesBuffers[i].Release();
			parts[i].Dispose();
			matrices[i].Dispose();
		}
		parts = null;
		matrices = null;
		matricesBuffers = null;
	}


	public virtual void EnableFractalCreation()
	{
	 	EnableFractal();

    }

	public virtual void DoFractalJob()
	{
		FractalJob();

    }

	FractalPart CreatePart (int childIndex, int level) => new FractalPart {
		direction = directions[childIndex],
		size = pow(scaleNum, level)
	};

	void FractalJob () {
		FractalPart rootPart = parts[0][0];
		rootPart.worldRotation = Quaternion.identity;
		rootPart.worldPosition = transform.position;
		parts[0][0] = rootPart;
		float objectScale = transform.lossyScale.x;
		float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
		matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

		float scale = objectScale;
		JobHandle jobHandle = default;

            for (int li = 1; li < parts.Length; li++)
            {		
                scale *= 0.5f;
                jobHandle = new UpdateFractalLevelJob
                {
                    scale = scale,
                    parents = parts[li - 1],
                    parts = parts[li],
                    matrices = matrices[li]
                }.ScheduleParallel(parts[li].Length, 4, jobHandle);
            }
            jobHandle.Complete();

		    SetPosi();

    }


	void SetPosi()
	{
		postions = new List<Vector3>();
		sizes = new List<float>();

        for (int i = 0, length = 1; i < parts.Length; i++, length *= 4)
        {
            for(int j = 0; j< length; j++)
			{
				Vector3 p = parts[i][j].worldPosition;
				float s = parts[i][j].size;

				postions.Add(p);
				sizes.Add(s);

			}
                      
        }

		Positions.fractalPositions = new Vector3[postions.Count];
		Sizes.fractalSizes = new float[sizes.Count];


        for (int i = 0; i < postions.Count; i++)
		{
			Positions.fractalPositions[i] = postions[i];
			Sizes.fractalSizes[i] = sizes[i];

        }


    }
}