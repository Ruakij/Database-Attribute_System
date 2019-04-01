using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    class DbFunction
    {
        internal static object[] BuildKeyEqualQuery(Dictionary<string, object> keysets, string seperator)
        {
            object[] param = new object[keysets.Count * 2];
            int c = 0;
            foreach (KeyValuePair<string, object> keyset in keysets)
            {
                string sql_string = "";

                if (c != 0) sql_string += seperator;
                sql_string += $"{keyset.Key}=";

                param[c] = sql_string;
                param[c + 1] = keyset.Value;

                c += 2;
            }

            return param;
        }

        public static string GetDbTableName(Type classType)
        {
            // Check if class has attribute 'DbObject' and get the database table-name
            if (!(classType.GetCustomAttribute(typeof(DbObject), true) is DbObject dbObjectAttribute)) throw new InvalidOperationException($"Cannot generate SQL-Query of '{classType.Name}'. Missing Attribute 'DbObject'");
            string tableName = dbObjectAttribute._tableName ?? classType.Name;    // If no alternative table-name is specified, use the class-name

            return tableName;
        }

        internal static void ReadDbClassFields<T>(T classObject, ref Dictionary<string, object> dbPrimaryKeys, ref Dictionary<string, object> dbAttributes, ref Dictionary<string, object> dbForeignKeys)
        {
            Type classType = typeof(T);

            // Reset lists (just in case)
            dbPrimaryKeys = new Dictionary<string, object>() { };
            dbAttributes = new Dictionary<string, object>() { };
            dbForeignKeys = new Dictionary<string, object>() { };

            // Iterate thru all properties
            foreach (System.Reflection.FieldInfo fi in classType.GetRuntimeFields())
            {
                // Check if current field is a db-field
                if (fi.GetCustomAttribute(typeof(DbPrimaryKey), true) is DbPrimaryKey pkey) // PrimaryKey
                {
                    string dbAttributeName = pkey._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    object value = fi.GetValue(classObject);
                    dbPrimaryKeys.Add(dbAttributeName, value);
                }
                else if (fi.GetCustomAttribute(typeof(DbAttribute), true) is DbAttribute att)   // Attributes
                {
                    string dbAttributeName = att._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    object value = fi.GetValue(classObject);
                    dbAttributes.Add(dbAttributeName, value);
                }
                else if (fi.GetCustomAttribute(typeof(DbForeignKey), true) is DbForeignKey fkey)    // ForeignKeys
                {
                    string dbAttributeName = fkey._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    object value = fi.GetValue(classObject);
                    dbForeignKeys.Add(dbAttributeName, value);
                }
            }
        }
    }
}
