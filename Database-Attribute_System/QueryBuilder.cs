using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    public class QueryBuilder
    {
        public static string SelectByPrimaryKeys<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Get db-table-name from class
            string tableName = Function.GetDbTableName(classType);

            // Get class db-fields
            Dictionary<string, object> dbPrimaryKeys = new Dictionary<string, object>() { };
            Dictionary<string, object> dbAttributes = new Dictionary<string, object>() { };
            Dictionary<string, object> dbForeignKeys = new Dictionary<string, object>() { };
            Function.ReadDbClassFields(classObject, ref dbPrimaryKeys, ref dbAttributes, ref dbForeignKeys);

            // Build where statements with primaryKey/s
            object[] param = Function.BuildKeyEqualQuery(dbPrimaryKeys, " AND ");
            // Add SQL-command part
            param[0] += $"SELECT * FROM {tableName} WHERE ";
            
            // Build and return the query
            return BuildQuery(param);
        }


        public static string UpdateByPrimaryKeys<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Get db-table-name from class
            string tableName = Function.GetDbTableName(classType);

            // Get class db-fields
            Dictionary<string, object> dbPrimaryKeys = new Dictionary<string, object>() { };
            Dictionary<string, object> dbAttributes = new Dictionary<string, object>() { };
            Dictionary<string, object> dbForeignKeys = new Dictionary<string, object>() { };
            Function.ReadDbClassFields(classObject, ref dbPrimaryKeys, ref dbAttributes, ref dbForeignKeys);

            // Add foreign-keys to attributes
            foreach(KeyValuePair<string, object> dbForeignKey in dbForeignKeys)
            {
                dbAttributes.Add(dbForeignKey.Key, dbForeignKey.Value);
            }

            // Build set-parameters
            object[] paramSet = Function.BuildKeyEqualQuery(dbAttributes, ", ");
            // Add SQL-command part
            paramSet[0] += $"UPDATE {tableName} SET ";

            // Build where-parameters
            object[] paramWhere = Function.BuildKeyEqualQuery(dbPrimaryKeys, " AND ");
            // Add SQL-command part
            paramWhere[0] += " WHERE ";

            // Build and return the query
            return BuildQuery(paramSet, paramWhere);
        }

        public static string DeleteByPrimaryKeys<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Get db-table-name from class
            string tableName = Function.GetDbTableName(classType);

            // Get class db-fields
            Dictionary<string, object> dbPrimaryKeys = new Dictionary<string, object>() { };
            Dictionary<string, object> dbAttributes = new Dictionary<string, object>() { };
            Dictionary<string, object> dbForeignKeys = new Dictionary<string, object>() { };
            Function.ReadDbClassFields(classObject, ref dbPrimaryKeys, ref dbAttributes, ref dbForeignKeys);

            // Build where-parameters
            object[] paramWhere = Function.BuildKeyEqualQuery(dbPrimaryKeys, " AND ");
            // Add SQL-command part
            paramWhere[0] += $"DELETE FROM {tableName} WHERE ";

            // Build and return the query
            return BuildQuery(paramWhere);
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
