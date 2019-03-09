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
                    if (!allowNull)
                        @class.Append("\t").Append("[NotNull]").Append(Environment.NewLine);

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

        public void ParseTypeDoc()
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

        public string SQLTypeConverter(string Name)
        {
            if(sqlTypeDict == null) ParseTypeDoc();

            if (sqlTypeDict.ContainsKey(Name)) return sqlTypeDict[Name];
            else return "?";
        }
    }
}
