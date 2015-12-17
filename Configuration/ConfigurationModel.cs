using Newtonsoft.Json;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Configuration
{
    /// <summary>
    /// The general configuration for the application.
    /// </summary>
    internal class AppConfiguration
    {
        [JsonRequired]
        public DatabaseConnectionConfiguration DatabaseConfiguration { get; set; }

        [JsonRequired]
        public InsertGeneratorConfiguration GeneratorConfiguration { get; set; }

        /// <summary>
        /// Last insert command generated successfully.
        /// </summary>
        [JsonIgnore]
        public string LastInsertCommand { get; set; }
    }

    /// <summary>
    /// The configuration for connecting with the database server.
    /// </summary>
    internal class DatabaseConnectionConfiguration
    {
        [JsonRequired]
        public string Password { get; set; }

        [JsonRequired]
        public string Login { get; set; }

        [JsonRequired]
        public string Server { get; set; }
    }

    /// <summary>
    /// The configuration for the <seealso cref="Generator.SqlInsertGenerator"/>.
    /// </summary>
    internal class InsertGeneratorConfiguration
    {
        /// <summary>
        /// Black list of all columns that the insert generator should ignore.
        /// </summary>
        public string[] ColumnBlackList { get; set; } = new string[0];

        /// <summary>
        /// The name of the database to read all information to generate the insert command.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// The targe table of the insert command.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// The where clause used to get data to make the insert command with.
        /// </summary>
        public string WhereClause { get; set; }
    }
}