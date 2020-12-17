namespace PikkartAR {

	public static class Constants {

		//public const string BASE_URL = "https://ws-test.ar.pikkart.com";
		public const string BASE_URL = "http://testcloud.pikkart.com:8084";

		public const int LOCAL_MARKER_MAX_COUNT = 100;

		public const string DB_NAME = "pikkart.db";

		public const float DEFAULT_NET_TIMEOUT_SEC = 55f;

		public const int APPLICATION_TARGET_FRAMERATE = 0;
		
		public const int UNITY_LARGE_FOV = 60;
		public const int UNITY_SMALL_FOV = 37;

		public static float CAM_LARGE_FOV = 56;
		public static float CAM_SMALL_FOV = 43;

		public const int PROCESS_WIDTH = 640;
		public const int PROCESS_HEIGHT = 480;

        public const int CAMERA_REQUESTED_WIDTH = 640;
        public const int CAMERA_REQUESTED_HEIGHT = 480;

		public const float NEAR_CLIP_PLANE = 0.01f;
		public const float FAR_CLIP_PLANE = 10f;

		public const int MATRIX_4X4_ARRAY_SIZE = 16;
		public const int MATRIX_3X4_ARRAY_SIZE = 12;

		public const int INTERVAL = 1000;
		public const int SHOW_EFFECT = 1;

		public const float SCREEN_ORIENTATION_CHECK_DELAY_TIME = 0.5f;

		public const float FIXED_FX_LANDSCAPE = 1.721197f;
		public const float FIXED_FY_LANDSCAPE = 2.797684f;
		public const float FIXED_FX_PORTRAIT = 2.763514f;
		public const float FIXED_FY_PORTRAIT = 1.830726f;

		public const bool CREATE_NEW_DB = false;

		//public const string UNITY_DEVICE_ID = "testUnity";

		public const string DATE_FORMAT = "R";
	}
}

