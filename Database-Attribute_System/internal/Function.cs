using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    internal class Function
    {
        public static string SqlEscape(string str)
        {
            string str_cpy = str.Replace(@"'", @"\'");
            return str_cpy;
        }

        public static string SqlSerialise(DateTime dt, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return dt.ToString(format);
        }

        // Recursive object[] copying
        internal static void RecursiveParameterCopying(ref List<object> paramz, object[] objects)
        {
            foreach (object obj in objects)
            {
                if (!(obj is object[]))
                    paramz.Add(obj);
                else
                    RecursiveParameterCopying(ref paramz, (object[])obj);
            }
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

        internal static Dictionary<string, FieldInfo> ReadDbClassFields<T>(T classObject)
        {
            Type classType = typeof(T);

            Dictionary<string, FieldInfo> dbFields = new Dictionary<string, FieldInfo>();

            // Iterate thru all properties
            foreach (System.Reflection.FieldInfo fi in classType.GetRuntimeFields())
            {
                // Check if current field is a db-field
                if (fi.GetCustomAttribute(typeof(DbPrimaryKey), true) is DbPrimaryKey pkey) // PrimaryKey
                {
                    string dbAttributeName = pkey._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    dbFields.Add(dbAttributeName, fi);
                }
                else if (fi.GetCustomAttribute(typeof(DbAttribute), true) is DbAttribute att)   // Attributes
                {
                    string dbAttributeName = att._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    dbFields.Add(dbAttributeName, fi);
                }
                else if (fi.GetCustomAttribute(typeof(DbForeignKey), true) is DbForeignKey fkey)    // ForeignKeys
                {
                    string dbAttributeName = fkey._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    dbFields.Add(dbAttributeName, fi);
                }
            }

            return dbFields;
        }

        public static string GetDbTableName(Type classType)
        {
            // Check if class has attribute 'DbObject' and get the database table-name
            if (!(classType.GetCustomAttribute(typeof(DbObject), true) is DbObject dbObjectAttribute)) throw new InvalidOperationException($"Cannot generate SQL-Query of '{classType.Name}'. Missing Attribute 'DbObject'");
            string tableName = dbObjectAttribute._tableName ?? classType.Name;    // If no alternative table-name is specified, use the class-name

            return tableName;
        }


        public static string SqlSerialise(object obj)
        {
            if (obj == null)            // Handle null
            {
                return "null";
            }
            else if (obj.GetType() == typeof(string))       // Handle strings
            {
                return "'" + SqlEscape((string)obj) + "'";  // wrap in sql-brackets and escape sql, if any
            }
            else if (obj.GetType() == typeof(byte) || obj.GetType() == typeof(int) || obj.GetType() == typeof(float) || obj.GetType() == typeof(double))  // Handle int, float & double
            {
                return obj.ToString().Replace(",", ".");    // just format to string and form comma to sql-comma
            }
            else if (obj.GetType() == typeof(DateTime))        // Handle DateTime
            {
                DateTime dateTime = (DateTime)obj;
                return "'" + SqlSerialise(dateTime) + "'";     // wrap in sql-brackets and convert to sql-datetime
            }
            else if (obj.GetType() == typeof(Guid))        // Handle DateTime
            {
                string guid = ((Guid)obj).ToString();  // Get Guid as string
                return "'" + guid + "'";                 // wrap in sql-brackets
            }
            else
            {
                return null;
            }
        }
    }
}
