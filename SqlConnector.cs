using CabralRodrigo.Util.SqlServerInsertGenerator.Configuration;
using System;
using System.Data.SqlClient;

namespace CabralRodrigo.Util.SqlServerInsertGenerator
{
    internal static class SqlConnector
    {
        /// <summary>
        /// Execute a <seealso cref="Action{SqlDataAdapter}"/> with a <seealso cref="SqlDataAdapter"/>.
        /// </summary>
        /// <param name="database">The database to get the connection to open the <seealso cref="SqlDataAdapter"/>.</param>
        /// <param name="command">The command to execute the <seealso cref="SqlDataAdapter"/>.</param>
        /// <param name="action">The action that will be executed.</param>
        public static void ExecuteWithSqlAdapter(string database, string command, Action<SqlDataAdapter> action)
        {
            using (var connection = GetConnection(database))
            using (var sqlCommand = new SqlCommand(command, connection))
            using (var adapter = new SqlDataAdapter(sqlCommand))
            {
                connection.Open();
                action(adapter);
            }
        }

        /// <summary>
        /// Gets the <seealso cref="SqlConnection"/> based on the current configuration.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>The created <seealso cref="SqlConnection"/>.</returns>
        private static SqlConnection GetConnection(string database)
        {
            return new SqlConnection(new SqlConnectionStringBuilder
            {
                InitialCatalog = database,
                UserID = ConfigurationManager.CurrentConfig.DatabaseConfiguration.Login,
                Password = ConfigurationManager.CurrentConfig.DatabaseConfiguration.Password,
                DataSource = ConfigurationManager.CurrentConfig.DatabaseConfiguration.Server
            }.ToString());
        }
    }
}