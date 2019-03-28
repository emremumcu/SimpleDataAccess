
using SimpleDataAccess.DbAttributes;

namespace SimpleDataAccess.SDAModels
{
    [Table(SchemaName = "INFORMATION_SCHEMA", TableName = "TABLES")]
    public class DbTable
    {
        public string TABLE_CATALOG { get; set; }

        public string TABLE_SCHEMA { get; set; }

        public string TABLE_NAME { get; set; }

        public string TABLE_TYPE { get; set; }
    }
}
