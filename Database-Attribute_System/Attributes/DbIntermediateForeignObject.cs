using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DbIntermediateForeignObject : Attribute
    {
        public Type foreignObjectType;

        public string _intermediateTableName;

        public string _keyName;
        public string _foreignKeyName;


        public DbPrimaryKey foreignPrimaryKeyAttribute;

        public FieldInfo parentField;
        public DbObject classAttribute;

        /// <summary>
        /// Marks variable as intermediate-object of an dbObject
        /// </summary>
        /// <param name="intermediateTableName">Table-name of intermediate-table. Must contain primaryKey of this class & target class</param>
        /// <param name="keyName">Fieldname of primaryKey associated with the IntermediateObject on this side [m]:n (null if same as primary key) [Only works with 1 primaryKey]</param>
        /// <param name="foreignKeyName">Fieldname of primaryKey associated with the IntermediateObject on the other side m:[n] (null if same as primary key) [Only works with 1 primaryKey]</param>
        public DbIntermediateForeignObject(string intermediateTableName, string keyName = null, string foreignKeyName = null)
        {
            this._intermediateTableName = intermediateTableName;
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
            if (classAttribute.primaryKeyAttributes.Count > 1) throw new InvalidOperationException($"IntermediateObject does not support multiple primaryKeys.");
            // Get primaryKey name if none is set
            if (_keyName == null) _keyName = classAttribute.primaryKeyAttributes[0]._attributeName;

            if (!(fi.FieldType is IList && fi.FieldType.IsGenericType))  // 1:m
                throw new InvalidOperationException($"IntermediateObject has to be typeof(List<T>). Maybe you meant to use DbForeignObject or DbReverseForeignObject for 1:m or 1:1 relations.");

            // Check the generic list and get inner-type
            Type foreignObjectType = null;
            foreach (Type interfaceType in fi.FieldType.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition()
                    == typeof(IList<>))
                {
                    foreignObjectType = fi.FieldType.GetGenericArguments()[0];
                    break;
                }
            }
            if (foreignObjectType == null) throw new InvalidOperationException("Could not read innter-type of generic-list!");

            // Now get the primaryKey from my foreignObject
            DbObject foreignDbObject = ClassAction.Init(foreignObjectType);
            // Check the primaryKey/s
            if (foreignDbObject.primaryKeyAttributes.Count < 1) throw new InvalidOperationException($"'{foreignDbObject.parentClassType.Name}' does not have a primaryKey.");
            if (foreignDbObject.primaryKeyAttributes.Count > 1) throw new InvalidOperationException($"IntermediateObject does not support multiple primaryKeys. (Found '{foreignDbObject.primaryKeyAttributes.Count}' in '{foreignDbObject.parentClassType.Name}')");
            // Save it
            foreignPrimaryKeyAttribute = foreignDbObject.primaryKeyAttributes[0];

            if (_foreignKeyName == null) _foreignKeyName = foreignPrimaryKeyAttribute._attributeName;
        }
    }
}
