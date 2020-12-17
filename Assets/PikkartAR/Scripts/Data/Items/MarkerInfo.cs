/*
 *  Oggetto Marker ritornato nelle calback all'utente, 
 *  Non contiente gli stessi campi del Marker salvato nel database
 */

namespace PikkartAR
{
    public class MarkerInfo
    {
        private string id;
        private int internal_id;
        private string customData;
        private MarkerDatabaseInfo database;
        private double width;
        private double height;
        private int arLogo_Code;
        private bool arLogoEnabled = false;

        public MarkerInfo(string id, int internal_id, double width, double height)
        {
            this.id = id;
            this.internal_id = internal_id;
            this.width = width;
            this.height = height;
            this.arLogo_Code = -1;
        }

        public string getId()
        {
            return id;
        }


        public int getInternalId()
        {
            return internal_id;
        }

        public string getCustomData()
        {
            return customData;
        }

        public MarkerDatabaseInfo getDatabase()
        {
            return database;
        }

        public double getWidth()
        {
            return width;
        }

        public double getHeight()
        {
            return height;
        }

        public void setCustomData(string customData)
        {
            this.customData = customData;
        }

        public void setDatabase(MarkerDatabaseInfo database)
        {
            this.database = database;
        }

        public void setARLogoCode(int v)
        {
            arLogo_Code = v;
        }

        public int getARLogoCode()
        {
            return arLogo_Code;
        }

        public bool isARLogoEnabled()
        {
            return arLogoEnabled;
        }

        public void setARLogoEnabled(bool v)
        {
            arLogoEnabled = v;
        }
    }
}
