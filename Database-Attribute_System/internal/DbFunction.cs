using eu.railduction.netcore.dll.Database_Attribute_System.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    class DbFunction
    {
        internal static object[] BuildKeyEqualQuery(Dictionary<string, object> keysets, string seperator)
        {
            object[] param = new object[keysets.Count * 2];
            int c = 0;
            foreach (KeyValuePair<string, object> keyset in keysets)
            {
                string sql_string = "";

                if (c != 0) sql_string += seperator;
                sql_string += $"{keyset.Key}=";

                param[c] = sql_string;
                param[c + 1] = keyset.Value;

                c += 2;
            }

            return param;
        }
    }
}
