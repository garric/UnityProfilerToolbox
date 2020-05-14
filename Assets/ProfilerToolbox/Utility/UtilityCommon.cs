namespace ProfilerToolbox 
{
    using UnityEngine;
    using UnityEngine.Assertions;

    using System.IO;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Text;    

    public static class UtilityCommon 
    {
        public static List<T> FindObjectsOfAll<T>() where T : Object
        {
            List<T> objects = new List<T>();
            foreach (T obj in Object.FindObjectsOfType<T>())
                objects.Add(obj);

            {
                //System.Type type = typeof(T);
                //for (int i = 0; i < SceneManager.sceneCount; i++)
                //{
                //    Scene scene = SceneManager.GetSceneAt(i);
                //    if (!scene.isLoaded)
                //        continue;

                //    List<GameObject> childs = new List<GameObject>();
                //    scene.GetRootGameObjects(childs);
                //    while (childs.Count > 0)
                //    {
                //        GameObject child = childs[0];                    
                //        for (int k = 0; k < child.transform.childCount; k++)
                //            childs.Add(child.transform.GetChild(k).gameObject);

                //        childs.RemoveAt(0);

                //        if (type != typeof(GameObject))
                //        {
                //            foreach (T comp in child.GetComponents<T>())
                //                objects.Add(comp);
                //        }
                //        else
                //            objects.Add(child as T);                    
                //    }
                //}
            }

            return objects;
        }

        public static void Destroy(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }

        public static T TryAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
                component = target.AddComponent<T>();
            return component;
        }

        #region string
        public static string Cancat(this string[] stringArray)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (string str in stringArray)
                sb.Append($"{str}, ");
            return sb.ToString();
        }

        public static string FormatSlash(this string path)
        {
            return path.Replace("\\", "/");
        }

        public static string[] ReadAllLines(this string filepath)
        {
            string[] alllines = new string[0];
            if (File.Exists(filepath))
            {
                File.SetAttributes(filepath, File.GetAttributes(filepath) & ~FileAttributes.ReadOnly);
                alllines = File.ReadAllLines(filepath, System.Text.Encoding.ASCII);
            }
            return alllines;
        }
        #endregion

        #region Reflection

        static IEnumerable<System.Type> m_AssemblyTypes;

        /// <summary>
        /// Gets all currently available assembly types.
        /// </summary>
        /// <returns>A list of all currently available assembly types</returns>
        /// <remarks>
        /// This method is slow and should be use with extreme caution.
        /// </remarks>
        public static IEnumerable<System.Type> GetAllAssemblyTypes()
        {
            if (m_AssemblyTypes == null)
            {
                m_AssemblyTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t =>
                    {
                        // Ugly hack to handle mis-versioned dlls
                        var innerTypes = new System.Type[0];
                        try
                        {
                            innerTypes = t.GetTypes();
                        }
                        catch { }
                        return innerTypes;
                    });
            }

            return m_AssemblyTypes;
        }

        /// <summary>
        /// Helper method to get the first attribute of type <c>T</c> on a given type.
        /// </summary>
        /// <typeparam name="T">The attribute type to look for</typeparam>
        /// <param name="type">The type to explore</param>
        /// <returns>The attribute found</returns>
        public static T GetAttribute<T>(this System.Type type) where T : System.Attribute
        {
            Assert.IsTrue(type.IsDefined(typeof(T), false), "Attribute not found");
            return (T)type.GetCustomAttributes(typeof(T), false)[0];
        }

        /// <summary>
        /// Returns all attributes set on a specific member.
        /// </summary>
        /// <typeparam name="TType">The class type where the member is defined</typeparam>
        /// <typeparam name="TValue">The member type</typeparam>
        /// <param name="expr">An expression path to the member</param>
        /// <returns>An array of attributes</returns>
        /// <remarks>
        /// This method doesn't return inherited attributes, only explicit ones.
        /// </remarks>
        public static System.Attribute[] GetMemberAttributes<TType, TValue>(Expression<System.Func<TType, TValue>> expr)
        {
            Expression body = expr;

            if (body is LambdaExpression)
                body = ((LambdaExpression)body).Body;

            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var fi = (FieldInfo)((MemberExpression)body).Member;
                    return fi.GetCustomAttributes(false).Cast<System.Attribute>().ToArray();
                default:
                    throw new System.InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns a string path from an expression. This is mostly used to retrieve serialized
        /// properties without hardcoding the field path as a string and thus allowing proper
        /// refactoring features.
        /// </summary>
        /// <typeparam name="TType">The class type where the member is defined</typeparam>
        /// <typeparam name="TValue">The member type</typeparam>
        /// <param name="expr">An expression path fo the member</param>
        /// <returns>A string representation of the expression path</returns>
        public static string GetFieldPath<TType, TValue>(Expression<System.Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            var members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = new StringBuilder();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }

            return sb.ToString();
        }

        #endregion
    }
}