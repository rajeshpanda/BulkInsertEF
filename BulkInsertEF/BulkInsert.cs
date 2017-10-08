using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RajeshPanda.EF
{
    /// <summary>
    /// Container for all the EF bulk insert logic
    /// </summary>
    public class BulkInsert : IBulkInsert
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext">DbContext</param>
        public BulkInsert(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// To bulk insert records.
        /// </summary>
        /// <param name="values">IEnumerable(object) of the type of records</param>
        public void BulkInsertCommand(IEnumerable<object> values)
        {
            object obj = null;
            foreach (var one in values)
            {
                obj = one;
                break;
            }
            var insertHeader = CreatePropertyInsertInline(obj);
            var insertValues = CreateValuesInsertInline(values);
            for (var size = 0; size < insertValues.Count; size = size + 1000)
            {
                var parentValues = insertValues.Skip(size).Take(1000);
                var parent = string.Join(",", parentValues);
                var insertSQL = insertHeader + parent;
                
                try
                {
                    _dbContext.Database.ExecuteSqlCommand(insertSQL);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception :  " + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// To bulk insert records in case of TPT inheritance
        /// </summary>
        /// <param name="values">IEnumerable(object) of the type of records</param>
        /// <param name="parentName">Parent class name</param>
        /// <param name="foreignKeys">comma separated foreign keys if any</param>
        public void BulkInsertCommandForTPT(IEnumerable<object> values, string parentName, string foreignKeys)
        {
            object obj = null;
            foreach (var one in values)
            {
                obj = one;
                break;
            }
            var insertParentHeader = CreateTPTPropertyInsertInline(obj, parentName);
            var insertParentValues = CreateTPTValuesInsertInline(values, parentName);
            for (var size = 0; size < insertParentValues.Count; size = size + 1000)
            {
                var parentValues = insertParentValues.Skip(size).Take(1000);
                var parent = string.Join(",", parentValues);
                var insertSQL = insertParentHeader + parent;
                
                try {
                    _dbContext.Database.ExecuteSqlCommand(insertSQL);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception :  " + e.StackTrace);
                }
            }

            var insertChildHeader = CreateTPTPropertyInsertInline(obj, parentName, foreignKeys);
            var insertChildValues = CreateTPTValuesInsertInline(values, parentName, foreignKeys);
            for(var size = 0; size < insertChildValues.Count; size = size + 1000)
            {
                var childValues = insertChildValues.Skip(size).Take(1000);
                var children = string.Join(",", childValues);
                var insertSQL = insertChildHeader + children;
                
                try
                {
                    _dbContext.Database.ExecuteSqlCommand(insertSQL);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception :  " + e.StackTrace);
                }
            }
        }

        private string CreatePropertyInsertInline(object insertObject)
        {
            StringBuilder properties = new StringBuilder();
            PropertyInfo[] listOfProperties = insertObject.GetType().GetProperties(BindingFlags.Instance |
                                                        BindingFlags.DeclaredOnly |
                                                        BindingFlags.Public);
            var tableName = insertObject.GetType().Name;
            properties.Append("INSERT INTO " + tableName + " (");
            var firstProperty = true;
            foreach (var property in listOfProperties)
            {
                if (property.GetMethod.IsVirtual)
                    continue;
                if (!firstProperty)
                {
                    properties.Append(", ");
                }
                if (firstProperty)
                    firstProperty = false;
                properties.Append(property.Name);
            }

            properties.Append(") VALUES");

            return properties.ToString();
        }

        private List<string> CreateValuesInsertInline(IEnumerable<object> objects)
        {
            var listOfValues = new List<string>();
            if (objects != null)
            {
                foreach (var value in objects)
                {
                    StringBuilder values = new StringBuilder();
                    var properties = value.GetType().GetProperties();
                    var firstProperty = true;
                    values.Append("(");
                    foreach (var p in properties)
                    {
                        if (p.GetMethod.IsVirtual)
                            continue;
                        var val = p.GetValue(value, null);
                        if (!firstProperty)
                        {
                            values.Append(", ");
                        }

                        if (firstProperty)
                            firstProperty = false;

                        if(val != null)
                        {
                            values = ParseValues(values, p, val);
                        }

                    }
                    values.Append(")");
                    listOfValues.Add(values.ToString());
                }
            }
            return listOfValues;
        }

        private string CreateTPTPropertyInsertInline(object insertObject, string parentName, string foreignKeys = null)
        {
            StringBuilder properties = new StringBuilder();

            PropertyInfo[] listOfProperties = insertObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var tableName = parentName;

            if (!string.IsNullOrEmpty(foreignKeys))
            {
                tableName = insertObject.GetType().Name;
            }

            properties.Append("INSERT INTO " + tableName + " (");
            var firstProperty = true;
            foreach (var property in listOfProperties)
            {
                if (!string.IsNullOrEmpty(foreignKeys))
                {
                    if (property.DeclaringType.Name == parentName && !foreignKeys.Contains(property.Name))
                        continue;
                }
                else
                {
                    if (property.DeclaringType.Name != parentName)
                        continue;
                }
                if (property.GetMethod.IsVirtual)
                    continue;
                if (!firstProperty)
                {
                    properties.Append(", ");
                }
                if (firstProperty)
                    firstProperty = false;
                properties.Append(property.Name);
            }

            properties.Append(") VALUES");

            return properties.ToString();
        }

        private List<string> CreateTPTValuesInsertInline(IEnumerable<object> objects, string parentName, string foreignKeys = null)
        {
            var listOfValues = new List<string>();
            if (objects != null)
            {
                foreach (var value in objects)
                {
                    StringBuilder values = new StringBuilder();
                    var properties = value.GetType().GetProperties();
                    var firstProperty = true;
                    values.Append("(");
                    foreach (var p in properties)
                    {
                        if (!string.IsNullOrEmpty(foreignKeys))
                        {
                            if (p.DeclaringType.Name == parentName && !foreignKeys.Contains(p.Name))
                                continue;
                        }
                        else
                        {
                            if (p.DeclaringType.Name != parentName)
                                continue;
                        }

                        if (p.GetMethod.IsVirtual)
                            continue;
                        var val = p.GetValue(value, null);
                        if (!firstProperty)
                        {
                            values.Append(", ");
                        }

                        if (firstProperty)
                            firstProperty = false;

                        if(val != null)
                        {
                            values = ParseValues(values, p, val);
                        }

                    }
                    values.Append(")");
                    listOfValues.Add(values.ToString());
                }
            }
            return listOfValues;
        }

        private StringBuilder ParseValues(StringBuilder values, PropertyInfo p, object val)
        {
            if (p.PropertyType.Name.Equals("String") || p.PropertyType.Name.Equals("Guid"))
            {
                val = val?.ToString().Replace("'", "''");
                values.Append("'" + val?.ToString() + "'");
            }
            else if (p.PropertyType.GenericTypeArguments[0].Name.Equals("DateTime"))
            {
                var temporal = DateTime.Parse(val.ToString());
                values.Append("'" + temporal.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            }
            else if (p.PropertyType.GenericTypeArguments[0].Name.Equals("DateTimeOffset"))
            {
                var temporal = DateTimeOffset.Parse(val.ToString());
                values.Append("'" + temporal.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            }
            else if (p.PropertyType.GenericTypeArguments[0].Name.Equals("Boolean"))
            {
                if (bool.Parse(val.ToString()))
                    values.Append("1");
                else
                {
                    values.Append("0");
                }
            }
            else
            {
                values.Append(val ?? 0);
            }

            return values;
        }
    }
}
