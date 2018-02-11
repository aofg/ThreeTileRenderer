using System;
using System.Collections.Generic;
using System.Linq;
using ThreeTileRenderer.DataTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

namespace ThreeTileRenderer.Editor
{
	public class TilePrepareEditor : EditorWindow {
		public float FindTolerance;

		private Texture2D whiteRect;

		[MenuItem("Tile Renderer/Prepare Asset Window")]
		public static void OpenTilePrepare()
		{
			var instance = GetWindow<TilePrepareEditor>();
			instance.Show();
		}

		private void OnEnable()
		{
			whiteRect = new Texture2D(1,1);
			whiteRect.SetPixel(0,0,Color.white);
			whiteRect.Apply();
		
			FindTolerance = EditorPrefs.GetFloat("TTR_FindTolerance", 0.0001f);
		
			if (SceneView.onSceneGUIDelegate != OnSceneGUI)
			{
				SceneView.onSceneGUIDelegate += OnSceneGUI;
			}
		}

		private void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		private void OnGUI()
		{
			FindTolerance = EditorGUILayout.Slider(FindTolerance, Mathf.Epsilon, 0.01f);
			EditorPrefs.SetFloat("TTR_FindTolerance", FindTolerance);


			if (GUILayout.Button("Generate Tile Asset"))
			{
				Generate();
			}
		}

		private void Generate()
		{
			var selectedRoot = Selection.activeGameObject;

			if (!selectedRoot ||
			    !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(selectedRoot)) ||
			    selectedRoot.transform.parent)
			{
				return;
			}
			
			var resultVertices = new List<Vector3>();
			var resultTriangles = new List<int>();
			var resultNormals = new List<Vector3>();
			var resultUvs = new List<Vector2>();
			var resultTangents = new List<Vector4>();
			var resultColors = new List<Color>();
			
			var meshFilters = selectedRoot.GetComponentsInChildren<MeshFilter>();
			var selectedTransform = selectedRoot.transform;
			var centerPoint = selectedRoot.transform.position;
			var minPoint = centerPoint - Vector3.right * 0.5f - Vector3.forward * 0.5f;
			var maxPoint = centerPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f;

			List<Vector3> worldVertices = new List<Vector3>();
//			List<Vector3> tileVertices = new List<Vector3>();
//			List<int> triangles = new List<int>();
			
			Vector3 lowerPoint = Vector3.positiveInfinity;
			Vector3 lowerCenterPoint = default(Vector3);
			float offset;

			
			// Found all world vertices 
			foreach (var meshFilter in meshFilters)
			{
				var mesh = meshFilter.mesh;
				var transform = meshFilter.transform;
				var vertices = mesh.vertices;
				var triangles = mesh.triangles;
				var worldMatrix = transform.localToWorldMatrix;
//				var rootLocalMatrix = selectedTransform.worldToLocalMatrix;
				var meshOffset = worldVertices.Count;
				worldVertices.AddRange(vertices.Select(v => worldMatrix.MultiplyPoint(v)));
				resultColors.AddRange(mesh.colors);
				resultNormals.AddRange(mesh.normals.Select(n => worldMatrix.rotation * n));
				resultTangents.AddRange(mesh.tangents.Select(t =>
				{
					var v3 = worldMatrix.rotation * t;
					return new Vector4(v3.x, v3.y, v3.z, t.w);
				}));
				resultUvs.AddRange(mesh.uv);
				
				for (var triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex+=3)
				{
					var vert0 = worldVertices[meshOffset + triangles[triangleIndex + 0]];
					var vert1 = worldVertices[meshOffset + triangles[triangleIndex + 1]];
					var vert2 = worldVertices[meshOffset + triangles[triangleIndex + 2]];

					bool outMinX = OutBounds(vert0.x, vert1.x, vert2.x, minPoint.x);
					bool outMaxX = OutBounds(vert0.x, vert1.x, vert2.x, maxPoint.x);
					bool outMinZ = OutBounds(vert0.z, vert1.z, vert2.z, minPoint.z);
					bool outMaxZ = OutBounds(vert0.z, vert1.z, vert2.z, maxPoint.z);

					if (!outMinX && !outMaxX && !outMinZ && !outMaxZ)
					{
						resultTriangles.Add(meshOffset + triangles[triangleIndex + 0]);
						resultTriangles.Add(meshOffset + triangles[triangleIndex + 1]);
						resultTriangles.Add(meshOffset + triangles[triangleIndex + 2]);
					}
				}
			}
			
			// Found lower center point
			foreach (var vertex in worldVertices)
			{
				if (vertex.y < lowerPoint.y)
				{
					lowerPoint = vertex;
				}
			}
			lowerCenterPoint = new Vector3(
				centerPoint.x,
				lowerPoint.y,
				centerPoint.z
			);
			
