/*
 *  Oggetto MarkerDatabase ritornato nelle calback all'utente, 
 *  Non contiente gli stessi campi del MarkerDatabase salvato nel database
 */

namespace PikkartAR
{
    public class MarkerDatabaseInfo
    {
        private string code;
        private string customData;
        private bool cloud;

        public MarkerDatabaseInfo(string code, string customData, bool cloud)
        {
            this.code = code;
            this.customData = customData;
            this.cloud = cloud;
        }

        public string getCode() {
            return code;
        }

        public string getCustomData() {
            return customData;
        }

        public bool isCloud() {
            return cloud;
        }
    }
}
