namespace PikkartAR
{
	/// <summary>
	/// RecognitionManager communication interface.
	/// </summary>
    public interface IRecognitionListener : INetworkInfoProvider
    {
        void OnPikkartARStartupComplete();
        void ExecutingCloudSearch();
        void CloudMarkerNotFound();
        void InternetConnectionNeeded();
        
        void MarkerFound(MarkerInfo marker/*, MarkerObject markerObject*/);
        void MarkerNotFound();
        void MarkerTrackingLost(string markerId/*, MarkerObject markerObject*/);

        void ARLogoFound(MarkerInfo marker, int payload);
    }
}