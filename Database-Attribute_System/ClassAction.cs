﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    class ClassAction
    {
        /// <summary>
        /// Fills an given dbObject with given data<para/>
        /// Data-attribute-names and class-fieldNames have to match! (non case-sensitive)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classObject">Given object (marked with Db-attributes)</param>
        /// <param name="data">The data</param>
        /// <param name="ignoreDataAttributeNotInClass">This disables errors when class-field has no data-attribute</param>
        public static void FillObject<T>(T classObject, Dictionary<string, object> data, bool ignoreDataAttributeNotInClass = false)
        {
            Type classType = classObject.GetType();

            string tableName = Function.GetDbTableName(classType);

            // Get class-fields
            Dictionary<string, FieldInfo> dbFields = Function.ReadDbClassFields(classObject);

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

                // If the field was not filled, throw an error if it will not be ignored
                if (!ignoreDataAttributeNotInClass && !dataIsSet) throw new InvalidOperationException($"Could not fill object. Data-Attribute '{data_keySet.Key}' was not found class!");
            }
        }


        public static void ResolveByPrimaryKey<T>(T classObject, Func<string, Dictionary<string, object>> queryExecutor, bool ignoreDataAttributeNotInClass = false)
        {
            string query = QueryBuilder.SelectByPrimaryKeys(classObject);
            Dictionary<string, object> data = queryExecutor(query);
            FillObject(classObject, data, ignoreDataAttributeNotInClass);
        }
    }
}