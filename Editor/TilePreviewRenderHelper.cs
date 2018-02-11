
using UnityEngine;

namespace ThreeTileRenderer.Editor
{
    public static class TilePreviewRenderHelper
    {
        #region Members
        /// <summary>
        /// The material used to render the Texture3D
        /// </summary>
        private static Material _previewMaterial;
        /// <summary>
        /// The texture that will be used for the background
        /// </summary>
        private static Texture2D _backgroundTexture;
        /// <summary>
        /// The GUIStyle that will be used for the background
        /// </summary>
        private static GUIStyle _backgroundGuiStyle;
        #endregion

        #region Functions
        /// <summary>
        /// Accessor to the material used to render the Texture3D
        /// </summary>
        private static Material PreviewMaterial
        {
            get
            {
                if (_previewMaterial == null)
                {
                    _previewMaterial = new Material(Shader.Find("Standard"));
                }

                return _previewMaterial;
            }
        }

        /// <summary>
        /// Accessor to the static background texture
        /// </summary>
        public static Texture2D BackgroundTexture
        {
            get
            {
                if (_backgroundTexture == null)
                {
                    _backgroundTexture = new Texture2D(1, 1);
                    _backgroundTexture.SetPixel(0, 0, Color.gray * 0.5f);
                    _backgroundTexture.Apply();
                }

                return _backgroundTexture;
            }
        }

        /// <summary>
        /// Accessor to the static background GUIStyle
        /// </summary>
        public static GUIStyle BackgroundGuiStyle
        {
            get
            {
                _backgroundGuiStyle = new GUIStyle();
                _backgroundGuiStyle.active.background = BackgroundTexture;
                _backgroundGuiStyle.focused.background = BackgroundTexture;
                _backgroundGuiStyle.hover.background = BackgroundTexture;
                _backgroundGuiStyle.normal.background = BackgroundTexture;

                return _backgroundGuiStyle;
            }
        }

        /// <summary>
        /// Sets the parameters to the PreviewRenderUtility and calls the rendering
        /// </summary>
        /// <param name="tile">The Tile to preview</param>
        /// <param name="angle">The camera angle</param>
        /// <param name="distance">The distance of the camera to the preview cube</param>
        /// <param name="samplingIterations">The amount of slices used to raymarch in the Texture3D</param>
        /// <param name="density">A linear factor to multiply the Texture3D with</param>
        private static void RenderInPreviewRenderUtility(TileAsset tile, Vector2 angle, float distance)
        {   
            PreviewRenderUtilityHelpers.Instance.DrawMesh(tile, Matrix4x4.identity, PreviewMaterial, 0);
            PreviewRenderUtilityHelpers.Instance.camera.transform.rotation = Quaternion.Euler(new Vector3(-angle.y, -angle.x, 0));
            PreviewRenderUtilityHelpers.Instance.camera.transform.position = Vector3.up * 1.5f + PreviewRenderUtilityHelpers.Instance.camera.transform.forward * -distance;
            PreviewRenderUtilityHelpers.Instance.camera.Render();
        }

        /// <summary>
        /// Renders a preview of the Texture3D
        /// </summary>
        /// <param name="texture3D">The Texture3D to preview</param>
        /// <param name="rect">The area where the preview is located</param>
        /// <param name="angle">The camera angle</param>
        /// <param name="distance">The distance of the camera to the preview cube</param>
        /// <param name="samplingIterations">The amount of slices used to raymarch in the Texture3D</param>
        /// <param name="density">A linear factor to multiply the Texture3D with</param>
        /// <returns>A Texture with the preview</returns>
        public static Texture RenderPreview(this TileAsset asset, Rect rect, Vector2 angle, float distance)
        {
            PreviewRenderUtilityHelpers.Instance.BeginPreview(rect, BackgroundGuiStyle);
            RenderInPreviewRenderUtility(asset, angle, distance);
            return PreviewRenderUtilityHelpers.Instance.EndPreview();
        }

        /// <summary>
        /// Renders a thumbnail of the Texture3D
        /// </summary>
        /// <param name="tile">The Texture3D to preview</param>
        /// <param name="rect">The area where the preview is located</param>
        /// <param name="angle">The camera angle</param>
        /// <param name="distance">The distance of the camera to the preview cube</param>
        /// <param name="samplingIterations">The amount of slices used to raymarch in the Texture3D</param>
        /// <param name="density">A linear factor to multiply the Texture3D with</param>
        /// <returns>A Texture2D with the thumbnail</returns>
        public static Texture2D RenderTexture3DStaticPreview(this TileAsset tile, Rect rect, Vector2 angle, float distance)
        {
            PreviewRenderUtilityHelpers.Instance.BeginStaticPreview(rect);
        
            RenderInPreviewRenderUtility(tile, angle, distance);

            return PreviewRenderUtilityHelpers.Instance.EndStaticPreview();
        }
        #endregion
    }
}
