using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ThreeTileRenderer.DataTypes
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TilePrototypeStruct
    {
        [FieldOffset(    0)] public unsafe Vector3* Vertices;
        [FieldOffset( 996)] public int VerticesCount;
        [FieldOffset(1000)] public unsafe int* Triangles;
        [FieldOffset(1996)] public int TrianglesCount;
        [FieldOffset(2000)] public unsafe Vector3* Normals;
        [FieldOffset(2996)] public int NormalsCount;
        [FieldOffset(3000)] public unsafe Vector2* Uvs;
        [FieldOffset(3996)] public int UvsCount;
        [FieldOffset(4000)] public unsafe Vector4* Tangents;
        [FieldOffset(4996)] public int TangentsCount;
        [FieldOffset(5000)] public unsafe Color* Colors;
        [FieldOffset(5996)] public int ColorsCount;

        public unsafe TilePrototypeStruct(Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uvs, Vector4[] tangents, Color[] colors)
        {
            Vertices  = (Vector3*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Vector3>() * (long) vertices.Length,  UnsafeUtility.AlignOf<Vector3>(), Allocator.Persistent);
            Triangles = (int*)     UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>()     * (long) triangles.Length, UnsafeUtility.AlignOf<int>(),     Allocator.Persistent);
            Normals   = (Vector3*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Vector3>() * (long) normals.Length,   UnsafeUtility.AlignOf<Vector3>(), Allocator.Persistent);
            Uvs       = (Vector2*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Vector2>() * (long) uvs.Length,       UnsafeUtility.AlignOf<Vector2>(), Allocator.Persistent);
            Tangents  = (Vector4*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Vector4>() * (long) tangents.Length,  UnsafeUtility.AlignOf<Vector4>(), Allocator.Persistent);
            Colors    = (Color*)   UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Color>()   * (long) colors.Length,    UnsafeUtility.AlignOf<Color>(),   Allocator.Persistent);
            
            // set buffer lenghts
            VerticesCount   = vertices.Length;
            TrianglesCount  = triangles.Length;
            NormalsCount    = normals.Length;
            UvsCount        = uvs.Length;
            TangentsCount   = tangents.Length;
            ColorsCount     = colors.Length;
        }
    }
    
    [System.Serializable]
    public class TilePrototypeData
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector3[] Normals;
        public Vector2[] Uvs;
        public Vector4[] Tangents;
        public Color[] Colors;

        public bool IsValid => Vertices.Length > 0 && Normals.Length == Vertices.Length && Uvs.Length == Vertices.Length &&
                               Tangents.Length == Vertices.Length && (Colors.Length == 0 || Colors.Length == Vertices.Length);

        public static implicit operator TilePrototypeStruct(TilePrototypeData data)
        {
            return new TilePrototypeStruct(
                data.Vertices,
                data.Triangles,
                data.Normals,
                data.Uvs,
                data.Tangents,
                data.Colors
            );
            {
//                Vertices = new NativeArray<Vector3>(data.Vertices, Allocator.Persistent),
//                Triangles = new NativeArray<int>(data.Triangles, Allocator.Persistent),
//                Normals = new NativeArray<Vector3>(data.Normals, Allocator.Persistent),
//                Uvs = new NativeArray<Vector2>(data.Uvs, Allocator.Persistent),
//                Tangents = new NativeArray<Vector4>(data.Tangents, Allocator.Persistent),
//                Colors = new NativeArray<Color>(data.Colors, Allocator.Persistent)
            };
            
        }
    }
}
