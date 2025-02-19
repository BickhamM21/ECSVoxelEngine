﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public class VoxelGenerationSystem : ComponentSystem
{
    EntityQuery chunksQuery;
    EntityQuery chunkManagerQuery;

    ComputeShader voxelDataGenerator = Resources.Load<ComputeShader>("GenerateVoxelData");
    ComputeBuffer voxelBuffer;

    AsyncGPUReadbackRequest request;

    int GenerateVoxelDataKernel;

    bool hasDispatched = false;

    int chunkID = -1;
    NativeArray<float> voxelData;

    protected override void OnCreate()
    {
        chunksQuery = World.Active.EntityManager.CreateEntityQuery(typeof(ChunkInfo));
        chunkManagerQuery = World.Active.EntityManager.CreateEntityQuery(typeof(VoxelData), typeof(ChunkManagerInfo));

        voxelBuffer = new ComputeBuffer(17 * 17 * 17, sizeof(float));
        GenerateVoxelDataKernel = voxelDataGenerator.FindKernel("GenerateVoxelData");
        voxelDataGenerator.SetBuffer(GenerateVoxelDataKernel, "VoxelData", voxelBuffer);

    }

    


    protected override void OnUpdate()
    {

        NativeArray<Entity> chunkManager = chunkManagerQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> chunks = chunksQuery.ToEntityArray(Allocator.TempJob);

        Entity chunkManagerEntity = chunkManager[0];

        chunkManager.Dispose();


        if (World.Active.EntityManager.GetComponentData<ChunkManagerInfo>(chunkManagerEntity).status == 0) {
            if (hasDispatched == false)
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    if (World.Active.EntityManager.GetComponentData<ChunkInfo>(chunks[i]).chunkProgress == 0)
                    {
                        chunkID = i;
                        break;
                    }
                    else
                    {
                        chunkID = -1;
                    }
                }

                if (chunkID == -1)
                {
                    chunks.Dispose();
                    return;
                }

                Entity chunk = chunks[chunkID];
                chunks.Dispose();



                ChunkInfo chunkInfo = World.Active.EntityManager.GetComponentData<ChunkInfo>(chunk);

                voxelDataGenerator.SetInt("SizeX", chunkInfo.size.x + 1);
                voxelDataGenerator.SetInt("SizeY", chunkInfo.size.y + 1);
                voxelDataGenerator.SetInt("SizeZ", chunkInfo.size.z + 1);

                voxelDataGenerator.SetInt("x", chunkInfo.pos.x);
                voxelDataGenerator.SetInt("y", chunkInfo.pos.y);
                voxelDataGenerator.SetInt("z", chunkInfo.pos.z);



                voxelDataGenerator.Dispatch(GenerateVoxelDataKernel, (int)math.ceil((chunkInfo.size.x + 1)/ 1), (int)math.ceil((chunkInfo.size.y + 1)/ 1), (int)math.ceil((chunkInfo.size.z + 1) / 1));
                hasDispatched = true;
                request = AsyncGPUReadback.Request(voxelBuffer);

            }



            if (request.done)
            {

                if(request.hasError == true)
                {
                    throw new System.NotImplementedException();
                }

                hasDispatched = false;

                voxelData = request.GetData<float>();

                DynamicBuffer<float> voxelDynamicBuffer = World.Active.EntityManager.GetBuffer<VoxelData>(chunkManagerEntity).Reinterpret<float>();
                voxelDynamicBuffer.CopyFrom(voxelData);

                for(int i = 0; i < 17 * 17 * 17; i++)
                {
                    Debug.Log(voxelData[i]);
                }
                //World.Active.EntityManager.SetComponentData<VoxelData>(chunkManagerEntity, voxelDynamicBuffer.Reinterpret<VoxelData>());

                ChunkInfo chunkInfo = World.Active.EntityManager.GetComponentData<ChunkInfo>(chunks[chunkID]);
                chunkInfo.chunkProgress = 1;

                World.Active.EntityManager.SetComponentData(chunks[chunkID], chunkInfo);

                ChunkManagerInfo managerInfo = World.Active.EntityManager.GetComponentData<ChunkManagerInfo>(chunkManagerEntity);

                managerInfo.status = 1;
                World.Active.EntityManager.SetComponentData(chunkManagerEntity, managerInfo);



            }


        }




        //throw new System.NotImplementedException();
    }
}
