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

        internal static List<BaseAttribute> ConvertToDerivedList(List<DbPrimaryKey> list)
        {
            List<BaseAttribute> derivedList = new List<BaseAttribute>() { };
            foreach (BaseAttribute key in list)
            {
                derivedList.Add(key);
            }
            return derivedList;
        }
        internal static List<BaseAttribute> ConvertToDerivedList(List<DbAttribute> list)
        {
            List<BaseAttribute> derivedList = new List<BaseAttribute>() { };
            foreach (BaseAttribute key in list)
            {
                derivedList.Add(key);
            }
            return derivedList;
        }
        internal static List<BaseAttribute> ConvertToDerivedList(List<DbForeignKey> list)
        {
            List<BaseAttribute> derivedList = new List<BaseAttribute>() { };
            foreach (BaseAttribute key in list)
            {
                derivedList.Add(key);
            }
            return derivedList;
        }

        internal static Dictionary<string, object> ReadFieldData<T>(List<BaseAttribute> fieldAttributes, T classObject)
        {
            Dictionary<string, object> fieldData = new Dictionary<string, object>() { };

            foreach (BaseAttribute attribute in fieldAttributes)
            {
                // Read the data and add it
                fieldData.Add(
                    attribute._attributeName,
                    attribute.parentField.GetValue(classObject)
                    );
            }

            return fieldData;   // Return the data
        }

        internal static void ConvertAttributeToDbAttributes(Type classType, Dictionary<string, object> attributeNameAndValues)
        {
            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);
            Dictionary<string, object> convertedAttributeNameAndValues = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> attributeNameAndValue in attributeNameAndValues)
            {
                bool nameFound = false;
                foreach (BaseAttribute baseAttribute in dbObject.baseAttributes)
                {
                    if (attributeNameAndValue.Key.ToLower() == baseAttribute.parentField.Name.ToLower())
                    {
                        convertedAttributeNameAndValues.Add(baseAttribute._attributeName, attributeNameAndValue.Value);

                        nameFound = true;
                        break;
                    }
                }

                if (!nameFound) throw new InvalidOperationException($"{attributeNameAndValue.Key} has no classField!");
            }
        }
        internal static void ConvertAttributeToDbAttributes(Type classType, List<string> attributeNames)
        {
            // Read dbObject-attribute
            DbObject dbObject = ClassAction.Init(classType);

            for(int i=0; i< dbObject.baseAttributes.Count; i++)
            {
                bool nameFound = false;
                foreach (BaseAttribute baseAttribute in dbObject.baseAttributes)
                {
                    if(attributeNames[i].ToLower() == baseAttribute.parentField.Name.ToLower())
                    {
                        attributeNames[i] = baseAttribute._attributeName;

                        nameFound = true;
                        break;
                    }
                }

                if (!nameFound) throw new InvalidOperationException($"{attributeNames[i]} has no classField!");
            }
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
