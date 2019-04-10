using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    public class BaseAttribute : Attribute
    {
        public FieldInfo parentField;
        public DbObject classAttribute;

        public string _attributeName;

        public BaseAttribute()
        {

        }
    }
}
