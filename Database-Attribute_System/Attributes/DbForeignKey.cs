using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbForeignKey : Attribute
    {
        public Type _classType;
        public string _attributeName;

        /// <summary>
        /// Marks variable as foreignKey of given class
        /// </summary>
        /// <param name="classType">Type of class to which this is the ForeignKey</param>
        /// <param name="dbAttributeName">Name of database-attribute (case-sensitivity is determined from database-attribute-settings) ['null' if the same as field-name]</param>
        public DbForeignKey(Type classType, string attributeName = null)
        {
            this._classType = classType;
            this._attributeName = attributeName;
        }
    }
}
