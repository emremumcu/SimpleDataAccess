using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccess.SDAModels
{
    public class DbColumn
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
}
