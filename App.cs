using CabralRodrigo.Util.SqlServerInsertGenerator.Configuration;
using CabralRodrigo.Util.SqlServerInsertGenerator.Generator;
using System;
using System.Windows.Forms;

namespace CabralRodrigo.Util.SqlServerInsertGenerator
{
    public static class App
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var result = ConfigurationManager.LoadConfiguration();
                if (result.LoadSucessful)
                {
                    var database = GetConsoleStringWithDefault("Enter the database name", ConfigurationManager.CurrentConfig.GeneratorConfiguration.Database, false);
                    var table = GetConsoleStringWithDefault("Enter the table name", ConfigurationManager.CurrentConfig.GeneratorConfiguration.Table, false);
                    var where = GetConsoleStringWithDefault("Enter the where clause (can be empty)", ConfigurationManager.CurrentConfig.GeneratorConfiguration.WhereClause, true);

                    var generator = new SqlInsertGenerator(database, table, where);
                    var insertCommand = generator.Build();

                    ConfigurationManager.CurrentConfig.GeneratorConfiguration.Database = database;
                    ConfigurationManager.CurrentConfig.GeneratorConfiguration.Table = table;
                    ConfigurationManager.CurrentConfig.GeneratorConfiguration.WhereClause = where;
                    ConfigurationManager.CurrentConfig.LastInsertCommand = insertCommand;
                    ConfigurationManager.SaveConfiguration();

                    Clipboard.SetText(insertCommand);
                    Console.WriteLine("Insert command copied to clipboard...");
                }
                else
                    Console.WriteLine(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static string GetConsoleStringWithDefault(string message, string defaultValue, bool canBeEmpty)
        {
            string result = string.Empty;
            do
            {
                Console.Write($"{message} (default: '{defaultValue}'): ");
                result = Console.ReadLine();
            } while (canBeEmpty ? false : defaultValue.IsNullOrEmpty() && result.IsNullOrEmpty());

            return result.IsNullOrEmpty() ? defaultValue : result;
        }
    }
}