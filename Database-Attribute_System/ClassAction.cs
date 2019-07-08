using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    public class ClassAction
    {
        private static Dictionary<Type, DbObject> initiatedClassTypes = new Dictionary<Type, DbObject>() { };
        /// <summary>
        /// Initiates the attribute-system and preloads all necessary information<para/>
        /// INFO: Will initiate necessary foreignObjects recursively!<para/>
        /// If an class is already initiated, it will be ignored!
        /// </summary>
        /// <param name="classType">The classType to preload</param>
        /// <returns>DbObject-attribute corresponding to the class</returns>
        public static DbObject Init(Type classType)
        {
            DbObject cachedDbObject;
            initiatedClassTypes.TryGetValue(classType, out cachedDbObject);

            if (cachedDbObject == null)
            {
                // Check if given class is marked as dbObject
                if (!(classType.GetCustomAttribute(typeof(DbObject), true) is DbObject dbObject)) throw new InvalidOperationException($"Cannot init '{classType.Name}'. Missing Attribute 'DbObject'");

                dbObject.Init(classType);   // Init dbObject
                initiatedClassTypes.Add(classType, dbObject);     // Set it to the list

                cachedDbObject = dbObject;
            }

            return cachedDbObject;
        }


        /// <summary>
        /// Fills an given dbObject with given data<para/>
        /// Data-attribute-names and class-fieldNames have to match! (non case-sensitive)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="data">The data</param>
        public static void FillObject<T>(T classObject, Dictionary<string, object> data)
        {
            Type classType = classObject.GetType();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            // Iterate through data
            foreach (KeyValuePair<string, object> data_keySet in data)
            {
                // Interate through class-fields
                foreach (BaseAttribute baseAttribute in dbObject.baseAttributes)
                {
                    // Remove any leading dots and table-specifiers
                    string dbAttName = data_keySet.Key;
                    if (dbAttName.Contains("."))
                    {
                        string[] dbAttNameSplit = dbAttName.Split('.');     // Split at the '.'
                        dbAttName = dbAttNameSplit[dbAttNameSplit.Length - 1];  // Copy the ending text
                    }

                    // If its a match, set the value
                    if (baseAttribute._attributeName.ToLower() == data_keySet.Key.ToLower())
                    {
                        object value = data_keySet.Value;
                        //if (baseAttribute.parentField.FieldType == typeof(Guid)) value = new Guid((string)value);   // If its a guid, i need to convert

                        baseAttribute.parentField.SetValue(classObject, value);
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Gets an dbObject by primaryKey/s
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        public static T GetByPrimaryKey<T>(Type classType, object primaryKeyValue, Func<string, List<Dictionary<string, object>>> queryExecutor) where T : new()
        {
            Dictionary<string, object> primaryKeyData = new Dictionary<string, object>() { };
            primaryKeyData.Add(null, primaryKeyValue);

            return GetByPrimaryKey<T>(classType, primaryKeyData, queryExecutor);
        }
        public static T GetByPrimaryKey<T>(Type classType, string primaryKeyName, object primaryKeyValue, Func<string, List<Dictionary<string, object>>> queryExecutor) where T : new()
        {
            Dictionary<string, object> primaryKeyData = new Dictionary<string, object>() { };
            primaryKeyData.Add(primaryKeyName, primaryKeyValue);

            return GetByPrimaryKey<T>(classType, primaryKeyData, queryExecutor);
        }
        public static T GetByPrimaryKey<T>(Type classType, Dictionary<string, object> primaryKeyData, Func<string, List<Dictionary<string, object>>> queryExecutor) where T: new()
        {
            // Create new empty object
            T obj = new T();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            // iterate thru them to check and fill object
            foreach (DbPrimaryKey primaryKeyAtt in dbObject.primaryKeyAttributes)
            {
                bool dataMatchFound = false;

                // Now search the corresponding primaryKeyData
                foreach (KeyValuePair<string, object> primaryKey in primaryKeyData)
                {
                    // primaryKey matches
                    if(primaryKeyAtt._attributeName.ToLower() == primaryKey.Key.ToLower())
                    {
                        // Set data
                        primaryKeyAtt.parentField.SetValue(obj, primaryKey.Value);

                        dataMatchFound = true;
                        break;
                    }
                }

                // If no data was found matching this field
                if (!dataMatchFound) throw new InvalidOperationException($"Cannot create object with primaryKeyData. No data assigned to field '{primaryKeyAtt.parentField.Name}'");
            }

            ResolveByPrimaryKey<T>(obj, queryExecutor);

            return obj;
        }

        // ----

        /// <summary>
        /// Gets all dbObjects of class/table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        public static List<T> GetList<T>(Type classType, Func<string, List<Dictionary<string, object>>> queryExecutor) where T : new()
        {
            // Read dbObject - attribute
            DbObject dbObject = ClassAction.Init(classType);

            string query = QueryBuilder.SelectByAttribute(dbObject._tableName);   // Generate query
            List<Dictionary<string, object>> dataSet = queryExecutor(query);    // Execute

            List<T> objs = new List<T>() { };
            foreach (Dictionary<string, object> data in dataSet)
            {
                T obj = new T();    // New object
                FillObject(obj, data);   // Fill it
                objs.Add(obj);      // Add to list
            }

            return objs;    // Return list
        }

        /// <summary>
        /// Gets an dbObject by primaryKey/s
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="whereClause">Custom where-clause params attached to query (SELECT * FROM tableName WHERE whereClause)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        public static List<T> GetListWithWhere<T>(Type classType, Func<string, List<Dictionary<string, object>>> queryExecutor, params object[] whereClause) where T : new()
        {
            // Read dbObject - attribute
            DbObject dbObject = ClassAction.Init(classType);

            string query = QueryBuilder.SelectWithWhere(dbObject._tableName, whereClause);   // Generate query
            List<Dictionary<string, object>> dataSet = queryExecutor(query);    // Execute

            List<T> objs = new List<T>() { };
            foreach (Dictionary<string, object> data in dataSet)
            {
                T obj = new T();    // New object
                FillObject(obj, data);   // Fill it
                objs.Add(obj);      // Add to list
            }

            return objs;    // Return list
        }
        
        /// <summary>
        /// Gets a list of dbObjects by attribute/s
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType">Type of class</param>
        /// <param name="fields">class-fields for select</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        /// <param name="runDataLossChecks">This checks if any class-field and data-attribute does not exists in either (Slower)</param>
        /// <returns>List of dbObjects</returns>
        public static List<T> GetListByAttribute<T>(Type classType, Dictionary<string, object> fields, Func<string, List<Dictionary<string, object>>> queryExecutor) where T : new()
        {
            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            Function.ConvertAttributeToDbAttributes(classType, fields);

            string query = QueryBuilder.SelectByAttribute(dbObject._tableName, fields);   // Generate query
            List<Dictionary<string, object>> dataSet = queryExecutor(query);    // Execute

            List<T> objs = new List<T>() { };
            foreach(Dictionary<string, object> data in dataSet)
            {
                T obj = new T();    // New object
                FillObject(obj, data);   // Fill it
                objs.Add(obj);      // Add to list
            }

            return objs;    // Return list
        }

        // -----

        /// <summary>
        /// Resolves dbObject by primaryKey/s<pragma/>
        /// Object needs to have primaryKey/s set!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        public static void ResolveByPrimaryKey<T>(T classObject, Func<string, List<Dictionary<string, object>>> queryExecutor)
        {
            string query = QueryBuilder.SelectByPrimaryKey(classObject);   // Generate query
            List<Dictionary<string, object>> dataSet = queryExecutor(query);    // Execute
            FillObject(classObject, dataSet[0]);   // Fill the object
        }

        /// <summary>
        /// Resolves all foreignKeys with the database<pragma/>
        /// Only works if the foreignKey is single (not assembled)!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        /// <param name="max_depth">Determents how deep resolving will be executed</param>
        /// <param name="runDataLossChecks">This checks if any class-field and data-attribute does not exists in either (Slower)</param>
        public static void ResolveForeignKeys<T>(T classObject, Func<string, List<Dictionary<string, object>>> queryExecutor, int max_depth = 1) where T: new()
        {
            Type classType = classObject.GetType();

            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            foreach (DbForeignObject foreignObjectAtt in dbObject.foreignObjectAttributes)
            {
                object foreignObject_value = foreignObjectAtt.parentField.GetValue(classObject);

                // When its empty, get it
                if(foreignObject_value == null)
                {
                    foreignObject_value = GetByPrimaryKey<T>(classType, foreignObjectAtt.foreignKeyAttribute.parentField.GetValue(classObject), queryExecutor);
                }

                // Recursive resolving
                if (max_depth - 1 > 0)
                {
                    ResolveForeignKeys(foreignObject_value, queryExecutor, max_depth - 1);
                }
            }
        }
    }
}
