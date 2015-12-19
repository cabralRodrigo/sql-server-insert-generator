using CabralRodrigo.Util.SqlServerInsertGenerator.Metadata;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Generator
{
    internal class SqlInsertCommandMetadata
    {
        public SqlInsertCommandMetadata(SqlServerTableMetadata metadata)
        {
            this.ProcessMetadata(metadata);
        }

        public string TableName { get; private set; }

        public List<SqlInsertCommandColumnMetadata> Columns { get; private set; }

        private void ProcessMetadata(SqlServerTableMetadata metadata)
        {
            this.TableName = metadata.TableName;
            this.Columns = SqlInsertCommandColumnMetadata.Build(metadata);
        }
    }

    internal class SqlInsertCommandColumnMetadata
    {
        private SqlInsertCommandColumnMetadata() { }

        public static List<SqlInsertCommandColumnMetadata> Build(SqlServerTableMetadata metadata)
        {
            var columns = metadata.Columns.Select(s => new SqlInsertCommandColumnMetadata
            {
                RawName = s.Name,
                Name = s.Name,
                DbType = s.DataType,
                DbTypeString = SqlInsertCommandColumnMetadata.ConvertSqlTypeForInsertCommand(s.DataType, s.Length),
                Nullable = s.IsNullable ? "null" : "not null",
                Value = s.IsNullable ? "null" : "''"
            }).ToList();

            var maxNameLength = columns.GetLengthOfLongestString(s => s.Name);
            var maxDbTypeLength = columns.GetLengthOfLongestString(s => s.DbTypeString);
            var maxNullableLength = columns.GetLengthOfLongestString(s => s.Nullable);

            foreach (var column in columns)
            {
                column.Name = column.Name.AppendUtilLength('-', maxNameLength);
                column.DbTypeString = column.DbTypeString.AppendUtilLength('-', maxDbTypeLength);
                column.Nullable = column.Nullable.AppendUtilLength('-', maxNullableLength);
            }

            return columns;
        }

        private static string ConvertSqlTypeForInsertCommand(SqlDbType dataType, int length)
        {
            if (length == -1)
                return dataType.ToString().ToLower();
            else
                return $"{dataType.ToString().ToLower()}({length})";
        }

        public string Name { get; set; }
        public string DbTypeString { get; set; }
        public string Nullable { get; set; }
        public string Value { get; set; }
        public SqlDbType DbType { get; private set; }
        public string RawName { get; private set; }
    }
}
