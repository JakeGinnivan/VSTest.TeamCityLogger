using System.Xml.Linq;

var xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011";
var file = @"TeamCityTestLoggerForVS2012\source.extension.vsixmanifest";
var doc = XDocument.Load(file);

doc.Element(XName.Get("PackageManifest", xmlns))
	.Element(XName.Get("Metadata", xmlns))
	.Element(XName.Get("Identity", xmlns))
	.SetAttributeValue("Version", Environment.GetEnvironmentVariable("VsixVersion") ?? "1.0.0");
	
doc.Save(file);