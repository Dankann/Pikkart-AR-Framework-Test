namespace PikkartAR
{
	/// <summary>
	/// Recognition options. Keep record of StorageMode and the CloudRecognitionInfo.
	/// </summary>
    public class RecognitionOptions
    {
        public enum RecognitionMode
        {
            TAP_TO_SCAN, CONTINUOUS_SCAN
        }

        /// <summary>
        /// Recognition storage enum. Types of storage option.
        /// Local: only local markers are used.
        /// Cloud: markers are always searched on the server.
        /// Global: both local and cloud mode are enabled. If a marker is not found locally the service asks the server.
        /// </summary>
        public enum RecognitionStorage
        {
            LOCAL, GLOBAL
        }

        /// <summary>
        /// The storage mode.
        /// </summary>
        private RecognitionStorage _storage;
        private RecognitionMode _mode;
        private CloudRecognitionInfo _cloudInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="PikkartAR.RecognitionOptions"/> class.
		/// </summary>
		/// <param name="storage">Recognition Storage Mode.</param>
        public RecognitionOptions(RecognitionStorage storage,
            RecognitionMode mode, CloudRecognitionInfo cloudAuthInfo) {
            
            //if (storage == RecognitionStorage.GLOBAL && 
            //    mode == RecognitionMode.CONTINUOUS_SCAN)
            //    throw new System.Exception("Continuous scan recognition mode is not supported with global storage");
            
            _storage = storage;
            _mode = mode;
            _cloudInfo = cloudAuthInfo;
        }
        
        public RecognitionStorage getStorage()
        {
            return _storage;
        }

        public RecognitionMode getMode()
        {
            return _mode;
        }

        public CloudRecognitionInfo getCloudInfo()
        {
            return _cloudInfo;
        }
    }
}
