using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThreeTileRenderer.DataTypes;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace ThreeTileRenderer
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TileChunkRenderer : MonoBehaviour
    {
        public TileAsset[] TileAssets;

        private Tile[] tilemap;    
        
        public const int ROWS = 8;
        public const int COLS = 8;
        private List<Vector3> resultVertices = new List<Vector3>();
        private List<int> resultTriangles = new List<int>();
        private List<Vector3> resultNormals = new List<Vector3>();
        private List<Vector2> resultUvs = new List<Vector2>();
        private List<Vector4> resultTangents = new List<Vector4>();
        private List<Color> resultColors = new List<Color>();
        
//        private int verticesCount;
//        private int trianglesCount;
        
        private bool isBuilding;
        private Mesh mesh;
        private Vector3 initialPosition;
        
        private static Quaternion[] rotationQuaternions = new Quaternion[]
        {
            Quaternion.identity,
            Quaternion.Euler(0, 90, 0),
            Quaternion.Euler(0, 180, 0),
            Quaternion.Euler(0, 270, 0), 
        };

        private void Start()
        {
            tilemap = new Tile[ROWS * COLS];
            var mf = GetComponent<MeshFilter>();
            mesh = mf.mesh;
            mesh.MarkDynamic();

            initialPosition = transform.position;
        }

        private void Update()
        {
            if (!isBuilding)
            {
                Build();
            }
        }

        private async void Build()
        {
            isBuilding = true;
            tilemap = await RandomizeTilemap();
            await Task.Run((Action)BuildMesh);
            
            mesh.SetVertices(resultVertices);
            await Task.Delay(1);
            mesh.SetTriangles(resultTriangles, 0);
            await Task.Delay(1);
            mesh.SetNormals(resultNormals);
            await Task.Delay(1);
            mesh.SetColors(resultColors);
            await Task.Delay(1);
            mesh.SetUVs(0, resultUvs);
            await Task.Delay(1);
            mesh.SetTangents(resultTangents);
            await Task.Delay(1);

//            await Task.Delay(TimeSpan.FromSeconds(3));
            isBuilding = false;
        }

        private void BuildMesh()
        {
            resultTriangles.Clear();
            resultVertices.Clear();
            resultNormals.Clear();
            resultTangents.Clear();
            resultUvs.Clear();
            resultColors.Clear();
            
            var verticesCount = 0;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    var index = row * COLS + col;
                    var tileIndex = tilemap[index];
                    var tileData = TileAssets[tileIndex.Id].Data;
                    var rotation = rotationQuaternions[(int)tileIndex.Rotation];

                    var tileOffset = new Vector3(
                        col,
                        0,
                        row
                    );

                    for (var vertexIndex = 0; vertexIndex < tileData.Vertices.Length; vertexIndex++)
                    {
                        resultVertices.Add(rotation * tileData.Vertices[vertexIndex] + tileOffset);
                        resultNormals.Add(rotation * tileData.Normals[vertexIndex]);
                        resultTangents.Add(tileData.Tangents[vertexIndex]);
                        resultUvs.Add(tileData.Uvs[vertexIndex]);
//                        resultColors.Add(tileData.Colors[vertexIndex]);
                    }

                    for (var triangleIndex = 0; triangleIndex < tileData.Triangles.Length; triangleIndex++)
                    {
                        resultTriangles.Add(tileData.Triangles[triangleIndex] + verticesCount);
                    }

                    verticesCount = resultVertices.Count;

//                    Thread.Sleep(100);
                }
            }
        }

        private Task<Tile[]> RandomizeTilemap()
        {
            var map = new Tile[ROWS * COLS];
            lock (TileAssets)
            {
                var max = TileAssets.Length;
                for (int r = 0; r < ROWS; r++)
                {
                    for (int c = 0; c < COLS; c++)
                    {
                        map[r * COLS + c] = new Tile
                        {
                            Id = UnityEngine.Random.Range(0, max),
                            Rotation = (TileRotation) UnityEngine.Random.RandomRange(0, 4),
                            FlipX = UnityEngine.Random.value > 0.5f,
                            FlipY = UnityEngine.Random.value > 0.5f
                        };
                    }
                }
            }

            return Task.FromResult(map);
        }

