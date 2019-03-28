using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    public class QueryBuilder
    {
        /// <summary>
        /// Build an SELECT-SQL-query by a database-primary-key<pragma/>
        /// Will build the query-string based on the <paramref name="comparisonValue"/> including serialising and escaping for SQL.<para/>
        /// </summary>
        /// <param name="handler">Method to send querys to the database</param>
        /// <param name="primaryKey">Database comparison value</param>
        /// <returns>Built SQL-Query</returns>
        public static string SelectByPrimaryKey(Type classType, object comparisonValue) => SelectByPrimaryKeys(classType, comparisonValue);
        /// <summary>
        /// Build an SELECT-SQL-query by database-primary-keys<pragma/>
        /// Will build the query-string based on the <paramref name="comparisonValues"/> including serialising and escaping for SQL.<para/>
        /// </summary>
        /// <param name="handler">Method to send querys to the database</param>
        /// <param name="primaryKeys">Database comparison values (Must be the exact same amount as primary keys in class/table)</param>
        /// <returns>Built SQL-Query</returns>
        public static string SelectByPrimaryKeys(Type classType, params object[] comparisonValues)
        {
            // Check if class has attribute 'DbObject' and get the database table-name
            if (!(classType.GetCustomAttribute(typeof(DbObject), true) is DbObject dbObjectAttribute)) throw new InvalidOperationException("Cannot generate SQL-Query of class. Missing Attribute 'DbObject'");
            string tableName = dbObjectAttribute._tableName ?? classType.Name;    // If no alternative table-name is specified, use the class-name

            // Iterate thru all properties
            List<string> dbPrimaryKeys = new List<string>() { };
            foreach (System.Reflection.FieldInfo fi in classType.GetRuntimeFields())
            {
                // Get primaryKey attribute from current property
                if (fi.GetCustomAttribute(typeof(DbPrimaryKey), true) is DbPrimaryKey pkey)
                {
                    string dbAttributeName = pkey._attributeName ?? fi.Name;     // If no alternative attribute-name is specified, use the property-name
                    dbPrimaryKeys.Add(dbAttributeName);
                }
            }

            if (comparisonValues.Length != dbPrimaryKeys.Count) throw new InvalidOperationException("Primary-key number of class/table and number of comparison values is not equal!"); 


            object[] param = new object[comparisonValues.Length * 2];
            for (int i = 0, c = 0; c < param.Length; i++, c += 2)
            {
                string sql_string = "";

                if (i == 0) sql_string += $"SELECT * FROM {tableName} WHERE ";
                else sql_string += " AND ";
                sql_string += $"{dbPrimaryKeys[i]}=";

                param[c] = sql_string;
                param[c + 1] = comparisonValues[i];
            }

            return BuildQuery(param);
        }


        /// <summary>
        /// Build an SQL-query<para/>
        /// Will build the query-string based on the <paramref name="param"/>s including serialising and escaping for SQL.<para/>
        /// </summary>
        /// <param name="param">Params with alternating query-parts and objects beginning with a query-part
        /// <para/>Example: "SELECT * FROM table WHERE id=", id, " AND name=", name</param>
        public static string BuildQuery(params object[] param)
        {
            // Convert array to list and add object[] to it accordingly
            List<object> paramz = new List<object>() { };
            foreach (object obj in param)
            {
                if (!(obj is object[]))
                    paramz.Add(obj);
                else
                {
                    foreach (object obj2 in (object[])obj)
                    {
                        paramz.Add(obj2);
                    }
                }
            }

            string query = "";
            for (int i = 0; i < paramz.Count; i++)
            {
                if (i % 2 == 0) query += paramz[i];  // Every 'even' count will just add
                else       // Every 'uneven' count will handle param as passed variable    
                {
                    string paramString = "";
                    if (paramz[i] == null)            // Handle null
                    {
                        paramString = "null";
                    }
                    else if (paramz[i].GetType() == typeof(string))       // Handle strings
                    {
                        paramString = "'" + Function.SqlEscape((string)paramz[i]) + "'";  // wrap in sql-brackets and escape sql, if any
                    }
                    else if (paramz[i].GetType() == typeof(int) || paramz[i].GetType() == typeof(float) || paramz[i].GetType() == typeof(double))  // Handle int, float & double
                    {
                        paramString = paramz[i].ToString().Replace(",", ".");    // just format to string and form comma to sql-comma
                    }
                    else if (paramz[i].GetType() == typeof(DateTime))        // Handle DateTime
                    {
                        DateTime dateTime = (DateTime)paramz[i];
                        paramString = "'" + Function.SqlSerialise(dateTime) + "'";     // wrap in sql-brackets and convert to sql-datetime
                    }
                    else if (paramz[i].GetType() == typeof(Guid))        // Handle DateTime
                    {
                        string guid = ((Guid)paramz[i]).ToString();  // Get Guid as string
                        paramString = "'" + guid + "'";                 // wrap in sql-brackets
                    }
                    else        // Unknown type in params
                    {
                        throw new Exception($"Error in query-builder: Type '{paramz[i].GetType().ToString()}' cannot be used!");
                    }

                    // Add formed param to query
                    query += paramString;
                }
            }

            // return built query
            return query;
        }
    }
}
