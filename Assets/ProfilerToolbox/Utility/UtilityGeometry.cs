namespace ProfilerToolbox 
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// all of mesh are in clockwise wind order
    /// </summary>
    internal static class UtilityGeometry 
    {
        static Mesh s_FullscreenTriangle;

        /// <summary>
        /// A full screen triangle mesh.
        /// </summary>
        public static Mesh fullscreenTriangle 
        {
            get {
                if (s_FullscreenTriangle != null)
                    return s_FullscreenTriangle;

                s_FullscreenTriangle = new Mesh { name = "Full Screen Triangle" };

                // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
                // this directly in the vertex shader using vertex ids :(
                s_FullscreenTriangle.SetVertices(new List<Vector3>
                {
                    new Vector3(-1f, -1f, 0f),
                    new Vector3(-1f,  3f, 0f),
                    new Vector3( 3f, -1f, 0f)
                });
                s_FullscreenTriangle.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
                s_FullscreenTriangle.UploadMeshData(false);

                return s_FullscreenTriangle;
            }
        }

        private static Mesh s_FullScreenQuad;

        /// <summary>
        /// A full screen quad mesh.
        /// </summary>
        public static Mesh fullscreenQuad {
            get {
                if (s_FullScreenQuad != null)
                    return s_FullScreenQuad;

                if (s_FullScreenQuad == null)
                {
                    s_FullScreenQuad = new Mesh { name = "FullScreen Quad Mesh" };
                    //// Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
                    //// this directly in the vertex shader using vertex ids :(
                    s_FullScreenQuad.vertices = new Vector3[4]
                    {
                        new Vector3(-1, -1, 0),
                        new Vector3(-1,  1, 0),
                        new Vector3( 1,  1, 0),
                        new Vector3( 1, -1, 0),
                    };
                    s_FullScreenQuad.uv = new Vector2[4] {
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 1),
                        new Vector2(1, 0),
                    };
                    s_FullScreenQuad.triangles = new int[6] {
                        0, 1, 3, // lower left triangle
                        1, 2, 3, // upper right triangle
                    };
                    s_FullScreenQuad.RecalculateBounds();
                    s_FullScreenQuad.UploadMeshData(false);
                };
                return s_FullScreenQuad;
            }
        }
    }
}
