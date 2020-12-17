/*
 *  Interfaccia per gestire i marker che arrivano dal cloud.
 *  Una volta implementata va settata con il metodo SetCloudMarkerObjectListener in PikartMain
 */

namespace PikkartAR
{
	/// <summary>
	/// Cloud marker object event interface.
	/// </summary>
    public interface ICloudMarkerObjectListener
    {
        void OnCloudMarkerFound(MarkerInfo marker);
        void OnCloudMarkerLost(MarkerInfo marker);
        void OnCloudARLogoFound(MarkerInfo marker, int payload);
    }
}