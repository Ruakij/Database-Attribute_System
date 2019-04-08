﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbForeignKey : Attribute
    {
        public Type _classType;
        public string _attributeName;
        public string _foreignKeyFieldName;
        public FieldInfo _foreignKeyField;

        /// <summary>
        /// Marks variable as foreignKey of given class
        /// </summary>
        /// <param name="classType">Type of class to which this is the ForeignKey</param>
        /// <param name="dbAttributeName">Name of database-attribute (case-sensitivity is determined from database-attribute-settings) ['null' if the same as field-name]</param>
        /// <param name="foreignKeyFieldName">Name of foreignKey-fieldName ['null' if the same as classType-name]</param>
        public DbForeignKey(Type classType, string foreignKeyFieldName = null, string attributeName = null)
        {
            this._classType = classType;
            this._attributeName = attributeName;    // Todo: Automatic resolving of name if it is null (?)

            bool fieldFound = false;
            foreach (System.Reflection.FieldInfo fi in classType.GetRuntimeFields())
            {
                if(fi.Name.ToLower() == foreignKeyFieldName.ToLower())
                {
                    this._foreignKeyFieldName = fi.Name;
                    this._foreignKeyField = fi;

                    fieldFound = true;
                    break;
                }
            }
            if (!fieldFound) throw new InvalidOperationException($"Field with name='{foreignKeyFieldName}' not found in {classType.Name}.");
        }
    }
}
