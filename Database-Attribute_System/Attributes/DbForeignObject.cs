﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbForeignObject : Attribute
    {
        public Type _foreignObjectType;

        public string _foreignKeyName;
        public DbForeignKey foreignKeyAttribute;

        public FieldInfo parentField;
        public DbObject classAttribute;

        /// <summary>
        /// Marks variable as foreign-object of an dbObject
        /// </summary>
        /// <param name="foreignObjectType">Type of foreignObject</param>
        /// <param name="foreignKeyName">Fieldname of foreignKey associated with the foreignObject</param>
        public DbForeignObject(Type foreignObjectType, string foreignKeyName = null)
        {
            this._foreignObjectType = foreignObjectType;
            this._foreignKeyName = foreignKeyName;
        }

        public void Init(FieldInfo fi, DbObject classAttribute)
        {
            this.parentField = fi;
            this.classAttribute = classAttribute;

            // Init foreign-object class
            DbObject foreignClassAttribute = ClassAction.Init(this._foreignObjectType);

            // Check if something is weird
            if (foreignClassAttribute.primaryKeyAttributes.Count < 1) throw new InvalidOperationException($"'{foreignClassAttribute.parentClassType.Name}' does not have a primaryKey.");
            if (foreignClassAttribute.primaryKeyAttributes.Count > 1) throw new InvalidOperationException($"ForeignObject does not support multiple primaryKeys.");

            Type primaryKeyType = foreignClassAttribute.primaryKeyAttributes[0].parentField.GetType();  // Read type of primaryKey in foreignObject-class
            foreach(DbForeignKey foreignKey in classAttribute.foreignKeyAttributes)     // Search for matching foreignKey
            {
                if(this._foreignKeyName != null)     // If i have a name
                {
                    // check if name matches
                    if (foreignKey.parentField.Name.ToLower() == this._foreignKeyName)
                    {
                        if(foreignKey.parentField.GetType() == primaryKeyType)
                        {
                            this._foreignKeyName = foreignKey.parentField.Name;
                            foreignKeyAttribute = foreignKey;
                            break;
                        }
                        else
                        {
                            // If a name was specified and the key does not match its an error
                            throw new InvalidOperationException($"ForeignKey='{this._foreignKeyName}' is typeOf='{foreignKey.parentField.GetType().Name}' but primaryKey of foreignObject-class is typeOf='{primaryKeyType.Name}'.");
                        }
                    }
                }
                else    // No name
                {
                    // Check if type matches
                    if (foreignKey.parentField.GetType() == primaryKeyType)
                    {
                        this._foreignKeyName = foreignKey.parentField.Name;
                        foreignKeyAttribute = foreignKey;
                        break;
                    }
                }
            }

            // Check if key-retrieval was successful
            if (foreignKeyAttribute == null) throw new InvalidOperationException($"No coresponding foreignKey.");
        }
    }
}
