using IBM.Data.DB2;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace SimpleDataAccess
{
    public enum DbProvider
    {
        MSSqlServer, Oracle, IBMDB2, OleDb, Odbc
    }

    internal class DbFactory
    {
        private DbProvider _DbProvider;

        public DbFactory(DbProvider provider)
        {
            _DbProvider = provider;
        }

        public DbProviderFactory CreateFactory()
        {
            DbProviderFactory dbFactory = null;

            switch (_DbProvider)
            {
                case DbProvider.MSSqlServer:
                    dbFactory = SqlClientFactory.Instance;
                    break;
                case DbProvider.OleDb:
                    dbFactory = OleDbFactory.Instance;
                    break;
                case DbProvider.Oracle:
                    dbFactory = OracleClientFactory.Instance;
                    break;
                case DbProvider.Odbc:
                    dbFactory = OdbcFactory.Instance;
                    break;
                case DbProvider.IBMDB2:
                    dbFactory = DB2Factory.Instance;
                    break;
            }

            return dbFactory;
        }

        public DbConnection CreateConnection()
        {
            switch (_DbProvider)
            {
                case DbProvider.MSSqlServer:
                    return new SqlConnection();
                case DbProvider.OleDb:
                    return new OleDbConnection();
                case DbProvider.Odbc:
                    return new OdbcConnection();
                case DbProvider.Oracle:
                    return new OracleConnection();
                case DbProvider.IBMDB2:
                    return new DB2Connection();
                default:
                    return null;
            }
        }

        public DbConnection CreateConnection(string connectionString)
        {
            switch (_DbProvider)
            {
                case DbProvider.MSSqlServer:
                    return new SqlConnection(connectionString);
                case DbProvider.OleDb:
                    return new OleDbConnection(connectionString);
                case DbProvider.Odbc:
                    return new OdbcConnection(connectionString);
                case DbProvider.Oracle:
                    return new OracleConnection(connectionString);
                case DbProvider.IBMDB2:
                    return new DB2Connection(connectionString);
                default:
                    return null;
            }
        }

        public DbCommand CreateCommand()
        {
            switch (_DbProvider)
            {
                case DbProvider.MSSqlServer:
                    return new SqlCommand();
                case DbProvider.OleDb:
                    return new OleDbCommand();
                case DbProvider.Odbc:
                    return new OdbcCommand();
                case DbProvider.Oracle:
                    return new OracleCommand();
                case DbProvider.IBMDB2:
                    return new DB2Command();
                default:
                    return null;
            }
        }

        public DbDataAdapter CreateAdpater()
        {
            switch (_DbProvider)
            {
                case DbProvider.MSSqlServer:
                    return new SqlDataAdapter();
                case DbProvider.OleDb:
                    return new OleDbDataAdapter();
                case DbProvider.Odbc:
                    return new OdbcDataAdapter();
                case DbProvider.Oracle:
                    return new OracleDataAdapter();
                case DbProvider.IBMDB2:
                    return new DB2DataAdapter();
                default:
                    return null;
            }
        }

        public DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            switch (_DbProvider)
            {
                case DbProvider.MSSqlServer:
                    return new SqlConnectionStringBuilder();
                case DbProvider.OleDb:
                    return new OleDbConnectionStringBuilder();
                case DbProvider.Odbc:
                    return new OdbcConnectionStringBuilder();
                case DbProvider.Oracle:
                    return new OracleConnectionStringBuilder();
                case DbProvider.IBMDB2:
                    return new DB2ConnectionStringBuilder();
                default:
                    return null;
            }
        }
    }
}