//        private Task<Vector4[]> TangentSolver(
//            Vector3[] vertices,
//            Vector3[] normals,
//            
//        )
//        {
//            var vertexCount = theMesh.vertexCount;
//            var vertices = theMesh.vertices;
//            var normals = theMesh.normals;
//            var texcoords = theMesh.uv;
//            var triangles = theMesh.triangles;
//            var triangleCount = triangles.Length/3;
//            var tangents = new Vector4[vertexCount];
//            var tan1 = new Vector3[vertexCount];
//            var tan2 = new Vector3[vertexCount];
//            var tri = 0;
//            for (var i = 0; i < (triangleCount); i++)
//            {
//                var i1 = triangles[tri];
//                var i2 = triangles[tri+1];
//                var i3 = triangles[tri+2];
//                var v1 = vertices[i1];
//                var v2 = vertices[i2];
//                var v3 = vertices[i3];
//                var w1 = texcoords[i1];
//                var w2 = texcoords[i2];
//                var w3 = texcoords[i3];
//                var x1 = v2.x - v1.x;
//                var x2 = v3.x - v1.x;
//                var y1 = v2.y - v1.y;
//                var y2 = v3.y - v1.y;
//                var z1 = v2.z - v1.z;
//                var z2 = v3.z - v1.z;
//                var s1 = w2.x - w1.x;
//                var s2 = w3.x - w1.x;
//                var t1 = w2.y - w1.y;
//                var t2 = w3.y - w1.y;
//                var r = 1.0f / (s1 * t2 - s2 * t1);
//                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
//                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
//                tan1[i1] += sdir;
//                tan1[i2] += sdir;
//                tan1[i3] += sdir;
//                tan2[i1] += tdir;
//                tan2[i2] += tdir;
//                tan2[i3] += tdir;
//                tri += 3;
//            }
//           
//            for (var i = 0; i < (vertexCount); i++)
//            {
//                var n = normals[i];
//                var t = tan1[i];
//               
//                // Gram-Schmidt orthogonalize
//                Vector3.OrthoNormalize( n, t );
//               
//                tangents[i].x  = t.x;
//                tangents[i].y  = t.y;
//                tangents[i].z  = t.z;
//           
//                // Calculate handedness
//                tangents[i].w = Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ? -1.0f : 1.0f;
//            }      
//            
//            return Task.FromResult(tangents);
//        }

