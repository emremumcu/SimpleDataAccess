
using SimpleDataAccess.DbAttributes;

namespace SimpleDataAccess.SDAModels
{
    [Table(SchemaName = "sys", TableName = "databases")]
    public class DbCatalog
    {
        public int database_id { get; set; }

        public string name { get; set; }

        public string collation_name { get; set; }
    }
}
