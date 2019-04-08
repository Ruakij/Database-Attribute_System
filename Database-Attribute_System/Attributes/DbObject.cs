using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DbObject : Attribute
    {
        public string _tableName;

        /// <summary>
        /// Marks variable as database-table
        /// </summary>
        /// <param name="tableName">Name of database-table (case-sensitivity is determined from database-table-settings) ['null' if the same as class-name]</param>
        public DbObject(string tableName = null)
        {
            this._tableName = tableName;    // Todo: Automatic resolving of name if it is null (?)
        }
    }
}
