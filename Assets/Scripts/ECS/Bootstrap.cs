using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using static Unity.Mathematics.math;

using Unity.Mathematics;
public class Bootstrap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var entityManager = World.Active.EntityManager;


        EntityArchetype chunkManager = entityManager.CreateArchetype(typeof(VoxelData), typeof(ChunkManagerInfo));
        EntityArchetype chunk = entityManager.CreateArchetype(typeof(ChunkInfo));


        NativeArray<Entity> chunkManagerEntity = new NativeArray<Entity>(1,Allocator.Persistent);

        entityManager.CreateEntity(chunkManager, chunkManagerEntity);

        ChunkManagerInfo managerInfo = new ChunkManagerInfo
        {
            status = 0
        };

        entityManager.SetComponentData(chunkManagerEntity[0], managerInfo);




        NativeArray<Entity> chunks = new NativeArray<Entity>(8 * 8 * 8, Allocator.Persistent);

        entityManager.CreateEntity(chunk, chunks);



        for(int i = 0; i < chunks.Length; i++)
        {
            int z = i / ((8) * (8));
            int b = i - (((8) * (8)) * z);
            int y = b / (8);
            int x = b % (8);

            ChunkInfo chunkInfo = new ChunkInfo
            {
                chunkProgress = 0,
                size = int3(16),
                pos = int3(x * 16,y * 16,z * 16)


            };

            entityManager.SetComponentData(chunks[i], chunkInfo);


        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