			// calculate center point to lowest center point offset
			offset = lowerCenterPoint.y - centerPoint.y;
			
			// Calculate tile space vertices
			var tileMatrix = selectedTransform.worldToLocalMatrix * Matrix4x4.Translate(new Vector3(0, -offset, 0));
			
			// TODO: Remove unconnected vertices (from cutted triangles)
			resultVertices.AddRange(worldVertices.Select(w => tileMatrix.MultiplyPoint(w)));


			var instance = CreateInstance<TileAsset>();
			instance.Data = new TilePrototypeData();
			instance.Data.Vertices = resultVertices.ToArray();
			instance.Data.Triangles = resultTriangles.ToArray();
			instance.Data.Colors = resultColors.ToArray();
			instance.Data.Normals = resultNormals.ToArray();
			instance.Data.Tangents = resultTangents.ToArray();
			instance.Data.Uvs = resultUvs.ToArray();
			
			string path = EditorUtility.SaveFolderPanel("Save tile asset to folder", EditorPrefs.GetString("TTR_TileAssetsFolder", ""), "");
			if (path.Length != 0)
			{
				EditorPrefs.SetString("TTR_TileAssetsFolder", path);
				if (path.StartsWith(UnityEngine.Application.dataPath))
				{
					path = "Assets" + path.Substring(UnityEngine.Application.dataPath.Length);
				}
				else
				{
					throw new ArgumentException("Path should be inside unity project");
				}
				
				AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath(path + "/Tile.asset"));
				AssetDatabase.Refresh();
				Selection.activeObject = instance;
			}

		}

		void OnSceneGUI(SceneView view) {
			Handles.BeginGUI();
			var selectedRoot = Selection.activeGameObject;

			if (!selectedRoot)
			{
				GUILayout.Label("Select tile prefab on scene");
				Handles.EndGUI();
				return;
			}


			if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(selectedRoot)))
			{
				GUILayout.Label("Allowed only scene instanciated prefabs");
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Instanciate asset from: {AssetDatabase.GetAssetPath(selectedRoot)}?");
				if (GUILayout.Button("Instanciate"))
				{
					var instance = Instantiate(selectedRoot);
					Selection.activeGameObject = instance;
				}
				GUILayout.EndHorizontal();
				Handles.EndGUI();
				return;
			}

			var selectedTransform = selectedRoot.transform;

			if (selectedTransform.parent)
			{
				GUILayout.Label("Allowed only in root objects!");
				Handles.EndGUI();
				return;
			}
		
			GUILayout.Label("Ready to prepare");
			Handles.EndGUI();

			var meshFilters = selectedRoot.GetComponentsInChildren<MeshFilter>();

			var centerPoint = selectedRoot.transform.position;
			var minPoint = centerPoint - Vector3.right * 0.5f - Vector3.forward * 0.5f;
			var maxPoint = centerPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f;

			List<Vector3> worldVertices = new List<Vector3>();
			List<Vector3> tileVertices = new List<Vector3>();
//			List<int> triangles = new List<int>();
			
			Vector3 lowerPoint = Vector3.positiveInfinity;
			Vector3 lowerCenterPoint = default(Vector3);
			float offset;

			
			// Found all world vertices 
			foreach (var meshFilter in meshFilters)
			{
				var mesh = meshFilter.mesh;
				var transform = meshFilter.transform;
				var vertices = mesh.vertices;
				var triangles = mesh.triangles;
				var worldMatrix = transform.localToWorldMatrix;
//				var rootLocalMatrix = selectedTransform.worldToLocalMatrix;
				var meshOffset = worldVertices.Count;
				worldVertices.AddRange(vertices.Select(v => worldMatrix.MultiplyPoint(v)));
				
				for (var triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex+=3)
				{
					var vert0 = worldVertices[meshOffset + triangles[triangleIndex + 0]];
					var vert1 = worldVertices[meshOffset + triangles[triangleIndex + 1]];
					var vert2 = worldVertices[meshOffset + triangles[triangleIndex + 2]];

					bool outMinX = OutBounds(vert0.x, vert1.x, vert2.x, minPoint.x);
					bool outMaxX = OutBounds(vert0.x, vert1.x, vert2.x, maxPoint.x);
					bool outMinZ = OutBounds(vert0.z, vert1.z, vert2.z, minPoint.z);
					bool outMaxZ = OutBounds(vert0.z, vert1.z, vert2.z, maxPoint.z);

					if (outMinX || outMaxX || outMinZ || outMaxZ)
					{
						// Could be trimmed
						Handles.color = new Color(1f, 0f, 0f, 0.5f);
						Handles.DrawAAConvexPolygon(
							vert0,
							vert1,
							vert2
						);
					}
				}
			}
			
			// Found lower center point
			foreach (var vertex in worldVertices)
			{
				if (vertex.y < lowerPoint.y)
				{
					lowerPoint = vertex;
				}
			}
			lowerCenterPoint = new Vector3(
				centerPoint.x,
				lowerPoint.y,
				centerPoint.z
			);
			
			// calculate center point to lowest center point offset
			offset = lowerCenterPoint.y - centerPoint.y;
			
			// Calculate tile space vertices
			var tileMatrix = selectedTransform.worldToLocalMatrix * Matrix4x4.Translate(new Vector3(0, -offset, 0));
			tileVertices.AddRange(worldVertices.Select(w => tileMatrix.MultiplyPoint(w)));


			Handles.color = new Color(0f, 1f, 0f, 0.5f);
			Handles.DrawAAPolyLine(
				whiteRect, 2f,
				lowerCenterPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f,
				lowerCenterPoint + Vector3.right * 0.5f - Vector3.forward * 0.5f,
				lowerCenterPoint - Vector3.right * 0.5f - Vector3.forward * 0.5f,
				lowerCenterPoint - Vector3.right * 0.5f + Vector3.forward * 0.5f,
				lowerCenterPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f
			);
			
			Handles.color = Color.white;
			foreach (var tileVertex in tileVertices)
			{
				Handles.DotHandleCap(0, tileVertex, Quaternion.identity, 0.01f, EventType.Repaint);
			}

