using UnityEditor;
using UnityEngine;

public class TilePrepareEditor : EditorWindow {
	public float FindTolerance;

	[MenuItem("Tile Renderer/Prepare Asset Window")]
	public static void OpenTilePrepare()
	{
		var instance = GetWindow<TilePrepareEditor>();
		instance.Show();
	}

	private void OnEnable()
	{
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
	}

	void OnSceneGUI(SceneView view) {
		Handles.BeginGUI();
		var selectedRoot = Selection.activeGameObject;

		if (!selectedRoot)
		{
			GUILayout.Label("Select tile prefab on scene");
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
			return;
		}

		if (selectedRoot.transform.parent)
		{
			GUILayout.Label("Allowed only in root objects!");
		}
		
		GUILayout.Label("Ready to prepare");
		Handles.EndGUI();

		var meshFilters = selectedRoot.GetComponentsInChildren<MeshFilter>();

		var centerPoint = selectedRoot.transform.position;
		var minPoint = centerPoint - Vector3.right * 0.5f - Vector3.forward * 0.5f;
		var maxPoint = centerPoint + Vector3.right * 0.5f + Vector3.forward * 0.5f;
		
		int index = 0;
		
		foreach (var meshFilter in meshFilters)
		{
			var mesh = meshFilter.mesh;
			var transform = meshFilter.transform;

			var vertices = mesh.vertices;
			var triangles = mesh.triangles;
			var localMatrix = transform.localToWorldMatrix;
			
//			foreach (var vertex in vertices)
//			{
//				var mvertex = localMatrix.MultiplyPoint(vertex);
//				bool inbounds = !(mvertex.x - minPoint.x < Mathf.Epsilon || 
//				                  maxPoint.x - mvertex.x < Mathf.Epsilon ||
//				                  mvertex.z - minPoint.z < Mathf.Epsilon || 
//				                  maxPoint.z - mvertex.z < Mathf.Epsilon);
//
//				Handles.color = inbounds ? Color.green : Color.red;
//				Handles.DotHandleCap(index++, localMatrix.MultiplyPoint(vertex), Quaternion.identity, 0.01f, EventType.Repaint);
//				Handles.color = Color.white;
//			}

			for (var triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex+=3)
			{
				var vert0 = vertices[triangles[triangleIndex + 0]];
				var vert1 = vertices[triangles[triangleIndex + 1]];
				var vert2 = vertices[triangles[triangleIndex + 2]];
				
				var mvertex0 = localMatrix.MultiplyPoint(vert0);
				var mvertex1 = localMatrix.MultiplyPoint(vert1);
				var mvertex2 = localMatrix.MultiplyPoint(vert2);


//				bool inbounds0 = InBounds(mvertex0, minPoint, maxPoint);
//				bool inbounds1 = InBounds(mvertex1, minPoint, maxPoint);
//				bool inbounds2 = InBounds(mvertex2, minPoint, maxPoint);

				bool outMinX = OutBounds(mvertex0.x, mvertex1.x, mvertex2.x, minPoint.x);
				bool outMaxX = OutBounds(mvertex0.x, mvertex1.x, mvertex2.x, maxPoint.x);
				bool outMinZ = OutBounds(mvertex0.z, mvertex1.z, mvertex2.z, minPoint.z);
				bool outMaxZ = OutBounds(mvertex0.z, mvertex1.z, mvertex2.z, maxPoint.z);

				if (outMinX || outMaxX || outMinZ || outMaxZ)
				{
					// Could be trimmed
					Handles.color = new Color(1f, 0f, 0f, 0.5f);
					Handles.DrawAAConvexPolygon(
						mvertex0,
						mvertex1,
						mvertex2
					);
				}
//				else
//				{
//					Handles.color = new Color(0f, 1f, 0f, 0.15f);
//				}
				
			}
			Handles.color = Color.white;
		}
		
		
		Handles.DrawWireCube(selectedRoot.transform.position, Vector3.one);

//		if (Event.current.type == EventType.Repaint)
//		{
//			Gizmos.DrawWireCube(selectedRoot.transform.position, Vector3.one);
//		}
		
//		Repaint();
	}
	
	private bool OutBounds(float point0, float point1, float point2, float boundPoint)
	{
		return Mathf.Abs(boundPoint - point0) < FindTolerance &&
		       Mathf.Abs(boundPoint - point1) < FindTolerance &&
			   Mathf.Abs(boundPoint - point2) < FindTolerance;
	}
}
