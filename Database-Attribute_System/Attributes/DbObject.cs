using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eu.railduction.netcore.dll.Database_Attribute_System.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DbObject : Attribute
    {
        public string _tableName;

        public Type parentClassType;
        public ConstructorInfo parentCInfo;

        // All childrenAttributes
        public List<BaseAttribute> baseAttributes = new List<BaseAttribute>() { };
        public List<DbPrimaryKey> primaryKeyAttributes = new List<DbPrimaryKey>() { };
        public List<DbAttribute> attributeAttributes = new List<DbAttribute>() { };
        public List<DbForeignKey> foreignKeyAttributes = new List<DbForeignKey>() { };
        public List<DbForeignObject> foreignObjectAttributes = new List<DbForeignObject>() { };
        public List<DbReverseForeignObject> reverseForeignObjectAttributes = new List<DbReverseForeignObject>() { };
        public List<DbIntermediateForeignObject> intermediateObjectAttributes = new List<DbIntermediateForeignObject>() { };

        /// <summary>
        /// Marks variable as database-table
        /// </summary>
        /// <param name="tableName">Name of database-table (case-sensitivity is determined from database-table-settings) ['null' if the same as class-name]</param>
        public DbObject(string tableName = null)
        {
            this._tableName = tableName;    // Todo: Automatic resolving of name if it is null (?)
        }

        public void Init(Type classType)
        {
            this.parentClassType = classType;
            parentCInfo = classType.GetConstructor(Type.EmptyTypes);

            this._tableName = this._tableName ?? classType.Name;    // If no alternative table-name is specified, use the class-name

            // Iterate thru all fields
            foreach (System.Reflection.FieldInfo fi in classType.GetRuntimeFields())
            {
                try
                {
                    // Check if current field is a db-field and initiate it
                    if (fi.GetCustomAttribute(typeof(DbPrimaryKey), true) is DbPrimaryKey pkey)     // PrimaryKey
                    {
                        pkey.Init(fi, this);

                        this.baseAttributes.Add(pkey);
                        this.primaryKeyAttributes.Add(pkey);
                    }
                    else if (fi.GetCustomAttribute(typeof(DbAttribute), true) is DbAttribute att)   // Attributes
                    {
                        att.Init(fi, this);

                        this.baseAttributes.Add(att);
                        this.attributeAttributes.Add(att);
                    }
                    else if (fi.GetCustomAttribute(typeof(DbForeignKey), true) is DbForeignKey fkey)    // ForeignKeys
                    {
                        fkey.Init(fi, this);

                        this.baseAttributes.Add(fkey);
                        this.foreignKeyAttributes.Add(fkey);
                    }
                    else if (fi.GetCustomAttribute(typeof(DbForeignObject), true) is DbForeignObject fobj)    // ForeignObjects
                    {
                        if(fi.FieldType == this.parentClassType)    // Check if we are referencing ourselfes
                            fobj.Init(fi, this, this);
                        else
                            fobj.Init(fi, this);

                        this.foreignObjectAttributes.Add(fobj);
                    }
                    else if (fi.GetCustomAttribute(typeof(DbReverseForeignObject), true) is DbReverseForeignObject rfobj)    // ReverseForeignObjects
                    {
                        rfobj.Init(fi, this);
                        this.reverseForeignObjectAttributes.Add(rfobj);
                    }
                    else if (fi.GetCustomAttribute(typeof(DbIntermediateForeignObject), true) is DbIntermediateForeignObject iobj)    // ReverseForeignObjects
                    {
                        iobj.Init(fi, this);
                        this.intermediateObjectAttributes.Add(iobj);
                    }
                }
                catch(InvalidOperationException ex)
                {
                    throw new InvalidOperationException($"Cannot init foreignObject-field='{fi.Name}' of class='{classType.Name}'.", ex);
                }
            }
        }
    }
}
