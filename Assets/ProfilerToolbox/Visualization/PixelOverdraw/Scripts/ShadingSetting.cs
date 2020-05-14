namespace ProfilerToolbox.Visualization 
{
    namespace PixelOverdraw 
    {
        using UnityEngine;

        [System.Serializable]
        public class ShadingSetting 
        {
            public const int GRIDSIZE_MIN = 16;
            public const float ALPHA_ONE = 1.0f / 255.0f;

            public bool opaqueBlack = false;
            public float overdrawColorSaturation = 10;
            public bool showAlpha = false;
            public bool whiteNumber = false;

            public bool resetStatistics = false;
            public bool refreshStatistics = false;

            public Vector2 lastGridSize = Vector2.zero;
            public bool realtimeStatistics = false;
            public int realtimeStatisticsFrequency = 15;
            public bool checkPlatformRenderingConvension = false;

            public Texture2D textureScreen; // for debug
        }
    }
}