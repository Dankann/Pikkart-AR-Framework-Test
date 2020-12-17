/*
 *  Generica interfaccia per gestire gli eventi sui marker
 *  Implementata in PikkartMain
 */

namespace PikkartAR
{
	/// <summary>
	/// Marker object event interface.
	/// </summary>
    public interface IMarkerObjectListener
    {
        void OnMarkerFound(MarkerInfo marker);
        void OnMarkerLost(MarkerInfo markerId);
        void OnARLogoFound(MarkerInfo marker, int payload);
    }
}