using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbAttribute : BaseAttribute
    {
        /// <summary>
        /// Marks variable as database-attribute
        /// </summary>
        /// <param name="dbAttributeName">Name of database-attribute (case-sensitivity is determined from database-attribute-settings) ['null' if the same as field-name]</param>
        public DbAttribute(string attributeName = null)
        {
            this._attributeName = attributeName;
        }

        public void Init(FieldInfo fi, DbObject classAttribute)
        {
            this.parentField = fi;
            this.classAttribute = classAttribute;

            this._attributeName = this._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
        }
    }
}
