using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbReverseForeignObject : Attribute
    {
        public Type foreignObjectType;

        public string _foreignKeyName;
        public DbForeignKey foreignKeyAttribute;

        public FieldInfo parentField;
        public DbObject classAttribute;

        /// <summary>
        /// Marks variable as reverse-foreign-object of an dbObject
        /// </summary>
        /// <param name="foreignKeyName">Fieldname of primaryKey associated with the reverseForeignObject (null if same as primary key) [Only works with 1 primaryKey]</param>
        public DbReverseForeignObject(string foreignKeyName = null)
        {
            this._foreignKeyName = foreignKeyName;
        }

        public void Init(FieldInfo fi, DbObject classAttribute)
        {
            this.parentField = fi;
            this.classAttribute = classAttribute;
            this.foreignObjectType = fi.FieldType;

            // Init foreign-object class
            DbObject foreignClassAttribute = ClassAction.Init(this.foreignObjectType);

            if (classAttribute.primaryKeyAttributes.Count < 1) throw new InvalidOperationException($"'{classAttribute.parentClassType.Name}' does not have a primaryKey.");
            if (classAttribute.primaryKeyAttributes.Count > 1) throw new InvalidOperationException($"ReverseForeignObject does not support multiple primaryKeys.");
            // Get primaryKey name if none is set
            if (_foreignKeyName == null) _foreignKeyName = classAttribute.primaryKeyAttributes[0]._attributeName;

            // Check if my primary-key is set in the foreign-class as foreignKey
            DbPrimaryKey primaryKey = classAttribute.primaryKeyAttributes[0];
            foreach (DbForeignKey foreignKey in foreignClassAttribute.foreignKeyAttributes)
            {
                if (primaryKey._attributeName.ToLower() == foreignKey._attributeName.ToLower())  // Name matches
                    if (primaryKey.parentField.GetType() == foreignKey.parentField.GetType())   // Type matches
                    {
                        foreignKeyAttribute = foreignKey;
                    }
                    else
                        // Same name, but wrong type
                        throw new InvalidOperationException($"ForeignObject='{foreignClassAttribute.parentClassType.Name}' has invalid type foreignKey='{foreignKey.parentField.Name}' for object='{classAttribute.parentClassType.Name}' with primaryKey='{primaryKey.parentField.Name}'.");
            }
            // No match
            if (foreignKeyAttribute == null) throw new InvalidOperationException($"ForeignObject='{foreignClassAttribute.parentClassType.Name}' is missing foreignKey for object='{classAttribute.parentClassType.Name}' with primaryKey='{primaryKey.parentField.Name}'.");
        }
    }
}
