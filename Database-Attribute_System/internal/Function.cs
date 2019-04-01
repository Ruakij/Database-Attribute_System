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
    }
}
