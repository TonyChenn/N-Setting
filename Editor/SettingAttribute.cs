using System;

namespace SettingKit.Editor
{
    /// <summary>
    /// 只能用于字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute:Attribute
    {
        private FieldType fieldType;
        private string title;

        public SettingAttribute(FieldType fieldType, string title)
        {
            this.fieldType = fieldType;
            this.title = title;
        }

        public string Title
        {
            get { return title; }
        }
        
        public FieldType FieldType
        {
            get { return fieldType; }
        }
    }
}