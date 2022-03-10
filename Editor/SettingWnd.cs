using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


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
                // 绘制字段
                foreach (PropertyInfo field in type_item.GetProperties())
                {
                    DrawProperty(field);
                }
                // 绘制方法
                foreach (MethodInfo method in type_item.GetMethods())
                {
                    DrawMethod(method);
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
    private void DrawPage()
    {
    }

    /// <summary>
    /// 绘制字段
    /// </summary>
    private void DrawProperty(PropertyInfo property)
    {
        SettingPropertyAttribute attr = property.GetCustomAttribute<SettingPropertyAttribute>(false);
        if (attr != null)
        {
            GUILayout.BeginHorizontal();
            //字段标题
            string title = attr.Title;

            switch (attr.FieldType)
            {
                case FieldType.Button:
                    DrawButton(title, attr.Text, attr.Callback);
                    break;
                case FieldType.TextField:
                    DrawTextField(property, title);
                    break;
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


    }

    /// <summary>
    /// 绘制方法
    /// </summary>
    /// <param name="methodInfo"></param>
    private void DrawMethod(MethodInfo methodInfo)
    {
        SettingMethodAttribute attr = methodInfo.GetCustomAttribute<SettingMethodAttribute>(false);
        if (attr != null)
        {
            GUILayout.BeginHorizontal();
            DrawButton(methodInfo, attr.Title, attr.Text);
            GUILayout.EndHorizontal();
        }
    }

    private void Refresh()
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
                    string moduleName = method.Invoke(o, new object[] { }).ToString();

                    if (!dict.ContainsKey(moduleName))
                        dict[moduleName] = new List<Type>();
                    dict[moduleName.ToString()].Add(classTypes[i]);
                }
            }
        }
    }

    #region DrawField

    private void DrawButton(MethodInfo methodInfo, string title, string text)
    {
        DrawButton(title, text, () =>
        {
            methodInfo.Invoke(this, null);
        });
    }

    private void DrawButton(string title, string text, Action action)
    {
        GUILayout.Label(title, GUILayout.Width(150));
        GUILayout.Space(30);
        if (GUILayout.Button(text))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// 文本框
    /// </summary>
    /// <param name="property"></param>
    /// <param name="fieldName"></param>
    private void DrawTextField(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));
        GUILayout.Space(30);
        string strValue = property.GetValue(null).ToString();
        GUILayout.TextField(strValue);
    }

    /// <summary>
    /// 绘制选择文件夹
    /// </summary>
    private void DrawSelectFolder(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));

        string strValue = property.GetValue(null).ToString();
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            strValue = EditorUtility.SaveFolderPanel("选择" + fieldName, strValue,
                strValue);
            if (!string.IsNullOrEmpty(strValue))
                property.SetValue(null, strValue);
        }

        GUILayout.TextField(strValue);
    }

    /// <summary>
    /// 绘制选择文件
    /// </summary>
    private void DrawSelectFile(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));
        string strValue = property.GetValue(null).ToString();
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            strValue = EditorUtility.OpenFilePanel("选择" + fieldName, strValue, "*");
            if (!string.IsNullOrEmpty(strValue))
                property.SetValue(null, strValue);
        }

        GUILayout.TextField(strValue);
    }

    /// <summary>
    /// 绘制Toggle
    /// </summary>
    private void DrawToggle(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));

        bool boolValue = (bool)property.GetValue(null);
        boolValue = GUILayout.Toggle(boolValue, "");
        property.SetValue(null, boolValue);
    }

    #endregion
}