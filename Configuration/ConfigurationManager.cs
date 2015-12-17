using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CabralRodrigo.Util.SqlServerInsertGenerator.Configuration
{
    internal static class ConfigurationManager
    {
        /// <summary>
        /// The root of the lock used to access the <seealso cref="ConfigurationManager.CurrentConfig"/> object.
        /// </summary>
        private static object rootLock = new object();

        /// <summary>
        /// Singleton of the main configuration object.
        /// </summary>
        public static AppConfiguration CurrentConfig { get; private set; }

        /// <summary>
        /// The default path to the configuration file.
        /// </summary>
        private static string ConfigurationFilePath
        {
            get
            {
                return Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName, "config.json");
            }
        }

        /// <summary>
        /// The default path to the last quety file.
        /// </summary>
        private static string LastInsertCommandFilePath
        {
            get
            {
                return Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName, "last-insert-command.sql");
            }
        }

        /// <summary>
        /// Tries to load the configuration of the default configuration file, if the <seealso cref="ConfigurationManager.CurrentConfig"/> if null.
        /// </summary>
        /// <returns>An instance of the class <seealso cref="LoadConfigurationResult"/>.</returns>
        public static LoadConfigurationResult LoadConfiguration()
        {
            if (ConfigurationManager.CurrentConfig == null)
                try
                {
                    var json = File.ReadAllText(ConfigurationManager.ConfigurationFilePath);
                    ConfigurationManager.CurrentConfig = JsonConvert.DeserializeObject<AppConfiguration>(json);

                    if (File.Exists(ConfigurationManager.LastInsertCommandFilePath))
                        ConfigurationManager.CurrentConfig.LastInsertCommand = File.ReadAllText(ConfigurationManager.LastInsertCommandFilePath);
                }
                catch (FileNotFoundException)
                {
                    var sb = new StringBuilder("Configuration file not found." + Environment.NewLine);

                    try
                    {
                        sb.AppendLine("Trying to create a default configuration file.");
                        sb.AppendLine($"Default configuration file created at: '{ConfigurationManager.ConfigurationFilePath}'.");
                        ConfigurationManager.CreateDefaultConfiguration();
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine("Error on creating the default configuration file: " + ex.Message);
                    }

                    return new LoadConfigurationResult(false, sb.ToString());
                }

            return new LoadConfigurationResult(true);
        }

        /// <summary>
        /// Save the <seealso cref="ConfigurationManager.CurrentConfig"/> object to the default configuration file if the object is not null.
        /// </summary>
        public static void SaveConfiguration()
        {
            if (ConfigurationManager.CurrentConfig != null)
                lock (rootLock)
                {
                    if (ConfigurationManager.CurrentConfig != null)
                    {
                        var json = JsonConvert.SerializeObject(ConfigurationManager.CurrentConfig, Formatting.Indented);
                        File.WriteAllText(ConfigurationManager.ConfigurationFilePath, json);

                        if (!ConfigurationManager.CurrentConfig.LastInsertCommand.IsNullOrEmpty())
                            File.WriteAllText(ConfigurationManager.LastInsertCommandFilePath, ConfigurationManager.CurrentConfig.LastInsertCommand);
                    }
                }
        }

        /// <summary>
        /// Create a default configuration file.
        /// </summary>
        private static void CreateDefaultConfiguration()
        {
            var json = JsonConvert.SerializeObject(new AppConfiguration
            {
                DatabaseConfiguration = new DatabaseConnectionConfiguration
                {
                    Server = string.Empty,
                    Password = string.Empty,
                    Login = string.Empty
                },
                GeneratorConfiguration = new InsertGeneratorConfiguration()
            }, Formatting.Indented);

            File.AppendAllText(ConfigurationManager.ConfigurationFilePath, json);
        }
    }
}