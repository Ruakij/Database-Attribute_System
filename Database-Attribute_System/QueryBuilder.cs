using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    public class QueryBuilder
    {
        /// <summary>
        /// Builds an SELECT-Sql-query based on an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="tableName">The db-table-name</param>
        /// <returns>SELECT-Sql-query</returns>
        public static string SelectByPrimaryKey<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            // Check if 'byPrimaryKey' is possible
            if (dbObject.primaryKeyAttributes.Count == 0) throw new InvalidOperationException($"Cannot generate SQL-Query of '{classType.Name}'. No primary-key/s!");
            Dictionary<string, object> dbPrimaryKeys = Function.ReadFieldData(Function.ConvertToDerivedList(dbObject.primaryKeyAttributes), classObject);

            return SelectByAttribute(dbObject._tableName, dbPrimaryKeys);
        }

        /// <summary>
        /// Builds an SELECT-Sql-query based on an object<para/>
        /// Object needs to have at least 1 attribute!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">The db-table-name</param>
        /// <param name="dbAttributes">The db-attributes with dbAttribute-name and value<para/>If null is given, it will generate a default 'SELECT * FROM tableName'</param>
        /// <returns>SELECT-Sql-query</returns>
        public static string SelectByAttribute(string tableName, Dictionary<string, object> dbAttributes = null)
        {
            object[] param = new object[1];
            if (dbAttributes != null)
            {
                // Build where statements with primaryKey/s
                param = DbFunction.BuildKeyEqualQuery(dbAttributes, " AND ");
            }

            string sqlCmd = $"SELECT * FROM {tableName}";
            // Add SQL-command part
            if (dbAttributes != null)
                param[0] = $"{sqlCmd} WHERE {param[0]}";
            else
                param[0] = sqlCmd;

            // Build and return the query
            return BuildQuery(param);
        }

        /// <summary>
        /// Builds an UPDATE-Sql-query based on an object<para/>
        /// Object needs to have at least 1 primary-key!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <returns>UPDATE-Sql-query</returns>
        public static string UpdateByPrimaryKey<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            // Check if 'byPrimaryKey' is possible
            if (dbObject.primaryKeyAttributes.Count == 0) throw new InvalidOperationException($"Cannot generate SQL-Query of '{classType.Name}'. No primary-key/s!");
            Dictionary<string, object> dbPrimaryKeys = Function.ReadFieldData(Function.ConvertToDerivedList(dbObject.primaryKeyAttributes), classObject);
            Dictionary<string, object> dbForeignKeys = Function.ReadFieldData(Function.ConvertToDerivedList(dbObject.foreignKeyAttributes), classObject);
            Dictionary<string, object> dbAttributes = Function.ReadFieldData(Function.ConvertToDerivedList(dbObject.attributeAttributes), classObject);

            // Add foreign-keys to attributes
            foreach (KeyValuePair<string, object> dbForeignKey in dbForeignKeys)
            {
                dbAttributes.Add(dbForeignKey.Key, dbForeignKey.Value);
            }

            // Build set-parameters
            object[] paramSet = DbFunction.BuildKeyEqualQuery(dbAttributes, ", ");
            // Add SQL-command part
            paramSet[0] = $"UPDATE {dbObject._tableName} SET "+ paramSet[0];

            // Build where-parameters
            object[] paramWhere = DbFunction.BuildKeyEqualQuery(dbPrimaryKeys, " AND ");
            // Add SQL-command part
            paramWhere[0] = " WHERE "+ paramWhere[0];

            // Build and return the query
            return BuildQuery(paramSet, paramWhere);
        }

        /// <summary>
        /// Builds an DELETE-Sql-query based on an object<para/>
        /// Object needs to have at least 1 primary-key!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <returns>DELETE-Sql-query</returns>
        public static string DeleteByPrimaryKey<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            // Check if 'byPrimaryKey' is possible
            if (dbObject.primaryKeyAttributes.Count == 0) throw new InvalidOperationException($"Cannot generate SQL-Query of '{classType.Name}'. No primary-key/s!");
            Dictionary<string, object> dbPrimaryKeys = Function.ReadFieldData(Function.ConvertToDerivedList(dbObject.primaryKeyAttributes), classObject);

            // Build and return the query
            return DeleteByAttribute(dbObject._tableName, dbPrimaryKeys);
        }

        public static string DeleteByAttribute(string tableName, Dictionary<string, object> dbAttributes = null)
        {
            object[] param = new object[1];
            if (dbAttributes != null)
            {
                // Build where statements with primaryKey/s
                param = DbFunction.BuildKeyEqualQuery(dbAttributes, " AND ");
            }

            string sqlCmd = $"DELETE FROM {tableName}";
            // Add SQL-command part
            if (dbAttributes != null)
                param[0] = $"{sqlCmd} WHERE {param[0]}";
            else
                param[0] = sqlCmd;

            // Build and return the query
            return BuildQuery(param);
        }

        /// <summary>
        /// Builds an INSERT-Sql-query based on an object<para/>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dbAttributes"></param>
        /// <returns></returns>
        public static string InsertAttributesByObject<T>(T classObject)
        {
            Type classType = classObject.GetType();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            List<string> attributes = new List<string>() { };
            List<object> data = new List<object>() { };
            foreach(BaseAttribute baseAttribute in dbObject.baseAttributes)
            {
                attributes.Add(baseAttribute._attributeName);
                data.Add(baseAttribute.parentField.GetValue(classObject));
            }

            return InsertAttributes(dbObject._tableName, attributes, data);
        }
        public static string InsertAttributes(string tableName, Dictionary<string, object> dbAttributes)
        {
            if (dbAttributes.Count == 0) throw new InvalidOperationException("Cannot generate SQL-Query. No attributes to insert.");

            List<string> attributes = new List<string>() { };
            List<object> data = new List<object>() { };

            foreach (KeyValuePair<string, object> attribute in dbAttributes)
            {
                attributes.Add(attribute.Key);
                data.Add(attribute.Value);
            }

            return InsertAttributes(tableName, attributes, data);
        }
        public static string InsertAttributes(string tableName, List<string> attributes, List<object> data)
        {
            if (attributes.Count != data.Count) throw new InvalidOperationException("Cannot generate SQL-Query. Attribute-count and data-count not equal.");

            string attributesSeperatedByComma = "";
            object[] attributeData = new object[attributes.Count*2];
            int c = 0;
            for(int i=0; i<attributes.Count*2; i+=2)
            {
                attributesSeperatedByComma += attributes[i];
                attributeData[c] = data[i+1];

                if(c+1 != attributes.Count*2)
                {
                    attributesSeperatedByComma += ", ";
                    attributeData[c+1] = ",";
                }
                c +=2;
            }

            // Build and return the query
            return BuildQuery($"INSERT INTO {tableName} ({attributesSeperatedByComma}) VALUES (", attributeData, ")");
        }



        /// <summary>
        /// Build an SQL-query<para/>
        /// Will build the query-string based on the <paramref name="param"/>s including serialising and escaping for SQL.<para/>
        /// (Supported-Types: null, string, byte, int, float, double, DateTime, Guid)
        /// </summary>
        /// <param name="param">Params with alternating query-parts and objects beginning with a query-part
        /// <para/>Example: "SELECT * FROM table WHERE id=", id, " AND name=", name<para/>
        /// Info: Any object[] will be opened recursively and added as <paramref name="param"/> accordingly!
        /// </param>
        public static string BuildQuery(params object[] param)
        {
            // Convert array to list and add object[] to it recursively
            List<object> paramz = new List<object>() { };
            Function.RecursiveParameterCopying(ref paramz, (object[])param);

            string query = "";
            for (int i = 0; i < paramz.Count; i++)
            {
                if (i % 2 == 0) query += paramz[i];  // Every 'even' count will just add
                else       // Every 'uneven' count will handle param as passed variable    
                {
                    string paramString = Function.SqlSerialise(paramz[i]);
                    if(paramString == null)        // Unknown type in params
                        throw new Exception($"Error in query-builder. Type '{paramz[i].GetType().ToString()}' cannot be serialised!");

                    // Add formed param to query
                    query += paramString;
                }
            }

            // return built query
            return query;
        }
    }
}
