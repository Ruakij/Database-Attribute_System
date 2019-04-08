using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    public class ClassAction
    {
        /// <summary>
        /// Fills an given dbObject with given data<para/>
        /// Data-attribute-names and class-fieldNames have to match! (non case-sensitive)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="data">The data</param>
        /// <param name="runDataLossChecks">This checks if any class-field and data-attribute does not exists in either (Slower)</param>
        public static void FillObject<T>(T classObject, Dictionary<string, object> data, bool runDataLossChecks = true)
        {
            Type classType = classObject.GetType();

            string tableName = Function.GetDbTableName(classType);

            // Get class-fields
            Dictionary<string, FieldInfo> dbFields = Function.ReadDbClassFields(classType);

            if (runDataLossChecks)
            {
                // Check every data-attribute for match in class-fields
                foreach (KeyValuePair<string, object> data_keySet in data)
                {
                    bool isFound = false;
                    foreach (KeyValuePair<string, FieldInfo> field_keySet in dbFields)
                    {
                        if (field_keySet.Key.ToLower() == data_keySet.Key.ToLower())
                            isFound = true;
                    }

                    if (!isFound)
                        throw new InvalidOperationException($"Could not fill object. Data-Attribute '{data_keySet.Key}' was not found in class!");
                }
                // Check every class-field for match in data-attributes
                foreach (KeyValuePair<string, FieldInfo> field_keySet in dbFields)
                {
                    bool isFound = false;
                    foreach (KeyValuePair<string, object> data_keySet in data)
                    {
                        if (field_keySet.Key.ToLower() == data_keySet.Key.ToLower())
                            isFound = true;
                    }

                    if (!isFound)
                        throw new InvalidOperationException($"Could not fill object. Class-field '{field_keySet.Key}' was not found in data!");
                }
            }      

            // Iterate through data
            foreach (KeyValuePair<string, object> data_keySet in data)
            {
                // Interate through class-fields
                foreach (KeyValuePair<string, FieldInfo> field_keySet in dbFields)
                { 
                    // If its a match, set the value
                    if (field_keySet.Key.ToLower() == data_keySet.Key.ToLower())
                    {
                        field_keySet.Value.SetValue(classObject, data_keySet.Value);
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Gets an dbObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        /// <param name="runDataLossChecks">This checks if any class-field and data-attribute does not exists in either (Slower)</param>
        public static T GetByPrimaryKey<T>(Type classType, Func<string, List<Dictionary<string, object>>> queryExecutor, bool runDataLossChecks = true) where T: new()
        {
            T obj = new T();

            string query = QueryBuilder.SelectByPrimaryKey(obj);   // Generate query
            List<Dictionary<string, object>> dataSet = queryExecutor(query);    // Execute
            FillObject(obj, dataSet[0], runDataLossChecks);   // Fill the object

            return obj;
        }

        /// <summary>
        /// Gets a list of dbObjects by attribute/s
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType">Type of class</param>
        /// <param name="fields">class-fields for select</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        /// <param name="runDataLossChecks">This checks if any class-field and data-attribute does not exists in either (Slower)</param>
        /// <returns></returns>
        public static List<T> GetListByAttribute<T>(Type classType, Dictionary<string, object> fields, Func<string, List<Dictionary<string, object>>> queryExecutor, bool runDataLossChecks = true) where T : new()
        {
            string tableName = Function.GetDbTableName(classType);  // Get database-tableName

            Function.ConvertAttributeToDbAttributes(classType, fields);

            string query = QueryBuilder.SelectByAttribute(tableName, fields);   // Generate query
            List<Dictionary<string, object>> dataSet = queryExecutor(query);    // Execute

            List<T> objs = new List<T>() { };
            foreach(Dictionary<string, object> data in dataSet)
            {
                T obj = new T();    // New object
                FillObject(obj, data, runDataLossChecks);   // Fill it
                objs.Add(obj);      // Add to list
            }

            return objs;    // Return list
        }



        /// <summary>
        /// Resolves all foreignKeys with the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        /// <param name="max_depth">Determents how deep resolving will be executed</param>
        /// <param name="runDataLossChecks">This checks if any class-field and data-attribute does not exists in either (Slower)</param>
        public static void ResolveForeignKeys<T>(T classObject, Func<string, List<Dictionary<string, object>>> queryExecutor, int max_depth = 1, bool runDataLossChecks = true) where T: new()
        {
            Type classType = classObject.GetType();

            // Get class-fields
            Dictionary<string, FieldInfo> dbFields = Function.ReadDbClassFields(classType);

            foreach (KeyValuePair<string, FieldInfo> dbField in dbFields)
            {
                // If field is foreignKey
                if (dbField.Value.GetCustomAttribute(typeof(DbForeignKey), true) is DbForeignKey fkey)
                {
                    FieldInfo f_Field = fkey._foreignKeyField;
                    object f_value = f_Field.GetValue(classObject);

                    // When its empty, get it
                    if(f_value == null)
                    {
                        f_value = GetByPrimaryKey<T>(classType, queryExecutor, runDataLossChecks); ;
                    }

                    // Recursive resolving
                    if (max_depth - 1 > 0)
                    {
                        ResolveForeignKeys(f_value, queryExecutor, max_depth - 1, runDataLossChecks);
                    }
                }
            }
        }
    }
}
