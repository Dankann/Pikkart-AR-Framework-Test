using System.Xml;
using System.Xml.Serialization;

/*
 *  Mappa gli xml dei marker
 */

/// <summary>
/// Marker info.
/// Stores marker data red from local xml.
/// </summary>
public class MarkerData {

	[XmlAttribute("name")]
	public string name;

	[XmlAttribute("width")]
	public float width;

	[XmlAttribute("height")]
	public float height;
}
