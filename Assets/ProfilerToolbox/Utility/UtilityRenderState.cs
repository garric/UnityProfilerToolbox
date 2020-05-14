namespace ProfilerToolbox 
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using System.Collections.Generic;

    public enum ZWrite 
    {
        Off,
        On,
    }

    /// <summary>
    /// Only run in editor mode now, because we have to parse shader render state from shader text(this only can be read in editor mode)
    /// TODO: persistence of shader render state to support runtime query 
    /// </summary>
    internal static class UtilityRenderState 
    {
#if UNITY_EDITOR
        public delegate float ParseStateValue(string token);

        internal enum RenderStatePropertyType {
            Cull,
            ZWrite,
            ZTest,

            Begin = Cull,
            End = ZTest + 1,
        }

        private static RenderStateCache cache = new RenderStateCache();
#endif

        /// <summary>
        /// see UtilityRenderState summary
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static float GetCullMode(Material material)
        {
#if UNITY_EDITOR
            return cache.GetValue(material, RenderStatePropertyType.Cull);
#endif
        }

        /// <summary>
        /// see UtilityRenderState summary
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static float GetZWrite(Material material)
        {
#if UNITY_EDITOR
            return cache.GetValue(material, RenderStatePropertyType.ZWrite);
#endif
        }

        /// <summary>
        /// see UtilityRenderState summary
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static float GetZTest(Material material)
        {
#if UNITY_EDITOR
            return cache.GetValue(material, RenderStatePropertyType.ZTest);
#endif
        }

#if UNITY_EDITOR
        internal class RenderStateProperty 
        {
            public string name;

            public float defaultValue;
            public string propertyName;

            public ParseStateValue parse;

            public const char SPLIT = '[';

            public bool ParseTokens(string[] tokens)
            {
                if (name != tokens[0])
                    return false;

                string param = tokens[1];
                if (param[0] == SPLIT)
                    propertyName = param.Substring(1, tokens[1].Length - 2);
                else
                    defaultValue = parse(tokens[1]);

                return true;
            }

            public static float ParseValueOfCull(string token)
            {
                CullMode mode;
                if (System.Enum.TryParse<CullMode>(token, out mode))
                    return (float)mode;

                Debug.LogError($"RenderStateProperty Cull unknown value {token}");
                return (float)CullMode.Back;
            }

            public static float ParseValueOfZWrite(string token)
            {
                ZWrite mode;
                if (System.Enum.TryParse<ZWrite>(token, out mode))
                    return (float)mode;

                Debug.LogError($"RenderStateProperty ZWrite unknown value {token}");
                return (float)ZWrite.On;
            }

            public static float ParseValueOfZTest(string token)
            {
                CompareFunction mode;
                if (System.Enum.TryParse<CompareFunction>(token, out mode))
                    return (float)mode;

                if (token == "LEqual")
                    return (float)CompareFunction.LessEqual;

                if (token == "GEqual")
                    return (float)CompareFunction.GreaterEqual;

                Debug.LogError($"RenderStateProperty ZTest unknown value {token}");
                return (float)CompareFunction.LessEqual;
            }
        }

        internal class RenderState 
        {
            public RenderStateProperty[] properties = new RenderStateProperty[RenderStatePropertyType.End - RenderStatePropertyType.Begin];

            public RenderState()
            {
                RenderStateProperty cull = new RenderStateProperty() { name = "Cull", defaultValue = (float)CullMode.Back, parse = RenderStateProperty.ParseValueOfCull };
                properties[RenderStatePropertyType.Cull - RenderStatePropertyType.Begin] = cull;

                RenderStateProperty zwrite = new RenderStateProperty() { name = "ZWrite", defaultValue = (float)ZWrite.On, parse = RenderStateProperty.ParseValueOfZWrite };
                properties[RenderStatePropertyType.ZWrite - RenderStatePropertyType.Begin] = zwrite;

                RenderStateProperty ztest = new RenderStateProperty() { name = "ZTest", defaultValue = (float)CompareFunction.LessEqual, parse = RenderStateProperty.ParseValueOfZTest };
                properties[RenderStatePropertyType.ZTest - RenderStatePropertyType.Begin] = ztest;
            }

            public void ParseTokens(string[] tokens)
            {
                foreach (RenderStateProperty property in properties)
                {
                    if (property.ParseTokens(tokens))
                        return;
                }
            }
        }

        internal class RenderStateCache {
            private Dictionary<Shader, RenderState> states = new Dictionary<Shader, RenderState>();

            private void CacheShaderRenderState(Shader shader)
            {
                states[shader] = new RenderState();

                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(shader);
                if (string.IsNullOrEmpty(assetPath))
                {
                    // can't parse unity built-in shaders now
                    Debug.LogError($"RenderState Can't Parse Unity Built-in Shader {shader.name}");
                    return;
                }

                int lastIndex = Application.dataPath.LastIndexOf("Assets");
                string filePath = Application.dataPath.Substring(0, lastIndex) + assetPath;
                Debug.Log($"Shader {filePath}");

                string[] lines = System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine == "NAME \"OUTLINE\"")
                        return;

                    string[] tokens = trimmedLine.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (tokens == null || tokens.Length == 0 || tokens.Length > 2)
                        continue;

                    int index = tokens[0].IndexOf(RenderStateProperty.SPLIT);
                    if (tokens.Length == 1 && index >= 0)
                    {
                        string raw = tokens[0];
                        tokens = new string[2] { raw.Substring(0, index), raw.Substring(index) };
                    }


                    for (int i = 0, imax = tokens.Length; i < imax; i++)
                        tokens[i] = tokens[i].Trim();

                    // Just override the last hit
                    // Though It may not be exactly now(N SubShaders, N Passes, Include Files), but is't enough for most cases.
                    states[shader].ParseTokens(tokens);
                }
            }

            public float GetValue(Material material, RenderStatePropertyType type)
            {
                if (!states.ContainsKey(material.shader))
                    CacheShaderRenderState(material.shader);

                RenderState state = states[material.shader];
                RenderStateProperty property = state.properties[type - RenderStatePropertyType.Begin];
                if (string.IsNullOrEmpty(property.propertyName))
                    return property.defaultValue;

                return material.GetFloat(property.propertyName);
            }
        }
#endif
    }
}