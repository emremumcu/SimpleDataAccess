using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccess
{
    /// <summary>
    /// Creates a Model class for SimpleDataAccess framework
    /// </summary>
    public class SDAModelCreator
    {
        private class Table
        {
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string TABLE_TYPE { get; set; }
        }

        private class Column
        {
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string TABLE_TYPE { get; set; }
            public string COLUMN_NAME { get; set; }
            public int IS_IDENTITY { get; set; }
            public int IS_COMPUTED { get; set; }
            public int ORDINAL_POSITION { get; set; }
            public string COLUMN_DEFAULT { get; set; }
            public string IS_NULLABLE { get; set; }
            public string DATA_TYPE { get; set; }
            public int CHARACTER_MAXIMUM_LENGTH { get; set; }
            public int CHARACTER_OCTET_LENGTH { get; set; }
            public byte NUMERIC_PRECISION { get; set; }
            public Int16 NUMERIC_PRECISION_RADIX { get; set; }
            public Int32 NUMERIC_SCALE { get; set; }
            public Int16 DATETIME_PRECISION { get; set; }
            public string CHARACTER_SET_CATALOG { get; set; }
            public string CHARACTER_SET_SCHEMA { get; set; }
            public string CHARACTER_SET_NAME { get; set; }
            public string COLLATION_CATALOG { get; set; }
            public bool IS_SPARSE { get; set; }
            public bool IS_COLUMN_SET { get; set; }
            public bool IS_FILESTREAM { get; set; }
        }

        private DbManager _dbManager;

        public SDAModelCreator(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        private DbDataReader CreateReader(string SQL, List<DbParameter> prms = null)
        {
            using (DbCommand dbCommand = _dbManager.CreateConnection().CreateCommand())
            {
                dbCommand.CommandText = SQL;

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

        private List<Table> GetTableList(string SchemaName)
        {
            //using (DbConnection conn = _dbManager.CreateConnection())
            //{
            //    conn.Open();

            //    //// table_catalog - table_schema - table_name - table_type
            //    string[] tableRestrictions = new string[4] { null, null, null, "BASE TABLE" };
            //    DataTable tableList = conn.GetSchema("Tables", tableRestrictions);

            //    conn.Close();

            //    return tableList;
            //}

            string SQL = @"
                SELECT 
                    [TABLE_CATALOG],
                    [TABLE_SCHEMA],
                    [TABLE_NAME],
                    [TABLE_TYPE] 
                FROM 
                    [INFORMATION_SCHEMA].[TABLES] 
                WHERE 1=1
                    AND [TABLE_SCHEMA]=@TABLE_SCHEMA
                ORDER BY 
                    TABLE_NAME";

            DbParameter p = _dbManager.GetDbFactory.CreateParameter();
            p.ParameterName = "@TABLE_SCHEMA";
            p.Value = SchemaName;

            List<DbParameter> prms = new List<DbParameter> { p };

            DbDataReader reader =  CreateReader(SQL, prms);

            List<Table> tableList = new GenericDataBinder<Table>().CreateList(reader);

            return tableList;
        }

        private List<Column> GetColumnList(string TableName, string SchemaName)
        {
            //using (DbConnection conn = _dbManager.CreateConnection())
            //{
            //    conn.Open();

            //    // table_catalog table_schema table_name table_type
            //    string[] columnRestrictions = new string[4] { null, SchemaName, TableName, null };
            //    DataTable columnList = conn.GetSchema("Columns", columnRestrictions);

            //    conn.Close();

            //    return columnList;
            //}

            string SQL = @"
                SELECT 
                    T.TABLE_CATALOG, 
                    T.TABLE_SCHEMA, 
                    T.TABLE_NAME, 
                    T.TABLE_TYPE, 
                    C2.COLUMN_NAME	, 
                    COLUMNPROPERTY(object_id(C2.TABLE_NAME), C2.COLUMN_NAME, 'IsIdentity') IS_IDENTITY,
                    COLUMNPROPERTY(object_id(C2.TABLE_NAME), C2.COLUMN_NAME, 'IsComputed') IS_COMPUTED,
                    C2.ORDINAL_POSITION	, 
                    C2.COLUMN_DEFAULT	, 
                    C2.IS_NULLABLE	, 
                    C2.DATA_TYPE	, 
                    C2.CHARACTER_MAXIMUM_LENGTH	, 
                    C2.CHARACTER_OCTET_LENGTH	, 
                    C2.NUMERIC_PRECISION	, 
                    C2.NUMERIC_PRECISION_RADIX	, 
                    C2.NUMERIC_SCALE	, 
                    C2.DATETIME_PRECISION	, 
                    C2.CHARACTER_SET_CATALOG	, 
                    C2.CHARACTER_SET_SCHEMA	, 
                    C2.CHARACTER_SET_NAME	, 
                    C2.COLLATION_CATALOG	,
                    C1.is_sparse IS_SPARSE,
                    C1.is_column_set IS_COLUMN_SET,
                    C1.is_filestream IS_FILESTREAM
                FROM 
                    INFORMATION_SCHEMA.TABLES T
                    INNER JOIN sys.columns C1 ON C1.object_id=object_id(T.TABLE_CATALOG + '.' + T.TABLE_SCHEMA + '.' + T.TABLE_NAME) 
                    INNER JOIN INFORMATION_SCHEMA.COLUMNS C2 ON C2.TABLE_CATALOG=T.TABLE_CATALOG AND C2.TABLE_SCHEMA=T.TABLE_SCHEMA AND C2.TABLE_NAME=T.TABLE_NAME
                    AND C1.column_id = C2.ORDINAL_POSITION
                WHERE 1=1
                    and T.TABLE_SCHEMA=@TABLE_SCHEMA
                    and T.TABLE_NAME=@TABLE_NAME
                    order by C2.ORDINAL_POSITION";

            DbParameter p1 = _dbManager.GetDbFactory.CreateParameter();
            p1.ParameterName = "@TABLE_SCHEMA";
            p1.Value = SchemaName;

            DbParameter p2 = _dbManager.GetDbFactory.CreateParameter();
            p2.ParameterName = "@TABLE_NAME";
            p2.Value = TableName;

            List<DbParameter> prms = new List<DbParameter> { p1, p2 };

            DbDataReader reader = CreateReader(SQL, prms);

            List<Column> tableList = new GenericDataBinder<Column>().CreateList(reader);

            return tableList;
        }
               
        private string AttributeMapper(Column col)
        {
            List<string> attribs = new List<string>();

            if (col.IS_NULLABLE.Trim() == "NO") attribs.Add(nameof(Attributes.NotNull));
            if (col.IS_IDENTITY == 1) attribs.Add(nameof(Attributes.Identity));
            if (col.IS_COMPUTED == 1) attribs.Add(nameof(Attributes.Computed));

            StringBuilder sb = new StringBuilder();

            if (attribs.Count > 0)
            {
                string attr = string.Join(", ", attribs.ToArray());
                return $"\t[{attr}]{Environment.NewLine}";
            }
            else
                return string.Empty;
        }

        private List<Column> colList;

        public string CreateModel(string TableName, string SchemaName = "dbo")
        {
            using (DbConnection conn = _dbManager.CreateConnection())
            {
                colList = GetColumnList(TableName, SchemaName);

                StringBuilder @class = new StringBuilder();

                @class.Append("using System;");
                @class.Append(Environment.NewLine);
                @class.Append("using SimpleDataAccess.Attributes;");
                @class.Append(Environment.NewLine);
                @class.Append("using System.Data.SqlTypes;");
                @class.Append(Environment.NewLine);
                @class.Append(Environment.NewLine);

                @class.Append($"[Table(SchemaName = \"{SchemaName}\", TableName = \"{TableName}\")]");
                @class.Append(Environment.NewLine);
                @class.Append($"public class {TableName} {{{Environment.NewLine}");

                foreach (Column c in colList)
                {
                    @class.Append(AttributeMapper(c));

                    @class.Append("\t");
                    @class.Append("public ");
                    @class.Append($"{ ConvertSqlServerFormatToCSharp(c.DATA_TYPE, c.IS_NULLABLE.Trim()=="YES") } ");
                    @class.Append($"{ c.COLUMN_NAME } ");
                    @class.Append("{ get; set; }");
                    @class.Append(Environment.NewLine);
                }

                @class.Append($"}}");

                return @class.ToString();
            }
        }

        private readonly string[] SqlServerTypes = { "bigint", "binary", "bit", "char", "date", "datetime", "datetime2", "datetimeoffset", "decimal", "filestream", "float", "geography", "geometry", "hierarchyid", "image", "int", "money", "nchar", "ntext", "numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar", "xml" };
        private readonly string[] CSharpTypes = { "long", "byte[]", "bool", "char", "DateTime", "DateTime", "DateTime", "DateTimeOffset", "decimal", "byte[]", "double", "Microsoft.SqlServer.Types.SqlGeography", "Microsoft.SqlServer.Types.SqlGeometry", "Microsoft.SqlServer.Types.SqlHierarchyId", "byte[]", "int", "decimal", "string", "string", "decimal", "string", "Single", "byte[]", "DateTime", "short", "decimal", "object", "string", "TimeSpan", "byte[]", "byte", "Guid", "bite[]", "string", "string" };

        private string ConvertSqlServerFormatToCSharp(string typeName, bool nullable)
        {
            var index = Array.IndexOf(SqlServerTypes, typeName);            

            string foundName = index > -1 ? CSharpTypes[index] : "object";

            if (nullable && (foundName != "string" && foundName != "object")) foundName += "?";

            return foundName;
        }

        private string ConvertCSharpFormatToSqlServer(string typeName)
        {
            var index = Array.IndexOf(CSharpTypes, typeName);

            return index > -1
                ? SqlServerTypes[index]
                : null;
        }







    }
}
