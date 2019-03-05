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
        private static void Parser(PropertyInfo pi, object entity, object value)
        {
            if (pi.PropertyType == typeof(string))
            {
                pi.SetValue(entity, value.ToString().Trim(), null);
            }
            else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(int?))
            {
                if (value == null)
                {
                    pi.SetValue(entity, null, null);
                }
                else
                {
                    pi.SetValue(entity, int.Parse(value.ToString()), null);
                }
            }
        }

        public T Map(DataRow row)
        {
            List<string> columnNames = row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            List<PropertyInfo> properties = (typeof(T)).GetProperties().Where(x => x.GetCustomAttributes(typeof(Column), true).Any()).ToList();

            T entity = new T();

            foreach (PropertyInfo pi in properties)
            {           
                Parser(pi, entity, row[pi.Name]);
            }

            return entity;
        }

        public IEnumerable<T> Map(DataTable table)
        {
            var columnNames = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

            var properties = (typeof(T)).GetProperties().Where(x => x.GetCustomAttributes(typeof(Column), true).Any()).ToList();

            List<T> entities = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T entity = new T();

                foreach (PropertyInfo pi in properties)
                {
                    Parser(pi, entity, row[pi.Name]);
                }

                entities.Add(entity);
            }

            return entities;
        }
    }
}
