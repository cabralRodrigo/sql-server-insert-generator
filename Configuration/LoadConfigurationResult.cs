namespace CabralRodrigo.Util.SqlServerInsertGenerator
{
    internal class LoadConfigurationResult
    {
        public LoadConfigurationResult(bool loadSuccessful, string errorMessage = null)
        {
            this.LoadSucessful = loadSuccessful;
            this.ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }

        public bool LoadSucessful { get; set; }
    }
}