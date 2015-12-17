using CabralRodrigo.Util.SqlServerInsertGenerator.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Generator
{
    internal class SqlInsertGenerator
    {
        /// <summary>
        /// Initializes a new instance of <seealso cref="SqlInsertGenerator"/>
        /// and configures the <see cref="SqlInsertGenerator"/> to generate the insert command with the table's schema only.
        /// </summary>
        /// <param name="database">The database which the data and the schema will be retrieved to generate the insert command.</param>
        /// <param name="table">The target table to generate de insert command.</param>
        public SqlInsertGenerator(string database, string table) : this(database, table, null) { }

        /// <summary>
        /// Initializes a new instance of <seealso cref="SqlInsertGenerator"/>
        /// and configures the <see cref="SqlInsertGenerator"/> to generate the insert command with the table's schema
        /// and with the date retrieved with the <paramref name="whereClause"/> parameter.
        /// </summary>
        /// <param name="database">The database which the data and the schema will be retrieved to generate the insert command.</param>
        /// <param name="table">The target table to generate de insert command.</param>
        /// <param name="whereClause">The where clause used to retrieve the data to fill the insert command.</param>
        public SqlInsertGenerator(string database, string table, string whereClause)
        {
            this.GenerateInsertWihData = !string.IsNullOrEmpty(whereClause);
            this.Database = database;
            this.Table = table;
            this.WhereClause = whereClause;
        }

        /// <summary>
        /// The database which the data and the schema will be retrieved to generate the insert command.
        /// </summary>
        private string Database { get; set; }

        /// <summary>
        /// Indicate when the insert command will be filled with actual data.
        /// </summary>
        private bool GenerateInsertWihData { get; set; }

        /// <summary>
        /// The target table to generate de insert command.
        /// </summary>
        private string Table { get; set; }

        /// <summary>
        /// The where clause used to retrieve the data to fill the insert command.
        /// </summary>
        private string WhereClause { get; set; }

        /// <summary>
        /// Builds the insert command.
        /// </summary>
        /// <returns>The insert command generated.</returns>
        public string Build()
        {
            //TODO: Use T4 Templates to generate the insert command

            var sb = new StringBuilder();
            var metadata = this.GetTableMetadata();
            var fillData = this.GetFillDataToInsertCommand();

            var insertFields = string.Join(", ", metadata.Columns.Select(s => s.Name).ToArray());
            var insertBody = this.BuildBodyInsert(metadata, fillData);

            sb.AppendLine($"insert into {this.Table} ({insertFields}) values ");
            sb.Append(insertBody);

            return sb.ToString();
        }

        /// <summary>
        /// Builds the body of the insert command.
        /// </summary>
        /// <param name="metadata">The metadata of the target table for generate the insert.</param>
        /// <param name="fillData">The data that will be used to fill the insert command.</param>
        /// <returns>The body of the insert command.</returns>
        private string BuildBodyInsert(SqlServerTableMetadata metadata, List<Dictionary<string, object>> fillData)
        {
            //TODO: Refactor this method to handle the metadata fields length in a better way
            var sb = new StringBuilder();

            var maxSizeName = metadata.Columns.GetLengthOfLongestString(s => s.Name);
            var maxSizeDataType = metadata.Columns.GetLengthOfLongestString(this.FormatColumnDataType);
            var maxSizeNullable = metadata.Columns.GetLengthOfLongestString(this.FormatColumnNullableField);

            var bodyParts = metadata.Columns.Select(column => new
            {
                Name = column.Name.AppendUtilLength('-', maxSizeName),
                RawName = column.Name,
                DbTypeString = this.FormatColumnDataType(column).AppendUtilLength('-', maxSizeDataType),
                Nullable = this.FormatColumnNullableField(column).AppendUtilLength('-', maxSizeNullable),
                DbType = column.DataType,
                DefaultValue = column.IsNullable ? "null" : "''"
            }).ToList();

            if (fillData.Count < 1)
                fillData.Add(null);

            foreach (var data in fillData)
            {
                sb.AppendLine("(");
                foreach (var part in bodyParts)
                {
                    string dataString = null;
                    if (data != null && data.ContainsKey(part.RawName))
                    {
                        var convertedData = this.ConvertObjectToSqlString(data[part.RawName], part.DbType);
                        if (this.DbTypeValueHasToBeQuoted(part.DbType, convertedData))
                            dataString = $"'{convertedData.Trim()}'";
                        else
                            dataString = convertedData;
                    }
                    else
                        dataString = part.DefaultValue;

                    sb.Append($"    /* {part.Name} | {part.DbTypeString} | {part.Nullable} */   {(data == null ? part.DefaultValue : dataString)}");

                    if (part != bodyParts.Last())
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }

                if (data == fillData.Last())
                    sb.AppendLine(")");
                else
                    sb.AppendLine("),");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a object to a sql string.
        /// </summary>
        /// <param name="objectValue">The object to be converted.</param>
        /// <param name="columnType">The column type of the object will be used.</param>
        /// <returns>The string representation of the object that can be used in a sql query.</returns>
        private string ConvertObjectToSqlString(object objectValue, SqlDbType columnType)
        {
            if (objectValue is DBNull)
                return "null";
            if (objectValue is string || objectValue is int)
                return objectValue.ToString();
            else if (objectValue is bool)
                return ((bool)objectValue) ? "1" : "0";
            else if (objectValue is Guid)
                return ((Guid)objectValue).ToString();
            else if (objectValue is DateTime)
                return ((DateTime)objectValue).ToString(columnType == SqlDbType.Date ? "yyy-MM-dd" : "yyyy-MM-dd HH:mm:ss.fff");
            else
                throw new Exception("Unknowm object type: " + objectValue.GetType());
        }

        /// <summary>
        /// Determines when the <seealso cref="SqlDbType"/> has to be quoted in a sql query.
        /// </summary>
        /// <param name="dbType">The <seealso cref="SqlDbType"/> to be checked.</param>
        /// <param name="objectValue">The value of the column.</param>
        /// <returns>True if the <seealso cref="SqlDbType"/> has to be quoted in a sql query, False if not.</returns>
        private bool DbTypeValueHasToBeQuoted(SqlDbType dbType, string objectValue)
        {
            return !new SqlDbType[]
            {
                SqlDbType.BigInt,
                SqlDbType.Bit,
                SqlDbType.Decimal,
                SqlDbType.Float,
                SqlDbType.Int,
                SqlDbType.Money,
                SqlDbType.SmallInt,
                SqlDbType.SmallMoney,
                SqlDbType.TinyInt
            }.Contains(dbType) && objectValue != "null";
        }

        /// <summary>
        /// Formats the property <seealso cref="SqlServerColumnMetadata.DataType"/>.
        /// </summary>
        /// <param name="columnMetadata">The column to format the <seealso cref="SqlServerColumnMetadata.DataType"/>.</param>
        /// <returns>String version of the property <seealso cref="SqlServerColumnMetadata.DataType"/></returns>
        private string FormatColumnDataType(SqlServerColumnMetadata columnMetadata)
        {
            if (columnMetadata.Length == -1)
                return columnMetadata.DataType.ToString().ToLower();
            else
                return $"{columnMetadata.DataType.ToString().ToLower()}({columnMetadata.Length})";
        }

        /// <summary>
        /// Formats the property <seealso cref="SqlServerColumnMetadata.IsNullable"/>.
        /// </summary>
        /// <param name="columnMetadata">The column to format the <seealso cref="SqlServerColumnMetadata.IsNullable"/>.</param>
        /// <returns>String version of the property <seealso cref="SqlServerColumnMetadata.IsNullable"/></returns>
        private string FormatColumnNullableField(SqlServerColumnMetadata columnMetadata)
        {
            if (columnMetadata.IsNullable)
                return "null";
            else
                return "not null";
        }

        /// <summary>
        /// Gets the fill data to be used in the generation of the insert command.
        /// </summary>
        /// <returns>The data to be used in the generation of the insert command.</returns>
        private List<Dictionary<string, object>> GetFillDataToInsertCommand()
        {
            var data = new List<Dictionary<string, object>>();

            if (this.GenerateInsertWihData)
                SqlConnector.ExecuteWithSqlAdapter(this.Database, $"select * from {this.Table} where {this.WhereClause}", adapter =>
                {
                    var table = adapter.GetFilledDataSet().Tables[0];
                    var columns = table.Columns.Cast<DataColumn>();
                    var rows = table.Rows.Cast<DataRow>();

                    foreach (var row in rows)
                    {
                        var dictionary = new Dictionary<string, object>();

                        foreach (var column in columns)
                            dictionary.Add(column.ColumnName, row[column.ColumnName]);

                        data.Add(dictionary);
                    }
                });

            return data;
        }

        /// <summary>
        /// Gets metadata of the target table.
        /// </summary>
        /// <returns>The metadata of the target table.</returns>
        private SqlServerTableMetadata GetTableMetadata()
        {
            return new SqlServerMetadataRetriever(this.Database, this.Table).RetrieveMetadata();
        }
    }
}