using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

public struct ChunkInfo : IComponentData
{
    public int chunkProgress;
    public int3 pos;
    public int3 size;
}

//[InternalBufferCapacity(65 * 65 *65)]
public struct VoxelData : IBufferElementData
{

    public float d;


}

public struct ChunkManagerInfo : IComponentData
{
    public int status;

}