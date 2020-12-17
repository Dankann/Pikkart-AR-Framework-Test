using System;
using System.Text;
using System.Runtime.InteropServices;

#if UNITY_ANDROID && !UNITY_EDITOR
using StringType = System.Text.StringBuilder;
#elif UNITY_IOS && !UNITY_EDITOR
using StringType = System.String;
#elif UNITY_WSA_10_0 || UNITY_WP_8_1
using StringType = System.Text.StringBuilder;
#elif UNITY_EDITOR_WIN
using StringType = System.Text.StringBuilder;
#elif UNITY_EDITOR_OSX
using StringType = System.String;
#endif

namespace PikkartAR
{
    /*! \brief Interface class with the native side of Pikkart SDK
    *
    *  Interface class with the native side of Pikkart SDK
    */
    public class PikkartARCore
    {
        const string PikkartLib =
#if UNITY_ANDROID && !UNITY_EDITOR
        "PikkartARCore";
#elif UNITY_IOS && !UNITY_EDITOR
        "__Internal";
#elif UNITY_WSA_10_0 || UNITY_WP_8_1
        "PikkartARCore.dll";
#elif UNITY_EDITOR_WIN
        "libPikkartARCore";
#elif UNITY_EDITOR_OSX
        "PikkartARCore";
#endif

        /*! \brief Initialize the core AR system.
         *
         *  Initialize the native side AR systems.
         *  \param width the width of the camera preview image.
         *  \param height the height of the camera preview image.
         *  \param interval time between consecutive cloud recognition service calls (in milliseconds).
         *  \param appDataDir filesystem app data directory.
         *  \param screenInches diagonal size of the device screen.
         *  \param h_fov horizontal field of view of the camera.
         *  \param v_fov vertical field of view of the camera.
         *  \param rgba_only camera images are RGBA only, no YUV data available. */
        [DllImport(PikkartLib)]
        public static extern int InitRecognition(
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr assetManager,
#endif
            int min_interval_local_marker_not_found, StringType appDataDir,
#if UNITY_EDITOR
            StringType LicencePath, StringType LicenceKey,
#endif
			int process_width, int processheight, float screenInches, 
            int camera_width, int camera_height,
            bool load_markers);


        [DllImport(PikkartLib)]
        public static extern void SetProjectionMatrix(float h_fov, float v_fov);

        [DllImport(PikkartLib)]
        public static extern void SetProjectionMatrix2(float fx, float fy, float cx, float cy);

        [DllImport(PikkartLib)]
        public static extern void SetCameraOriginalSize(int width, int height);

        [DllImport(PikkartLib)]
        public static extern void UpdateViewport(int width, int height, int angle);

        /*! \brief Start the recognition system.
         *
         *  Start the native side of the recognition and tracking systems.
         *  \param onlineRecognition enable online recognition through Pikkart's CRS.
         *  \param effect graphical effect to be overlayed on the camera image when doing recognition.
         */
        [DllImport(PikkartLib)]
        public static extern void StartRecognition(bool do_cloud_recognition, bool effect);

        [DllImport(PikkartLib)]
        public static extern void StopRecognition();

        [DllImport(PikkartLib)]
        public static extern void EnableRecognition();

        [DllImport(PikkartLib)]
        public static extern void DisableRecognition();

        [DllImport(PikkartLib)]
        public static extern void ChangeMode(bool onlineRecognition);

        /*! \brief Send RGBA channel camera data to the native core.
         *
         *  Send uncompressed RGBA camera data to the native core.
         *  this function also convert data to gray-scale in order to be processed.
         *  \param frameBuffer pointer to image data buffer
         */
        [DllImport(PikkartLib)]
        public static extern void CopyNewFrame(IntPtr frameBuffer, int image_width, int image_height);

        [DllImport(PikkartLib)]
        public static extern void CopyNewFrame32(UnityEngine.Color32[] frameBuffer, int image_width, int image_height);

        [DllImport(PikkartLib)]
        public static extern void CopyNewFrameFromGL(IntPtr gl_id, int image_width, int image_height);
        /*! \brief Process camera frame.
         *
         *  Process camera frame in order to detect and track marker
         *  return true if has detected or is tracking something.
         */
        [DllImport(PikkartLib)]
        public static extern bool ImageProcessing();

        [DllImport(PikkartLib)]
        public static extern void StartEffect();

        [DllImport(PikkartLib)]
        public static extern void StopEffect();

        [DllImport(PikkartLib)]
        public static extern void SyncMarkersWithFolder();

        /*! \brief Save a marker to local directory.
         *
         *  Save a marker into device internal memory.
         *  \param markerId the ID of the marker to be saved.
         *  \param markerDescriptor the marker data.
         */
        [DllImport(PikkartLib)]
        public static extern bool SaveLocalMarker(StringType markerId, StringType markerDescriptor);

        /*! \brief Delete a locally saved marker.
         *
         *  Delete a locally saved marker. Free allocated RAM and stop tracking if needed.
         *  \param localMarkerId the ID of the marker to be deleted.
         */
        [DllImport(PikkartLib)]
        public static extern bool DeleteLocalMarker(StringType localMarkerId, bool remove_file);