//			foreach (var meshFilter in meshFilters)
//			{
//				var mesh = meshFilter.mesh;
//				var transform = meshFilter.transform;
//
//				var vertices = mesh.vertices;
//				var triangles = mesh.triangles;
//				var localMatrix = transform.localToWorldMatrix;
////				var tileSpaceMatrix = transform.localToWorldMatrix * selectedRoot.transform.worldToLocalMatrix;
//				var rootLocalMatrix = selectedRoot.transform.worldToLocalMatrix;
//				var worldVertices = vertices.Select(v => localMatrix.MultiplyPoint(v)).ToArray();
//
//
//				var offset = lowerCenterPoint.y - centerPoint.y;
//				
//				rootLocalMatrix *= Matrix4x4.Translate(new Vector3(0, -offset, 0)); 
//				
//				var tileVertices = vertices.Select(v =>
//				{
//					// 1) local to world
//					var mv = localMatrix.MultiplyPoint(v);
//					// 2) world to root space
//					mv = rootLocalMatrix.MultiplyPoint(mv);
//					
//					return mv;
//				}).ToArray();
//				
//				
//				foreach (var tileVertex in tileVertices)
//				{
//					Handles.DotHandleCap(0, tileVertex, Quaternion.identity, 0.01f, EventType.Repaint);
//				}
//			
//				// Draw tile plane
//				Handles.color = new Color(0f, 1f, 0f, 0.5f);
//				Handles.DrawAAPolyLine(
//					whiteRect, 2f,
//					lowerCenterPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f,
//					lowerCenterPoint + Vector3.right * 0.5f - Vector3.forward * 0.5f,
//					lowerCenterPoint - Vector3.right * 0.5f - Vector3.forward * 0.5f,
//					lowerCenterPoint - Vector3.right * 0.5f + Vector3.forward * 0.5f,
//					lowerCenterPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f
//				);
//				Handles.color = Color.white;
//
//				for (var triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex+=3)
//				{
//					var vert0 = worldVertices[triangles[triangleIndex + 0]];
//					var vert1 = worldVertices[triangles[triangleIndex + 1]];
//					var vert2 = worldVertices[triangles[triangleIndex + 2]];
//
//
//					bool outMinX = OutBounds(vert0.x, vert1.x, vert2.x, minPoint.x);
//					bool outMaxX = OutBounds(vert0.x, vert1.x, vert2.x, maxPoint.x);
//					bool outMinZ = OutBounds(vert0.z, vert1.z, vert2.z, minPoint.z);
//					bool outMaxZ = OutBounds(vert0.z, vert1.z, vert2.z, maxPoint.z);
//
//					if (outMinX || outMaxX || outMinZ || outMaxZ)
//					{
//						// Could be trimmed
//						Handles.color = new Color(1f, 0f, 0f, 0.5f);
//						Handles.DrawAAConvexPolygon(
//							vert0,
//							vert1,
//							vert2
//						);
//					}
//				}
//				Handles.color = Color.white;
//			}
		}
	
		private bool OutBounds(float point0, float point1, float point2, float boundPoint)
		{
			return Mathf.Abs(boundPoint - point0) < FindTolerance &&
			       Mathf.Abs(boundPoint - point1) < FindTolerance &&
			       Mathf.Abs(boundPoint - point2) < FindTolerance;
		}
	}
}
