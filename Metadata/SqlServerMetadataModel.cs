using System.Collections.Generic;
using System.Data;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Metadata
{
    public class SqlServerColumnMetadata
    {
        public SqlDbType DataType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public string Schema { get; set; }
    }

    public class SqlServerTableMetadata
    {
        public List<SqlServerColumnMetadata> Columns { get; set; }
        public string TableName { get; set; }
    }
}