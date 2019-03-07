using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq.Expressions;
using SimpleDataAccess.Attributes;
using System.Reflection;
using System.Linq;

namespace SimpleDataAccess
{
    public partial class DbManager
    {
        private DbFactory dbFactory;
        private DbConnectionStringBuilder dbConnStrBuilder;

        public DbManager(DbProvider provider, string connectionString)
        {
            try
            {
                dbFactory = new DbFactory(provider);
                dbConnStrBuilder = dbFactory.CreateConnectionStringBuilder();
                dbConnStrBuilder.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region ExecuteNonQuery

        public int ExecuteNonQuery(string SQL, List<DbParameter> prms = null)
        {
            using (DbCommand dbCommand = dbFactory.CreateCommand())
            {
                dbCommand.CommandText = SQL;
                dbCommand.Connection = dbFactory.CreateConnection(dbConnStrBuilder.ConnectionString);

                if (prms != null)
                {
                    dbCommand.Parameters.Clear();
                    dbCommand.Parameters.AddRange(prms.ToArray());
                }

                dbCommand.Connection.Open();
                int numRows = dbCommand.ExecuteNonQuery();
                dbCommand.Connection.Close();

                return numRows;
            }
        }

        public int ExecuteNonQuery(string SQL, DbParameter prm)
        {
            return ExecuteNonQuery(
                SQL,
                new List<DbParameter>() {
                    prm
                }
            );
        }

        #endregion ExecuteNonQuery
        

        #region ExecuteScalar

        public object ExecuteScalar(string SQL, List<DbParameter> prms = null)
        {
            using (DbCommand dbCommand = dbFactory.CreateCommand())
            {
                dbCommand.CommandText = SQL;
                dbCommand.Connection = dbFactory.CreateConnection(dbConnStrBuilder.ConnectionString); 

                if (prms != null)
                {
                    dbCommand.Parameters.Clear();
                    dbCommand.Parameters.AddRange(prms.ToArray());
                }

                dbCommand.Connection.Open();
                object result = dbCommand.ExecuteScalar();
                dbCommand.Connection.Close();

                return result;
            }
        }

        public object ExecuteScalar(string SQL, DbParameter prm)
        {
            return ExecuteScalar(
                SQL,
                new List<DbParameter>() {
                    prm
                }
            );
        }

        #endregion ExecuteScalar


        #region Select

        public DataTable Select(String SQL, List<DbParameter> prms = null)
        {
            using (DbDataAdapter dbDataAdapter = dbFactory.CreateAdpater())
            {                
                dbDataAdapter.SelectCommand = dbFactory.CreateCommand();
                dbDataAdapter.SelectCommand.CommandText = SQL;
                dbDataAdapter.SelectCommand.Connection = dbFactory.CreateConnection(dbConnStrBuilder.ConnectionString); 

                if (prms != null)
                {
                    dbDataAdapter.SelectCommand.Parameters.Clear();
                    dbDataAdapter.SelectCommand.Parameters.AddRange(prms.ToArray());
                }

                DataTable returnTable = new DataTable();

                returnTable.BeginLoadData();
                dbDataAdapter.Fill(returnTable);
                returnTable.EndLoadData();

                return returnTable;
            }
        }

        public DataTable Select(String SQL, DbParameter prm)
        {
            return Select(SQL,
                new List<DbParameter>() {
                    prm
                }
            );
        }

        #endregion Select


        #region EntityBinders

        public T SelectSingle<T>(Expression<Func<T, object>> filter) where T : class, new()
        {
            Table TableAttribute = typeof(T).GetCustomAttribute<Table>(false);

            if (TableAttribute == null)
            {
                throw new CustomAttributeFormatException($"{ typeof(Table).FullName } attribute is required for { typeof(T).FullName }");
            }
            else
            {
                string TableName = TableAttribute.Name;

                List<string> columnList = new List<string>();

                PropertyInfo[] props = typeof(T).GetProperties();

                foreach (PropertyInfo pi in props)
                {
                    Column col = pi.GetCustomAttribute<Column>(false);
                    if (col != null) columnList.Add(col.Name);
                }

                string sqlFilter = new QueryTranslator().Translate(filter);

                string SQL = $"SELECT { String.Join(",", columnList.ToArray()) } FROM {TableName} WHERE {sqlFilter}";

                System.Data.DataTable dt = Select(SQL);

                T entitiy = new DataMapper<T>().Map(dt.Rows[0]);

                return entitiy;
            }

        }

        public List<T> Select<T>(Expression<Func<T, object>> filter) where T : class, new()
        {
            Table TableAttribute = typeof(T).GetCustomAttribute<Table>(false);

            if (TableAttribute == null)
            {
                throw new CustomAttributeFormatException($"{ typeof(Table).FullName } attribute is required for { typeof(T).FullName }");
            }
            else
            {
                string TableName = TableAttribute.Name;

                List<string> columnList = new List<string>();

                PropertyInfo[] props = typeof(T).GetProperties();

                foreach (PropertyInfo pi in props)
                {
                    Column col = pi.GetCustomAttribute<Column>(false);
                    if (col != null) columnList.Add(col.Name);
                }

                string sqlFilter = new QueryTranslator().Translate(filter);

                string SQL = $"SELECT { String.Join(",", columnList.ToArray()) } FROM {TableName} WHERE {sqlFilter}";

                System.Data.DataTable dt = Select(SQL);

                List<T> list = new List<T>();

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new DataMapper<T>().Map(dr));
                }

                return list;
            }

        }

