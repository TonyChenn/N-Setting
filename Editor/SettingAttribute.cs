using System;
using SFramework.Editor;

namespace SettingKit.Editor
{
    /// <summary>
    /// 只能用于字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute:Attribute
    {
        private string title;

        public SettingAttribute(string title)
        {
            this.title = title;
        }
        
        public string Title
        {
            get { return title; }
        }
    }
}