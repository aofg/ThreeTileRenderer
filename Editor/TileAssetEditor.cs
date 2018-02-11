using UnityEditor;
using UnityEngine;

namespace ThreeTileRenderer.Editor
{
    [CustomEditor(typeof(TileAsset))]
    public class TileAssetEditor : UnityEditor.Editor
    {
        private Vector2 cameraAngle;

        public override bool HasPreviewGUI()
        {
            return true;
        }
        
        /// <summary>
        /// Sets back the camera angle
        /// </summary>
        public void ResetPreviewCameraAngle()
        {
            cameraAngle = new Vector2(127.5f, -22.5f);
        }
        
        /// <summary>
        /// Draws the content of the Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //// Had to disable the default Inspector as it makes preview lag
            //DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Draws the toolbar area on top of the preview window
        /// </summary>
        public override void OnPreviewSettings()
        {
            if (GUILayout.Button("Reset Camera", EditorStyles.miniButton))
            {
                ResetPreviewCameraAngle();
            }
        }

        /// <summary>
        /// Draws the preview area
        /// </summary>
        /// <param name="rect">The area of the preview window</param>
        /// <param name="backgroundStyle">The default GUIStyle used for preview windows</param>
        public override void OnPreviewGUI(Rect rect, GUIStyle backgroundStyle)
        {
            cameraAngle = PreviewRenderUtilityHelpers.DragToAngles(cameraAngle, rect);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(
                    rect,
                    ((TileAsset) serializedObject.targetObject).RenderPreview(rect, cameraAngle, 6.5f), 
                    ScaleMode.StretchToFill, 
                    true);
            }
        }
    }
}