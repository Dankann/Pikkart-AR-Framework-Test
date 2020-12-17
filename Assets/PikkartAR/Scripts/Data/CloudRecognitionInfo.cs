/*
 *  Info per la comunicazione col server
 *  WebApiUrl nel codice, estendere e chiamare il costruttore protected se si vuole cambiare
 */

namespace PikkartAR
{
    public class CloudRecognitionInfo
    {
        private string _webApiUrl = "https://ws-2-0.crs.pikkart.com";
        private string[] _databaseNames;

        public CloudRecognitionInfo(string databaseName)
        {
            _databaseNames = new string[] { databaseName };
        }

        public CloudRecognitionInfo(string[] databaseNames)
        {
            _databaseNames = databaseNames;
        }

        protected CloudRecognitionInfo(string webApiUrl, string databaseName)
        {
            _webApiUrl = webApiUrl;
            _databaseNames = new string[] { databaseName };
        }

        protected CloudRecognitionInfo(string webApiUrl, string[] databaseNames)
        {
            _webApiUrl = webApiUrl;
            _databaseNames = databaseNames;
        }

        public string GetWebApiUrl() { return _webApiUrl; }

        protected void SetWebApiUrl(string webApiUrl)
        {
            _webApiUrl = webApiUrl;
        }

        public string[] getDatabaseNames()
        {
            return _databaseNames;
        }

        public void SetDatabaseName(string databaseName)
        {
            _databaseNames = new string[] { databaseName };
        }

        public void SetDatabaseName(string[] databaseNames)
        {
            _databaseNames = databaseNames;
        }
    }
}
