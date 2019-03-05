using SimpleDataAccess.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccess
{
    public class DataMapper<T> where T : class, new()
    {
        private static void Parser(PropertyInfo prop, object entity, object value)
        {
            if (prop.PropertyType == typeof(string))
            {
                prop.SetValue(entity, value.ToString().Trim(), null);
            }
            else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
            {
                if (value == null)
                {
                    prop.SetValue(entity, null, null);
                }
                else
                {
                    prop.SetValue(entity, int.Parse(value.ToString()), null);
                }
            }
        }

        public T Map(DataRow row)
        {
            List<string> columnNames = row.Table.Columns
                                       .Cast<DataColumn>()
                                       .Select(c => c.ColumnName)
                                       .ToList();

            List<PropertyInfo> properties = (typeof(T)).GetProperties()
                                              .Where(x => x.GetCustomAttributes(typeof(Column), true).Any())
                                              .ToList();

            T entity = new T();

            foreach (var prop in properties)
            {           
                Parser(prop, entity, row[prop.Name]);
            }

            return entity;
        }

        public IEnumerable<T> Map(DataTable table)
        {
            var columnNames = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

            var properties = (typeof(T)).GetProperties()
                                                .Where(x => x.GetCustomAttributes(typeof(Column), true).Any())
                                                .ToList();


            List<T> entities = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T entity = new T();

                foreach (var prop in properties)
                {
                    Parser(prop, entity, row[prop.Name]);
                }

                entities.Add(entity);
            }

            return entities;
        }
    }
}