        /*! \brief Get pointer to compressed image data buffer to be sent to the cloud recognition service
         *
         *  Get pointer to compressed image data buffer to be sent to the cloud recognition service
         */
        [DllImport(PikkartLib)]
        public static extern int GetUpdatedServerBuffer(IntPtr serverBuffer);

        [DllImport(PikkartLib)]
        public static extern bool ForceMarkerSearch(StringType markerId);

        [DllImport(PikkartLib)]
        public static extern bool ForceMarkerSearchInternalId(int markerId);

        /*! \brief Tell Pikkart's native core were the camera image has to be stored (for OpenGL devices)
         *
         *  Tell Pikkart's native core the OpenGL texture id were the current processed camera image has 
         *  to be stored (to be later shown by Unity).
         *  This function assume the camera image was passed to the native cure as YUV data.
         *  This function is usually called once at the begginning. The native core automatically convert YUV data
         *  to RGBA and update the opengl texture data at the beginning of each Unity rendering cycle.
         *
         *  \param outputCameraBuffer a pointer whose value is the gl texture id (the actual value of the pointer
         *   not its pointed value. Blame Unity for this).
         */
        [DllImport(PikkartLib)]
        public static extern void CameraUpdateUnityTextureId_GL(IntPtr outputCameraBuffer);

        [DllImport(PikkartLib)]
        public static extern void ForceInitCameraGL(int camera_width, int camera_height);

        [DllImport(PikkartLib)]
        public static extern void RenderCameraGL(int camera_width, int camera_height, int viewport_width, int viewport_height, int angle);

#if UNITY_EDITOR
        [DllImport(PikkartLib)]
        public static extern void UpdateCameraTexture(IntPtr outputCameraBuffer);
#endif

        [DllImport(PikkartLib)]
        public static extern int GetActiveMarkersInternalIDs(int[] id_array);

        [DllImport(PikkartLib)]
        public static extern int GetTrackedInternalIDs(int[] id_array);

        [DllImport(PikkartLib)]
        public static extern bool IsTrackingName(StringType marker_database_Id);

        [DllImport(PikkartLib)]
        public static extern bool IsTrackingInternalID(int marker_internal_Id);

        [DllImport(PikkartLib)]
        public static extern int GetMarkerNameFromInternalIDs(int marker_internal_Id, IntPtr marker_database_Id);

        [DllImport(PikkartLib)]
        public static extern float GetMarkerWidthName(StringType marker_database_Id);

        [DllImport(PikkartLib)]
        public static extern float GetMarkerWidthInternalID(int marker_internal_Id);

        [DllImport(PikkartLib)]
        public static extern float GetMarkerHeightName(StringType marker_database_Id);

        [DllImport(PikkartLib)]
        public static extern float GetMarkerHeightInternalID(int marker_internal_Id);

        [DllImport(PikkartLib)]
        public static extern int GetMarkerPoseInternalID(int marker_internal_Id, float[] pose_array);

        [DllImport(PikkartLib)]
        public static extern int GetMarkerPoseName(StringType marker_database_Id, float[] pose_array);

        [DllImport(PikkartLib)]
        public static extern int GetMarkerPoseName_WithPrediction(StringType marker_database_Id, int frame_prediction, float[] output);

        [DllImport(PikkartLib)]
        public static extern int GetMarkerPoseInternalID_WithPrediction(int marker_internal_Id, int frame_prediction, float[] output);

        [DllImport(PikkartLib)]
        public static extern int MarkerHasLogoName(StringType markerId);

        [DllImport(PikkartLib)]
        public static extern int MarkerHasLogoInternalId(int markerId);

        [DllImport(PikkartLib)]
        public static extern int GetProjectionMatrix(float[] proj_data);

        [DllImport(PikkartLib)]
        public static extern void DecryptCalibParams(IntPtr data, float[] out_params);


        [DllImport(PikkartLib)]
        public static extern bool LoadDiscoverModel(StringType modelname);

        [DllImport(PikkartLib)]
        public static extern void EnableDiscover(bool enabled);

        [DllImport(PikkartLib)]
        public static extern int GetActiveInterestPoints(int[] internal_ids);

        [DllImport(PikkartLib)]
        public static extern void GetPositionOfInterestPoint(int ip_internal_id, float[] pos);

        [DllImport(PikkartLib)]
        public static extern int GetInterestPointPublicID(int ip_internal_id, IntPtr ip_public_id);
        /*! \brief Check for a valid license file.
         *
         *  Check for a valid license file.
         */
        [DllImport(PikkartLib)]
        public static extern bool CheckUnityLicense(
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr assetManager
#elif UNITY_EDITOR_WIN
            StringType p, StringType k
#endif
            );

        /*! \brief Creates a server signature for the cloud recognition service
         *
         *  Creates a server signature for the cloud recognition service.
         */
        [DllImport(PikkartLib)]
        public static extern void CreateUnityServerSignature(StringType method, StringType body,
            StringType date, StringType path, IntPtr output);


        /*! \brief Send render event to the native core.
         * For Pikkart SDK internal use only.
         */
        [DllImport(PikkartLib)]
        public static extern IntPtr GetRenderEventFunc();

        
    }
}