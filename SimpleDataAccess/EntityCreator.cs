using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleDataAccess
{
    public class EntityCreator
    {
        private DbManager _dbManager;

        public EntityCreator(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public string Create(string SchemaName, string TableName)
        {
            using (DbConnection conn = _dbManager.CreateConnection())
            {
                conn.Open();

                                                           // table_catalog table_schema table_name table_type
                string[] tableRestrictions = new string[4] { null, null, null, "BASE TABLE" }; 
                DataTable tableList = conn.GetSchema("Tables", tableRestrictions);

                                                           // table_catalog table_schema table_name table_type
                string[] columnRestrictions = new string[4] { null, null, TableName, null };
                DataTable columnList = conn.GetSchema("Columns", columnRestrictions);

                conn.Close();

                List<string> idColList = IdColumnNames(SchemaName, TableName);

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

                foreach (DataRow dr in columnList.Rows)
                {
                    bool allowNull = (dr["IS_NULLABLE"].ToString() == "YES");
                    bool isIdColumn = (idColList.Contains(dr["COLUMN_NAME"].ToString()));

                    if (!allowNull)
                        @class.Append("\t").Append("[NotNull]").Append(Environment.NewLine);
                    if(isIdColumn)
                        @class.Append("\t").Append("[Identity]").Append(Environment.NewLine);

                    @class.Append("\t");
                    @class.Append("public ");
                    @class.Append($"{ SQLTypeConverter(dr["DATA_TYPE"].ToString()) } ");
                    @class.Append($"{ dr["COLUMN_NAME"] } ");
                    @class.Append("{ get; set; }");
                    @class.Append(Environment.NewLine);
                }

                @class.Append($"}}");

                return @class.ToString();
            }
        }

        Dictionary<string, string> sqlTypeDict;

        private void ParseTypeDoc()
        {
            sqlTypeDict = new Dictionary<string, string>();

            XmlDocument sqlTypeDoc = new XmlDocument();

            sqlTypeDoc.Load("types.config");

            XmlNode a = sqlTypeDoc.SelectSingleNode("/configuration/sqlTypeSettings");

            foreach (XmlNode c in a.ChildNodes)
            {
                if (c.NodeType != XmlNodeType.Comment)
                {
                    sqlTypeDict.Add(c.Attributes["sqlType"].Value, c.Attributes["netType"].Value);
                }

            }
        }

        private string SQLTypeConverter(string Name)
        {
            if(sqlTypeDict == null) ParseTypeDoc();

            if (sqlTypeDict.ContainsKey(Name)) return sqlTypeDict[Name];
            else return "?";
        }

        private List<string> IdColumnNames(string SchemaName, string TableName)
        {
            string sql = @"SELECT ColumnName FROM (
                        SELECT 
	                        OBJECT_SCHEMA_NAME(tables.object_id, db_id())
	                        AS SchemaName,
	                        tables.name As TableName,
	                        columns.name as ColumnName
                        FROM sys.tables tables 
	                        JOIN sys.columns columns 
                        ON tables.object_id=columns.object_id
                        WHERE columns.is_identity=1
                        ) AS IDCOLS where IDCOLS.SchemaName=@SchemaName and IDCOLS.TableName=@TableName
            ";

            List<DbParameter> prms = new List<DbParameter>();

            prms.Add(new SqlParameter("@SchemaName", SchemaName));
            prms.Add(new SqlParameter("@TableName", TableName));

            DataTable dt = _dbManager.Select(sql);

            return dt.AsEnumerable().Select(x => x.Field<string>("ColumnName")).ToList();
        }

    }
}
