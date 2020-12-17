/*
 *  Interfaccia per le callback del DataProvider
 *  Implementata dal RecognitionManager
 */

namespace PikkartAR
{
	/// <summary>
	/// DataProvider communication interface.
	/// </summary>
	public interface IRecognitionDataProviderListener : INetworkInfoProvider
	{
		void FindMarkerCallback(Marker marker);
        void FindSimilarMarkerCallback(string[] markersId);
        void GetMarkerCallback(Marker marker);
        void GetCameraParamCallback(double[] cam_params, bool save);
    }
}