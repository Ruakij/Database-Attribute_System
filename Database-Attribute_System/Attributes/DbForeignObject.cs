using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbForeignObject : Attribute
    {
        public Type foreignObjectType;

        public string _foreignKeyName;
        public DbForeignKey foreignKeyAttribute;

        public FieldInfo parentField;
        public DbObject classAttribute;

        /// <summary>
        /// Marks variable as foreign-object of an dbObject
        /// </summary>
        /// <param name="foreignKeyName">Fieldname of foreignKey associated with the foreignObject</param>
        public DbForeignObject(string foreignKeyName = null)
        {
            this._foreignKeyName = foreignKeyName;
        }

        public void Init(FieldInfo fi, DbObject classAttribute, DbObject foreignClassAttribute = null)
        {
            this.parentField = fi;
            this.classAttribute = classAttribute;
            this.foreignObjectType = fi.FieldType;

            // Init foreign-object class
            if(foreignClassAttribute == null)
                foreignClassAttribute = ClassAction.Init(this.foreignObjectType);

            // Check if something is weird
            if (foreignClassAttribute.primaryKeyAttributes.Count < 1) throw new InvalidOperationException($"'{foreignClassAttribute.parentClassType.Name}' does not have a primaryKey.");
            if (foreignClassAttribute.primaryKeyAttributes.Count > 1) throw new InvalidOperationException($"ForeignObject does not support multiple primaryKeys.");

            Type primaryKeyType = foreignClassAttribute.primaryKeyAttributes[0].parentField.GetType();  // Read type of primaryKey in foreignObject-class
            foreach(DbForeignKey foreignKey in classAttribute.foreignKeyAttributes)     // Search for matching foreignKey
            {
                if(this._foreignKeyName != null)     // If i have a name
                {
                    // check if name matches
                    if (foreignKey.parentField.Name.ToLower() == this._foreignKeyName.ToLower())
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