//        public TileAsset[] TemporaryTileAssets;
//        private TileChunkBuilderJob builderJob; 
//
////        private int[] temporaryTileMap = new int[]
////        {
////            0, 0, 0, 0, 0, 0, 0, 1,
////            0, 0, 2, 2, 2, 2, 2, 1,
////            0, 0, 1, 0, 0, 0, 0, 1,
////            0, 0, 1, 0, 0, 0, 0, 1,
////            0, 0, 1, 0, 0, 0, 0, 1,
////            0, 0, 1, 0, 0, 0, 0, 1,
////            0, 0, 1, 0, 0, 0, 0, 1,
////            0, 0, 3, 3, 3, 3, 0, 1
////        };
//
//        private Mesh mesh;
//        private NativeArray<TilePrototypeStruct> prototypeStructs;
//        private NativeArray<int> tilemap;
//        private NativeArray<Vector3> vertices;
//        private NativeArray<int> triangles;
//        private JobHandle jobHandler;
//        private List<Vector3> resultVertices = new List<Vector3>();
//        private List<int> resultTriangles = new List<int>();
//        
//        public const int ROWS = 8;
//        public const int COLS = 8;
//
//        private struct TileChunkBuilderJob : IJobParallelFor
//        {
//            public NativeArray<TilePrototypeStruct> TilePrototypes;
//            public NativeArray<Vector3> Vertices;
//            public NativeArray<int> Triangles;
//            public NativeArray<int> TileMap;
//            public int Rows;
//            public int Cols;
//
//            public int VerticesCount;
//            public int TrianglesCount;
//            
//            public unsafe void Execute(int index)
//            {
//                var row = index / Cols;
//                var col = index % Cols;
//
//                var tileIndex = TileMap[index];
//                var tileData = TilePrototypes[tileIndex];
//                
//                var tileOffset = new Vector3(
//                    col,
//                    0,
//                    row
//                );
//                
//                for (var vertexIndex = 0; vertexIndex < tileData.VerticesCount; vertexIndex++)
//                {
//                    Vertices[vertexIndex + VerticesCount] = tileData.Vertices[vertexIndex] + tileOffset; 
//                }
//                
//                for (var triangleIndex = 0; triangleIndex < tileData.TrianglesCount; triangleIndex++)
//                {
//                    Triangles[triangleIndex + TrianglesCount] = tileData.Triangles[triangleIndex] + VerticesCount;
//                }
//
//                VerticesCount += tileData.VerticesCount;
//                TrianglesCount += tileData.TrianglesCount;
//            }
//        }
//
//        private void Start()
//        {
//            tilemap = new NativeArray<int>(ROWS * COLS, Allocator.Persistent);
//            prototypeStructs = new NativeArray<TilePrototypeStruct>(TemporaryTileAssets.Select(a => (TilePrototypeStruct) a.Data).ToArray(), Allocator.Persistent);
//            vertices = new NativeArray<Vector3>(65536, Allocator.Persistent);
//            triangles = new NativeArray<int>(65536, Allocator.Persistent);
//            var mf = GetComponent<MeshFilter>();
//            mesh = mf.mesh;
//            mesh.MarkDynamic();
//        }
//
//        private void Update()
//        {
//            RandomizeTileMap(ROWS, COLS);
//            builderJob = new TileChunkBuilderJob
//            {
//                TilePrototypes = prototypeStructs,
//                Vertices = vertices,
//                Triangles = triangles,
//                TileMap = tilemap,
//                Rows = ROWS,
//                Cols = COLS
//            };
//
//            jobHandler = builderJob.Schedule(65536, 1);
//        }
//
//        private void RandomizeTileMap(int rows, int cols)
//        {
//            var max = TemporaryTileAssets.Length;
//            for (int r = 0; r < rows; r++)
//            {
//                for (int c = 0; c < cols; c++)
//                {
//                    tilemap[r * cols + c] = Random.Range(0, max);
//                }
//            }
//        }
//
//        private void LateUpdate()
//        {
//            jobHandler.Complete();
//
//            var verticesCount = builderJob.VerticesCount;
//
//            if (resultVertices.Count > verticesCount)
//            {
//                resultVertices.RemoveRange(verticesCount, resultVertices.Count - verticesCount);
//            } 
//            else if (resultVertices.Count < verticesCount)
//            {
//                resultVertices.Capacity = verticesCount;
//            }
//
//            for (int vertexIndex = 0; vertexIndex < verticesCount; vertexIndex++)
//            {
//                resultVertices[vertexIndex] = builderJob.Vertices[vertexIndex];
//            }
//            
//            
//            var trianglesCount = builderJob.TrianglesCount;
//
//            if (resultTriangles.Count > trianglesCount)
//            {
//                resultTriangles.RemoveRange(trianglesCount, resultTriangles.Count - trianglesCount);
//            } 
//            else if (resultTriangles.Count < trianglesCount)
//            {
//                resultTriangles.Capacity = trianglesCount;
//            }
//
//            for (int triangleIndex = 0; triangleIndex < trianglesCount; triangleIndex++)
//            {
//                resultTriangles[triangleIndex] = builderJob.Triangles[triangleIndex];
//            }
//
//            mesh.SetTriangles(resultTriangles, 0);
//        }
//        
//        private void OnDestroy()
//        {
//            prototypeStructs.Dispose();
//            tilemap.Dispose();
//            vertices.Dispose();
//            triangles.Dispose();
//        }
    }
}