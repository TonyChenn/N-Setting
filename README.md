# N-Setting
Unity编辑器工具，将Unity项目中所有模块的自定义的设置整合到同一个设置窗口。再也不用到处找修改哪里了。

# 效果图
![previous](/previous.jpg)

# 用法
1. 建议一个模块创建一个设置工具类。（当然也可以创建多个，不影响）该类继承两个接口：<kbd>IPathConfig</kbd>,<kbd>IEditorPrefs</kbd>
分别实现1个方法：
```csharp
// IPathConfig
// 设置在设置窗口中所属窗口
public string GetModuleName()
{
    return "导表配置";
}
```
```csharp
// IEditorPrefs
// 将设置选项的唯一EditorPrefs Key放入下面，方便统一管理
public void ReleaseEditorPrefs()
{
    EditorPrefs.DeleteKey("Path_TableConfig_ExcelFolder");
    EditorPrefs.DeleteKey("Path_TableConfig_GenAssetFolder");
    EditorPrefs.DeleteKey("Path_TableConfig_GenCSharpFolder");
}
```
2. 根据需要添加属性，该属性回显示到设置窗口上,如下：
```csharp
// SettingAttribute 中参数
// - 类型
// - 设置中item的标题
// get;set; 用来保存设置到EditorPrefs
[SettingAttribute(FieldType.Folder, "Excel 目录: ")]
public static string ExcelFolder
{
    get { return EditorPrefsHelper.GetString("Path_TableConfig_ExcelFolder", Application.dataPath); }
    set => EditorPrefsHelper.SetString("Path_TableConfig_ExcelFolder", value);
}
```
