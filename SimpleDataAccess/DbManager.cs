using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

using SimpleDataAccess.DbAttributes;
using System.Collections.Concurrent;

namespace SimpleDataAccess
{
    public partial class DbManager
    {
        private DbFactory dbFactory;

        internal DbFactory GetDbFactory { get { return dbFactory; } }

        private DbConnectionStringBuilder _dbConnectionStringBuilder;

        public DbManager(DbProvider provider, string connectionString)
        {
            try
            {
                dbFactory = new DbFactory(provider);
                _dbConnectionStringBuilder = dbFactory.CreateConnectionStringBuilder();
                _dbConnectionStringBuilder.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DbConnection CreateConnection()
        {
            return dbFactory.CreateConnection(_dbConnectionStringBuilder.ConnectionString);
        }

        #region ExecuteNonQuery

        public int ExecuteNonQuery(string SQL, List<DbParameter> prms = null)
        {
            using (DbCommand dbCommand = dbFactory.CreateCommand())
            {
                dbCommand.CommandText = SQL;
                dbCommand.Connection = dbFactory.CreateConnection(_dbConnectionStringBuilder.ConnectionString);

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
                dbCommand.Connection = dbFactory.CreateConnection(_dbConnectionStringBuilder.ConnectionString); 

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
                dbDataAdapter.SelectCommand.Connection = dbFactory.CreateConnection(_dbConnectionStringBuilder.ConnectionString); 

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

        public enum SortOrder
        {
            Ascending, Descending, Default
        }

        private DbDataReader CreateReader(string SQL, List<DbParameter> prms = null)
        {
            using (DbCommand dbCommand = dbFactory.CreateCommand())
            {
                dbCommand.CommandText = SQL;
                dbCommand.Connection = dbFactory.CreateConnection(_dbConnectionStringBuilder.ConnectionString);

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

        private string TableName<T>()
        {
            TableAttribute TableAttribute = typeof(T).GetCustomAttribute<TableAttribute>(false);

            string tableFullName = string.Empty;

            if(TableAttribute != null)
            {
                // if TableAttribute is specified, use it for fetching table name
                string schema = (TableAttribute.SchemaName) != null ? ($"{TableAttribute.SchemaName}."):(null);
                string table = TableAttribute.TableName;

                tableFullName = $"{schema}{table}";
            }
            else
            {
                // if TableAttribute is not specified, use class name for fetching table name
                tableFullName = typeof(T).Name;
            }

            return tableFullName;
        }

        public List<T> SelectAll<T>(Expression<Func<T, object>> OrderBy = null, SortOrder sort = SortOrder.Default) where T : class, new()
        {
            List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

            string SQL = $"SELECT { String.Join(",", properties.Select(p => p.Name).ToArray()) } FROM { TableName<T>() }";

            if(OrderBy != null)
            {
                string orderBy = new QueryTranslator().Translate(OrderBy);
                string sortOrder = (sort == SortOrder.Ascending) ? ("ASC") : (sort == SortOrder.Descending) ? ("DESC"): ("");

                SQL += $" ORDER BY {orderBy} {sortOrder}";
            }

            DbDataReader reader = CreateReader(SQL);

            List<T> list = new GenericDataBinder<T>().CreateList(reader);

            return list;
        }

        public List<T> Select<T>(Expression<Func<T, object>> filter, Expression<Func<T, object>> OrderBy = null, SortOrder sort = SortOrder.Default) where T : class, new()
        {
            List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

            string sqlFilter = new QueryTranslator().Translate(filter);

            string SQL = $"SELECT { String.Join(",", properties.Select(p => p.Name).ToArray()) } FROM { TableName<T>() }  WHERE {sqlFilter}";

            if (OrderBy != null)
            {
                string orderBy = new QueryTranslator().Translate(OrderBy);
                string sortOrder = (sort == SortOrder.Ascending) ? ("ASC") : (sort == SortOrder.Descending) ? ("DESC") : ("");

                SQL += $" ORDER BY {orderBy} {sortOrder}";
            }

            DbDataReader reader = CreateReader(SQL);

            List<T> list = new GenericDataBinder<T>().CreateList(reader);

            return list;

        }
        
        #endregion EntityBinders


        #region Insert


        public void Insert<T>(T obj) where T : class, new()
        {
            string tableName = TableName<T>();

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

            IEnumerable<PropertyInfo> excludedProperties = from prop in properties
                    from attr in prop.CustomAttributes
                    where attr.AttributeType == typeof(Identity) || attr.AttributeType == typeof(Computed)
                    select prop;

            List<PropertyInfo> insertableProperties = properties.Except(excludedProperties.ToList()).ToList();

            List < DbParameter > prms = new List<DbParameter>();

            foreach (PropertyInfo pi in insertableProperties)
            {
                DbParameter p = dbFactory.CreateParameter();
                p.ParameterName = $"@{pi.Name}";
                p.Value = pi.GetValue(obj) ?? DBNull.Value;

                prms.Add(p);
            }

            string SQLINSERT = $@"INSERT INTO {tableName} ({String.Join(", ", insertableProperties.Select(p => p.Name).ToArray())}) VALUES (@{String.Join(", @", insertableProperties.Select(p => p.Name).ToArray())})";

            ExecuteNonQuery(SQLINSERT, prms);
        }

        #endregion Insert
    }
}
