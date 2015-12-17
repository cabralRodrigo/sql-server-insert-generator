using CabralRodrigo.Util.SqlServerInsertGenerator.Configuration;
using System;
using System.Data;
using System.Linq;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Metadata
{
    public class SqlServerMetadataRetriever
    {
        /// <summary>
        /// Initializes a new instance of <seealso cref="SqlServerMetadataRetriever"/>.
        /// </summary>
        /// <param name="database">The database to get the metadata.</param>
        /// <param name="table">The table which the metadata will be retrieved.</param>
        public SqlServerMetadataRetriever(string database, string table)
        {
            this.Table = table;
            this.Database = database;
        }

        /// <summary>
        /// The table which the metadata will be retrieved.
        /// </summary>
        public string Table { get; private set; }

        /// <summary>
        /// The database to get the metadata.
        /// </summary>
        private string Database { get; set; }

        /// <summary>
        /// Retrieve the metadata about the <seealso cref="SqlServerMetadataRetriever.Table"/> table.
        /// </summary>
        /// <returns>The metadata retrived from the server about the <seealso cref="SqlServerMetadataRetriever.Table"/> table.</returns>
        public SqlServerTableMetadata RetrieveMetadata()
        {
            SqlServerTableMetadata metadata = null;
            SqlConnector.ExecuteWithSqlAdapter(Database, $"select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{this.Table}'", adapter =>
            {
                metadata = this.ParseDataSet(adapter.GetFilledDataSet());
            });

            return metadata;
        }

        /// <summary>
        /// Finds the <seealso cref="SqlDbType"/> for a string.
        /// </summary>
        /// <param name="typeString">The string for find the <seealso cref="SqlDbType"/>.</param>
        /// <returns>The <seealso cref="SqlDbType"/> found.</returns>
        private SqlDbType FindDbType(string typeString)
        {
            foreach (string dbType in Enum.GetNames(typeof(SqlDbType)))
                if (typeString.Trim().ToLower() == dbType.ToLower())
                    return (SqlDbType)Enum.Parse(typeof(SqlDbType), dbType);

            throw new Exception($"Couldn't find the db type for string: {typeString}");
        }

        /// <summary>
        /// Parses a <seealso cref="DataSet"/> into <seealso cref="SqlServerTableMetadata"/>.
        /// </summary>
        /// <param name="dataset">The <seealso cref="DataSet"/> to be parsed.</param>
        /// <returns>An instance of the <seealso cref="SqlServerTableMetadata"/> class.</returns>
        private SqlServerTableMetadata ParseDataSet(DataSet dataset)
        {
            if (dataset.Tables[0].Rows.Count < 1)
                throw new Exception($"The table '{this.Table}' wasn't found in the database '{this.Database}'.");

            return new SqlServerTableMetadata
            {
                TableName = dataset.Tables[0].Rows[0].Field<string>("TABLE_NAME").Trim(),
                Columns = dataset.Tables[0].Rows.Cast<DataRow>().Select(s => new SqlServerColumnMetadata
                {
                    Schema = s.Field<string>("TABLE_NAME"),
                    Name = s.Field<string>("COLUMN_NAME"),
                    Position = s.Field<int>("ORDINAL_POSITION"),
                    DefaultValue = s.Field<string>("COLUMN_DEFAULT"),
                    IsNullable = s.Field<string>("IS_NULLABLE").ToLower() != "no",
                    DataType = FindDbType(s.Field<string>("DATA_TYPE").Trim()),
                    Length = s.Field<int?>("CHARACTER_MAXIMUM_LENGTH") ?? -1
                })
                .Where(w => !ConfigurationManager.CurrentConfig.GeneratorConfiguration.ColumnBlackList.Contains(w.Name))
                .OrderBy(s => s.Position)
                .ToList()
            };
        }
    }
}