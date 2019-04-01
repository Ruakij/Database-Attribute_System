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
            Dictionary<string, FieldInfo> dbFields = Function.ReadDbClassFields(classObject);

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
                // If the data was set
                bool dataIsSet = false;

                // Interate through class-fields
                foreach (KeyValuePair<string, FieldInfo> field_keySet in dbFields)
                { 
                    // If its a match, set the value
                    if (field_keySet.Key.ToLower() == data_keySet.Key.ToLower())
                    {
                        field_keySet.Value.SetValue(classObject, data_keySet.Value);
                        dataIsSet = true;
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Resolves an object with the database<para/>
        /// Needs to have primaryKey/s set!<para/>
        /// - Generates an query<para/>
        /// - Sends an query via Func<para/>
        /// - Fills the object with data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="queryExecutor">Function to handle query-calls - Has to return Dictionary[attributeName, attributeValue]</param>
        /// <param name="ignoreDataAttributeNotInClass">This disables errors when class-field has no data-attribute</param>
        public static void ResolveByPrimaryKey<T>(T classObject, Func<string, Dictionary<string, object>> queryExecutor, bool ignoreDataAttributeNotInClass = false)
        {
            string query = QueryBuilder.SelectByPrimaryKey(classObject);
            Dictionary<string, object> data = queryExecutor(query);
            FillObject(classObject, data, ignoreDataAttributeNotInClass);
        }
    }
}