        public List<T> SelectAll<T>() where T : class, new()
        {
            Table TableAttribute = typeof(T).GetCustomAttribute<Table>(false);

            if (TableAttribute == null)
            {
                throw new CustomAttributeFormatException($"{ typeof(Table).FullName } attribute is required for { typeof(T).FullName }");
            }
            else
            {
                string TableName = TableAttribute.Name;

                List<string> columnList = new List<string>();

                PropertyInfo[] props = typeof(T).GetProperties();

                foreach (PropertyInfo pi in props)
                {
                    Column col = pi.GetCustomAttribute<Column>(false);
                    if (col != null) columnList.Add(col.Name);
                }

                string SQL = $"SELECT { String.Join(",", columnList.ToArray()) } FROM {TableName}";

                System.Data.DataTable dt = Select(SQL);

                List<T> list = new List<T>();

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new DataMapper<T>().Map(dr));
                }

                return list;
            }

        }


        private DbDataReader CreateReader(string SQL, List<DbParameter> prms = null)
        {
            using (DbCommand dbCommand = dbFactory.CreateCommand())
            {
                dbCommand.CommandText = SQL;
                dbCommand.Connection = dbFactory.CreateConnection(dbConnStrBuilder.ConnectionString);

                if (prms != null)
                {
                    dbCommand.Parameters.Clear();
                    dbCommand.Parameters.AddRange(prms.ToArray());
                }

                dbCommand.Connection.Open();
                DbDataReader reader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);

                return reader;
            }
        }

        public List<T> SelectAll2<T>() where T : class, new()
        {
            string TableName = typeof(T).Name;

            List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

            string SQL = $"SELECT { String.Join(",", properties.Select(p => p.Name).ToArray()) } FROM Person.{TableName}";

            DbDataReader reader = CreateReader(SQL);

            List<T> list = new GenericDataBinder<T>().CreateList(reader);

            return list;

        }

        public List<T> Select2<T>(Expression<Func<T, object>> filter) where T : class, new()
        {
            string TableName = typeof(T).Name;

            List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

            string sqlFilter = new QueryTranslator().Translate(filter);

            string SQL = $"SELECT { String.Join(",", properties.Select(p => p.Name).ToArray()) } FROM Person.{TableName}  WHERE {sqlFilter}";

            DbDataReader reader = CreateReader(SQL);

            List<T> list = new GenericDataBinder<T>().CreateList(reader);

            return list;

        }

        public List<T> SelectSingle2<T>(Expression<Func<T, object>> filter) where T : class, new()
        {
            string TableName = typeof(T).Name;

            List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

            string sqlFilter = new QueryTranslator().Translate(filter);

            string SQL = $"SELECT { String.Join(",", properties.Select(p => p.Name).ToArray()) } FROM Person.{TableName}  WHERE {sqlFilter}";

            DbDataReader reader = CreateReader(SQL);

            List<T> list = new GenericDataBinder<T>().CreateList(reader);

            return list;

        }
        #endregion EntityBinders

    }
}
