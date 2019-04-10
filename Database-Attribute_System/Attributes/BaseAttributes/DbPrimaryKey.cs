using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbPrimaryKey : BaseAttribute
    {
        /// <summary>
        /// Marks variable as primaryKey fo current class
        /// </summary>
        /// <param name="dbAttributeName">Name of database-attribute (case-sensitivity is determined from database-attribute-settings) ['null' if the same as field-name]</param>
        public DbPrimaryKey(string attributeName = null)
        {
            this._attributeName = attributeName;    // Todo: Automatic resolving of name if it is null (?)
        }

        public void Init(FieldInfo fi, DbObject classAttribute)
        {
            this.parentField = fi;
            this.classAttribute = classAttribute;

            this._attributeName = this._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
        }
    }
}
