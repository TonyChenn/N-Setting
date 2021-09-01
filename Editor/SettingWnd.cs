using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SettingKit.Editor
{
    public class SettingWnd : EditorWindow
    {
        [MenuItem("Tools/设置")]
        private static void ShowWindow()
        {
            ShowWindow("");
        }


        public static SettingWnd ShowWindow(string tag = null)
        {
            var window = GetWindow<SettingWnd>();
            window.titleContent = new GUIContent("模块设置工具箱");
            window.minSize = new Vector2(800, 500);
            window.Show();
            if (window.dict.ContainsKey(tag))
                window.selectIndex = window.dict.Keys.ToList().IndexOf(tag);
            else
                window.selectIndex = 0;
            return window;
        }

        private int selectIndex = 0;
        private Dictionary<string, List<Type>> dict = new Dictionary<string, List<Type>>();
        private Vector2 pos;

        private void Awake()
        {
            Refresh();
        }

        private void OnGUI()
        {
            selectIndex = GUILayout.SelectionGrid(selectIndex, dict.Keys.ToArray(), 5);
            GUILayout.Space(20);
            pos = GUILayout.BeginScrollView(pos);
            if (dict.Count > 0)
            {
                foreach (Type type_item in dict.Values.ToArray()[selectIndex])
                {
                    foreach (PropertyInfo field in type_item.GetProperties())
                    {
                        DrawProperty(field);
                    }
                }
            }

            GUILayout.EndScrollView();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新"))
            {
                Refresh();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制页面的所有属性
        /// </summary>
        void DrawPage()
        {
        }

        /// <summary>
        /// 绘制字段
        /// </summary>
        void DrawProperty(PropertyInfo property)
        {
            GUILayout.BeginHorizontal();
            SettingAttribute attr = property.GetCustomAttribute<SettingAttribute>(false);
            //字段类型
            FieldType fieldType = attr == null ? FieldType.Folder : attr.FieldType;
            //字段标题
            string title = attr == null ? property.Name + ": " : attr.Title;

            switch (fieldType)
            {
                case FieldType.Folder:
                    DrawSelectFolder(property, title);
                    break;
                case FieldType.File:
                    DrawSelectFile(property, title);
                    break;
                case FieldType.Toggle:
                    DrawToggle(property, title);
                    break;
                default:
                    GUILayout.Button(property.PropertyType.ToString());
                    break;
            }

            GUILayout.EndHorizontal();
        }

        void Refresh()
        {
            dict.Clear();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                List<Type> list = new List<Type>(20);
                var classTypes = assembly.GetTypes();
                for (int i = 0; i < classTypes.Length; i++)
                {
                    if (classTypes[i].IsInterface) continue;

                    // 所有有接口的类
                    Type ins = classTypes[i].GetInterface("IPathConfig");
                    MethodInfo method = classTypes[i].GetMethod("GetModuleName", Type.EmptyTypes);

                    if (ins != null && method != null)
                    {
                        object o = Activator.CreateInstance(classTypes[i]);
                        string name = method.Invoke(o, new object[] { }).ToString();

                        if (!dict.ContainsKey(name))
                            dict[name] = new List<Type>();
                        dict[name.ToString()].Add(classTypes[i]);
                    }
                }
            }
        }

        #region DrawField

        /// <summary>
        /// 绘制选择文件夹
        /// </summary>
        public void DrawSelectFolder(PropertyInfo property, string fieldName)
        {
            GUILayout.Label(fieldName, GUILayout.Width(100));

            string strValue = property.GetValue(null).ToString();
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                strValue = EditorUtility.SaveFolderPanel("选择" + fieldName, strValue,
                    strValue);
                property.SetValue(null, strValue);
            }

            GUILayout.TextField(strValue);
        }

        /// <summary>
        /// 绘制选择文件
        /// </summary>
        public void DrawSelectFile(PropertyInfo property, string fieldName)
        {
            GUILayout.Label(fieldName, GUILayout.Width(100));
            string strValue = property.GetValue(null).ToString();
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                strValue = EditorUtility.OpenFilePanel("选择" + fieldName, strValue, "*");
                property.SetValue(null, strValue);
            }

            GUILayout.TextField(strValue);
        }

        /// <summary>
        /// 绘制Toggle
        /// </summary>
        public void DrawToggle(PropertyInfo property, string fieldName)
        {
            bool boolValue = (bool) property.GetValue(null);
            boolValue = GUILayout.Toggle(boolValue, fieldName);
            property.SetValue(null, boolValue);
        }

        #endregion
    }
}