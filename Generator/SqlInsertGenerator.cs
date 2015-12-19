using CabralRodrigo.Util.SqlServerInsertGenerator.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
            var metadata = this.GetTableMetadata();
            var fillData = this.GetFillDataToInsertCommand();
            var template = new SqlInsertCommandTemplate(new SqlInsertCommandMetadata(metadata), fillData);

            return template.TransformText();
        }

        /// <summary>
        /// Converts a object to a sql string.
        /// </summary>
        /// <param name="objectValue">The object to be converted.</param>
        /// <returns>The string representation of the object that can be used in a sql query.</returns>
        private string ConvertObjectToSqlString(object objectValue)
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
                return ((DateTime)objectValue).ToString("yyyy-MM-dd HH:mm:ss.fff");
            else
                throw new Exception("Unknowm object type: " + objectValue.GetType());
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
        private List<Dictionary<string, string>> GetFillDataToInsertCommand()
        {
            var data = new List<Dictionary<string, string>>();

            if (this.GenerateInsertWihData)
                SqlConnector.ExecuteWithSqlAdapter(this.Database, $"select * from {this.Table} where {this.WhereClause}", adapter =>
                {
                    var table = adapter.GetFilledDataSet().Tables[0];
                    var columns = table.Columns.Cast<DataColumn>();
                    var rows = table.Rows.Cast<DataRow>();

                    foreach (var row in rows)
                    {
                        var dictionary = new Dictionary<string, string>();

                        foreach (var column in columns)
                            dictionary.Add(column.ColumnName, this.ConvertObjectToSqlString(row[column.ColumnName]));

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