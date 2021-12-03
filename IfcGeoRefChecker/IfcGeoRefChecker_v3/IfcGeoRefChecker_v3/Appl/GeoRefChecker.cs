using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace IfcGeoRefChecker.Appl
{
    public class GeoRefChecker
    {
        public string GlobalID { get; set; }
        public string IFCSchema { get; set; }
        public string TimeCreation { get; set; }
        public string TimeCheck { get; set; }
        public string LengthUnit { get; set; }
        public string WKTRep { get; set; }
        public List<Level10> LoGeoRef10 { get; set; } = new List<Level10>();
        public List<Level20> LoGeoRef20 { get; set; } = new List<Level20>();
        public List<Level30> LoGeoRef30 { get; set; } = new List<Level30>();
        public List<Level40> LoGeoRef40 { get; set; } = new List<Level40>();
        public List<Level50> LoGeoRef50 { get; set; } = new List<Level50>();

        private IO.IfcReader obj;

        /// <summary>
        /// Creates Checker object from JSON
        /// </summary>
        public GeoRefChecker(string jsonString)
        {
            Log.Information("GeoRefChecker: Deserialization of JSON-string initialized...");

            try
            {
                JObject jsonObj = JObject.Parse(jsonString);

                this.GlobalID = jsonObj["GlobalID"].ToString();
                this.IFCSchema = jsonObj["IFCSchema"].ToString();
                this.TimeCheck = jsonObj["TimeCheck"].ToString();
                this.TimeCreation = jsonObj["TimeCreation"].ToString();

                this.LengthUnit = jsonObj["LengthUnit"].ToString();

                var lev10 = jsonObj["LoGeoRef10"].Children();

                foreach(var res in lev10)
                {
                    var l10 = new Level10();
                    JsonConvert.PopulateObject(res.ToString(), l10);
                    this.LoGeoRef10.Add(l10);
                }

                var lev20 = jsonObj["LoGeoRef20"].Children();

                foreach(var res in lev20)
                {
                    var l20 = new Level20();
                    JsonConvert.PopulateObject(res.ToString(), l20);
                    this.LoGeoRef20.Add(l20);
                }

                var lev30 = jsonObj["LoGeoRef30"].Children();

                foreach(var res in lev30)
                {
                    var l30 = new Level30();
                    JsonConvert.PopulateObject(res.ToString(), l30);
                    this.LoGeoRef30.Add(l30);
                }

                var lev40 = jsonObj["LoGeoRef40"].Children();

                foreach(var res in lev40)
                {
                    var l40 = new Level40();
                    JsonConvert.PopulateObject(res.ToString(), l40);
                    this.LoGeoRef40.Add(l40);
                }

                var lev50 = jsonObj["LoGeoRef50"].Children();

                foreach(var res in lev50)
                {
                    var l50 = new Level50();
                    JsonConvert.PopulateObject(res.ToString(), l50);
                    this.LoGeoRef50.Add(l50);
                }

                Log.Information("GeoRefChecker: Deserialization of JSON - string successful.");
            }
            catch(Exception e)
            {
                var str = "GeoRefChecker: Error occured while deserialization of JSON-file Error: ." + e.Message;

                Log.Error(str);
                MessageBox.Show(str);
            }
        }

        /// <summary>
        /// Creates Checker object from IFC-file
        /// </summary>
        public GeoRefChecker(IfcStore model)
        {
            try
            {
                Log.Information("GeoRefChecker: Checking of IFC-file initialized...");

                this.obj = new IO.IfcReader(model);

                var bldg = obj.BldgReader();
                var site = obj.SiteReader();
                var proj = obj.ProjReader();
                var prods = obj.UpperPlcmProdReader();

                this.GlobalID = proj.GlobalId;
                this.IFCSchema = model.SchemaVersion.ToString();
                this.TimeCreation = model.Header.TimeStamp;
                this.TimeCheck = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);      //UTC timestamp
                this.LengthUnit = obj.LengthUnitReader();

                if(bldg != null)
                {
                    this.LoGeoRef10.Add(GetLevel10(bldg));
                }
                //this.LoGeoRef10.Add(GetLevel10(bldg));
                this.LoGeoRef10.Add(GetLevel10(site));

                this.LoGeoRef20.Add(GetLevel20(site));

                if(prods.Contains(site))
                {
                    this.LoGeoRef30.Add(GetLevel30(site));          //global placement investigation only for elements which can be seriously georeferenced (site -> default)
                }

                if(prods.Contains(bldg))
                {
                    this.LoGeoRef30.Add(GetLevel30(bldg));          //global placement investigation only for elements which can be seriously georeferenced (bldg -> possible)
                }

                //foreach(var prod in prods)
                //{
                //    this.LoGeoRef30.Add(GetLevel30(prod));
                //}                                                 //DEPRECATED: other products than bldg or site will not be handled but will be later considered when editing file

                this.LoGeoRef40.Add(GetLevel40(proj));

                if(model.SchemaVersion.ToString() != "Ifc2X3")      //für Versionen ab IFC4
                {
                    this.LoGeoRef50.Add(GetLevel50(proj));
                }
                else                                                //für Versionen IFC2x3: Investigation of applied PropertySets for georeferencing
                {
                    var psetMap = obj.PSetReaderMap();
                    var psetCrs = obj.PSetReaderCRS();

                    if(psetMap != null && psetCrs != null)
                    {
                        this.LoGeoRef50.Add(GetLevel50(psetMap, psetCrs));
                    }
                    else
                    {
                        this.LoGeoRef50.Add(GetLevel50(proj));

                        //var l50f = new Level50();
                        //l50f.GeoRef50 = false;
                        //l50f.Reference_Object = GetInfo(proj);
                        //this.LoGeoRef50.Add(l50f);
                    }
                }
            }
            catch(Exception e)
            {
                var str = "GeoRefChecker: Error occured while checking of IFC-file Error: ." + e.Message;

                Log.Error(str);
                MessageBox.Show(str);
            }
        }

        private Level10 GetLevel10(IIfcSpatialStructureElement spatialElement)
        {
            var l10 = new Appl.Level10();

            try
            {
                Log.Information("GeoRefChecker: Reading Level 10 attributes started...");

                IIfcPostalAddress address = null;

                if(spatialElement is IIfcSite)
                {
                    address = (spatialElement as IIfcSite).SiteAddress;
                }
                else
                {
                    address = (spatialElement as IIfcBuilding).BuildingAddress;
                }

                l10.Reference_Object = GetInfo(spatialElement);

                if(address != null)
                {
  
                    l10.Instance_Object = GetInfo(address);

                    var alines = address.AddressLines;

                    // check if at least one value is present
                    // "WHERE WR1 : EXISTS (InternalLocation) OR EXISTS (AddressLines) OR EXISTS (PostalBox) OR EXISTS (PostalCode) OR EXISTS (Town) OR EXISTS (Region) OR EXISTS (Country);"
                    if (alines.Count > 0 || address.Country.HasValue || address.PostalCode.HasValue || address.Town.HasValue || address.Region.HasValue)
                    {
                        // check that the attributes do not contain only spaces
                        if ((address.PostalCode.ToString().All(c => c.Equals(' ')) == false) || (address.Town.ToString().All(c => c.Equals(' ')) == false) 
                            || (address.Region.ToString().All(c => c.Equals(' ')) == false) || (address.Country.ToString().All(c => c.Equals(' ')) == false))
                        {
                            l10.GeoRef10 = true;
                        }

                        foreach (var a in alines)
                        {
                            if (a != null && (a.ToString().All(c => c.Equals(' ')) == false))
                            {
                                l10.GeoRef10 = true;
                            }
                        }
                    }

                    if(alines.Count > 0)
                    {
                        foreach(var a in alines)
                        {
                            if (a != null && (a.ToString().All(c => c.Equals(' ')) == false))
                            {
                                l10.AddressLines.Add(a);
                            }
                            else
                            {
                                l10.AddressLines.Add("n/a ");

                            }
                        }
                    }
                    else
                    {
                        l10.AddressLines.Add("n/a");
                    }

                    l10.Postalcode = (address.PostalCode.HasValue && (address.PostalCode.ToString().All(c => c.Equals(' ')) == false)) ? address.PostalCode.ToString() : "n/a";
                    l10.Town = (address.Town.HasValue && (address.Town.ToString().All(c => c.Equals(' ')) == false)) ? address.Town.ToString() : "n/a";
                    l10.Region = (address.Region.HasValue && (address.Region.ToString().All(c => c.Equals(' ')) == false)) ? address.Region.ToString() : "n/a";
                    l10.Country = (address.Country.HasValue && (address.Country.ToString().All(c => c.Equals(' ')) == false)) ? address.Country.ToString() : "n/a";
                }
                else
                {
                    l10.GeoRef10 = false;
                }

                Log.Information("GeoRefChecker: Reading Level 10 attributes successful.");
            }

            catch(Exception e)
            {
                Log.Error("GeoRefChecker: Error occured while reading LoGeoRef10 attribute values. \r\nError message: " + e.Message);
            }

            return l10;
        }

        private Level20 GetLevel20(IIfcSite site)
        {
            var l20 = new Level20();

            try
            {
                Log.Information("GeoRefChecker: Reading Level 20 attributes started...");

                l20.Reference_Object = GetInfo(site);

                //if(site.RefLatitude.HasValue || site.RefLongitude.HasValue)
                //{
                //    l20.Latitude = site.RefLatitude.Value.AsDouble;
                //    l20.Longitude = site.RefLongitude.Value.AsDouble;
                //
                //    l20.GeoRef20 = true;
                //}
                //else
                //{
                //    l20.Latitude = null;
                //    l20.Longitude = null;
                //
                //    l20.GeoRef20 = false;
                //}
                //
                //l20.Elevation = site.RefElevation.Value;

                var plcmsite = new PlacementXYZ(site);

                l20.GeoRef20 = plcmsite.GeoRefPlcm;
                
                if (l20.GeoRef20)
                {
                    l20.Latitude = plcmsite.LocationXYZ[0];
                    l20.Longitude = plcmsite.LocationXYZ[1];
                    if (site.RefElevation.HasValue)
                    {
                        l20.Elevation = plcmsite.LocationXYZ[2];
                    }
                    else
                    {
                        l20.Elevation = null;
                    }
                }

                Log.Information("GeoRefChecker: Reading Level 20 attributes successful.");
            }
            catch(Exception e)
            {
                MessageBox.Show("GeoRefChecker: Error occured while reading LoGeoRef20 attribute values. Error: " + e.Message);
                Log.Error("GeoRefChecker: Error occured while reading LoGeoRef20 attribute values. \r\nError message: " + e.Message);
            }

            return l20;
        }

        private Level30 GetLevel30(IIfcProduct elem)
        {
            var l30 = new Level30();

            try
            {
                Log.Information("GeoRefChecker: Reading Level 30 attributes started...");

                var elemPlcm = (IIfcLocalPlacement)elem.ObjectPlacement;
                var plcm3D = (IIfcAxis2Placement3D)elemPlcm.RelativePlacement;

                l30.Reference_Object = GetInfo(elem);

                l30.Instance_Object = GetInfo(plcm3D);

                var plcm = new PlacementXYZ(plcm3D);

                l30.GeoRef30 = plcm.GeoRefPlcm;
                l30.ObjectLocationXYZ = plcm.LocationXYZ;
                l30.ObjectRotationX = plcm.RotationX;
                l30.ObjectRotationZ = plcm.RotationZ;

                Log.Information("GeoRefChecker: Reading Level 30 attributes successful.");
            }
            catch(Exception e)
            {
                Log.Error("GeoRefChecker: Error occured while reading LoGeoRef30 attribute values. \r\nError message: " + e.Message);
            }

            return l30;
        }

        private Level40 GetLevel40(IIfcProject proj)
        {
            var l40 = new Level40();

            try
            {
                Log.Information("GeoRefChecker: Reading Level 40 attributes started...");

                l40.Reference_Object = GetInfo(proj);

                var prjCtx = obj.ContextReader(proj).Where(a => a.ContextType == "Model").SingleOrDefault();

                l40.Instance_Object = GetInfo(prjCtx);

                //variable for the WorldCoordinatesystem attribute
                var plcm = prjCtx.WorldCoordinateSystem;
                var plcmXYZ = new PlacementXYZ(plcm);

                l40.GeoRef40 = plcmXYZ.GeoRefPlcm;
                l40.ProjectLocation = plcmXYZ.LocationXYZ;
                l40.ProjectRotationX = plcmXYZ.RotationX;

                if(plcm is IIfcAxis2Placement3D)
                {
                    l40.ProjectRotationZ = plcmXYZ.RotationZ;
                }

                //variable for the TrueNorth attribute
                var dir = prjCtx.TrueNorth;

                l40.TrueNorthXY = new List<double>();

                if(dir != null)
                {
                    l40.TrueNorthXY.Add(dir.DirectionRatios[0]);
                    l40.TrueNorthXY.Add(dir.DirectionRatios[1]);
                }
                else
                {
                    //if omitted, default values (see IFC schema for IfcGeometricRepresentationContext):

                    l40.TrueNorthXY.Add(0);
                    l40.TrueNorthXY.Add(1);
                }

                Log.Information("GeoRefChecker: Reading Level 40 attributes successful.");
            }
            catch(Exception e)
            {
                Log.Error("GeoRefChecker: Error occured while reading LoGeoRef40 attribute values. \r\nError message: " + e.Message);
            }

            return l40;
        }

        private Level50 GetLevel50(IIfcProject proj)
        {
            var l50 = new Level50();

            try
            {
                Log.Information("GeoRefChecker: Reading Level 50 attributes via MapConversion started...");

                l50.Reference_Object = GetInfo(proj);

                var prjCtx = obj.ContextReader(proj).Where(a => a.ContextType == "Model").SingleOrDefault();

                var map = obj.MapReader(prjCtx);

                if (map != null)
                {
                    var plcmXYZ = new PlacementXYZ(map);

                    l50.Instance_Object = GetInfo(map);

                    l50.Translation_Eastings = plcmXYZ.LocationXYZ[0];
                    l50.Translation_Northings = plcmXYZ.LocationXYZ[1];
                    l50.Translation_Orth_Height = plcmXYZ.LocationXYZ[2];

                    l50.RotationXY = plcmXYZ.RotationXY;

                    // Ausgelagert nach PlacementXYZ (IIfcMapConversion mapc)
                    //if(map.XAxisAbscissa.HasValue && map.XAxisOrdinate.HasValue)
                    //{
                    //    l50.RotationXY = new List<double>();

                    //    l50.RotationXY.Add(map.XAxisAbscissa.Value);
                    //    l50.RotationXY.Add(map.XAxisOrdinate.Value);
                    //}
                    //else
                    //{
                    //    //if omitted, values for no rotation (angle = 0) applied (consider difference to True North)

                    //    l50.RotationXY.Add(0);
                    //    l50.RotationXY.Add(1);
                    //}

                    l50.Scale = (map.Scale.HasValue) ? map.Scale.Value : 1;

                    var mapCRS = (IIfcProjectedCRS)map.TargetCRS;

                    if(mapCRS != null)
                    {
                        l50.GeoRef50 = plcmXYZ.GeoRefPlcm;

                        //Die Angabe des CRS ist für LoGeoRef50 auch erforderlich. Angabe muss dabei im EPSG:xxxx Format erfolgen. (Leerzeichen zwischen EPSG: und xxxx werden hier auch geduldet)
                        if (mapCRS.Name != null && Regex.IsMatch(mapCRS.Name, @"^EPSG:\s*[0-9]+$", RegexOptions.IgnoreCase))
                        {
                            l50.CRS_Name = mapCRS.Name.ToString(); 
                        }
                        else
                        {
                            l50.GeoRef50 = false;
                        }

                        l50.CRS_Description = (mapCRS.Description != null) ? mapCRS.Description.ToString() : "n/a";
                        l50.CRS_Geodetic_Datum = (mapCRS.GeodeticDatum != null) ? mapCRS.GeodeticDatum.ToString() : "n/a";
                        l50.CRS_Vertical_Datum = (mapCRS.VerticalDatum != null) ? mapCRS.VerticalDatum.ToString() : "n/a";
                        l50.CRS_Projection_Name = (mapCRS.MapProjection != null) ? mapCRS.MapProjection.ToString() : "n/a";
                        l50.CRS_Projection_Zone = (mapCRS.MapZone != null) ? mapCRS.MapZone.ToString() : "n/a";

                    }
                    else
                    {
                        l50.GeoRef50 = false;
                    }

                }
                else
                {
                    l50.GeoRef50 = false;
                }

                Log.Information("GeoRefChecker: Reading Level 50 attributes successful.");
            }

            catch(Exception e)
            {
                Log.Error("GeoRefChecker: Error occured while reading LoGeoRef50 attribute values. \r\nError message: " + e.Message);
            }

            return l50;
        }

        private Level50 GetLevel50(IIfcPropertySet psetMap, IIfcPropertySet psetCrs)
        {
            var l50 = new Level50();

            try
            {
                Log.Information("GeoRefChecker: Reading Level 50 attributes via PropertySets started...");

                foreach(var pset in psetMap.DefinesOccurrence)
                {
                    var relObj = pset.RelatedObjects;

                    if(relObj.Contains(obj.ProjReader()))
                    {
                        l50.Reference_Object = GetInfo(obj.ProjReader());
                        break;
                    }
                    else if(relObj.Contains(obj.SiteReader()))
                    {
                        l50.Reference_Object = GetInfo(obj.SiteReader());
                    }
                    else if(relObj.Contains(obj.BldgReader()))
                    {
                        l50.Reference_Object = GetInfo(obj.BldgReader());
                    }
                    else
                        l50.Reference_Object = GetInfo(relObj.FirstOrDefault());
                }

                l50.Instance_Object = GetInfo(psetMap);

                //var prop = (psetMap.HasProperties.Where(p => p.Name == "Eastings").SingleOrDefault() as IIfcPropertySingleValue);
                //var propVal = prop.NominalValue;
                //var vall = propVal.Value;
                //
                //var sd = double.TryParse(vall.ToString(), out double asas);

                //check if XYZ has Values
                l50.Translation_Eastings = (!double.IsNaN(GetPropertyValueNo(psetMap, "Eastings"))) ? GetPropertyValueNo(psetMap, "Eastings") : double.NaN;
                l50.Translation_Northings = (!double.IsNaN(GetPropertyValueNo(psetMap, "Northings"))) ? GetPropertyValueNo(psetMap, "Northings") : double.NaN;
                l50.Translation_Orth_Height = (!double.IsNaN(GetPropertyValueNo(psetMap, "OrthogonalHeight"))) ? GetPropertyValueNo(psetMap, "OrthogonalHeight") : double.NaN;

                l50.RotationXY = new List<double>();

                if (!double.IsNaN(GetPropertyValueNo(psetMap, "XAxisAbscissa")) && !double.IsNaN(GetPropertyValueNo(psetMap, "XAxisOrdinate"))) 
                    //if omitted, values for no rotation (angle = 0) applied (consider difference to True North)
                {
                    l50.RotationXY.Add(GetPropertyValueNo(psetMap, "XAxisAbscissa"));
                    l50.RotationXY.Add(GetPropertyValueNo(psetMap, "XAxisOrdinate"));
                }
                else
                {
                    l50.RotationXY.Add(0);
                    l50.RotationXY.Add(1);
                };

                //check logic of coordinates and rotation
                if (((l50.Translation_Eastings > 0) && (l50.Translation_Northings > 0) && ((l50.Translation_Orth_Height >= 0) | double.IsNaN(l50.Translation_Orth_Height))) 
                    && 
                    (System.Math.Round(System.Math.Pow(l50.RotationXY[0], 2) + System.Math.Pow(l50.RotationXY[1], 2), 5) == 1))
                {
                    //by definition: ONLY in this case there could be an georeferencing
                    l50.GeoRef50 = true;
                }
                else
                {
                    l50.GeoRef50 = false;
                }

                l50.Scale = (!double.IsNaN(GetPropertyValueNo(psetMap, "Scale"))) ? GetPropertyValueNo(psetMap, "Scale") : 1;

                if (GetPropertyValueStr(psetCrs, "Name") != null && Regex.IsMatch(GetPropertyValueStr(psetCrs, "Name"), @"^EPSG:\s*[0-9]+$", RegexOptions.IgnoreCase))
                {
                    l50.CRS_Name = GetPropertyValueStr(psetCrs, "Name");
                }
                else
                {
                    l50.CRS_Name = "n/a";
                    l50.GeoRef50 = false;
                }

                l50.CRS_Description = (GetPropertyValueStr(psetCrs, "Description") != null) ? GetPropertyValueStr(psetCrs, "Description") : "n/a";
                l50.CRS_Geodetic_Datum = (GetPropertyValueStr(psetCrs, "GeodeticDatum") != null) ? GetPropertyValueStr(psetCrs, "GeodeticDatum") : "n/a";
                l50.CRS_Vertical_Datum = (GetPropertyValueStr(psetCrs, "VerticalDatum") != null) ? GetPropertyValueStr(psetCrs, "VerticalDatum") : "n/a";
                l50.CRS_Projection_Name = (GetPropertyValueStr(psetCrs, "MapProjection") != null) ? GetPropertyValueStr(psetCrs, "MapProjection") : "n/a";
                l50.CRS_Projection_Zone = (GetPropertyValueStr(psetCrs, "MapZone") != null) ? GetPropertyValueStr(psetCrs, "MapZone") : "n/a";

                Log.Information("GeoRefChecker: Reading Level 50 attributes successful.");
            }

            catch(Exception e)
            {
                MessageBox.Show("GeoRefChecker: Error occured while reading LoGeoRef50 attribute values. \r\nError message: " + e.Message);

                Log.Error("GeoRefChecker: Error occured while reading LoGeoRef50 attribute values. \r\nError message: " + e.Message);
            }

            return l50;
        }

        private double GetPropertyValueNo(IIfcPropertySet pset, string propName)
        {
            var prop = (pset.HasProperties.Where(p => p.Name == propName).SingleOrDefault() as IIfcPropertySingleValue);
            if (prop != null)
            {
                var propVal = prop.NominalValue.ToString();

                var val = double.TryParse(propVal, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal);
                //var val = double.TryParse(propVal, out double doubleVal);

                return doubleVal;
            }
            else
            {
                return double.NaN;
            }
        }

        private string GetPropertyValueStr(IIfcPropertySet pset, string propName)
        {
            var prop = (pset.HasProperties.Where(p => p.Name == propName).SingleOrDefault() as IIfcPropertySingleValue);
            if (prop != null)
            {
                var propVal = prop.NominalValue.ToString();

                return propVal;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Returns a list with required entity information (STEP-hash number, Entity type).
        /// For reference objects with own IFC-id (inherited from IfcRoot) also the id will be returned,
        /// </summary>
        private List<string> GetInfo(IPersistEntity entity)
        {
            var info = new List<string>
                    {
                        {"#" + entity.GetHashCode() },
                        {entity.GetType().Name },
                };

            if(entity is IIfcRoot)
            {
                info.Add((entity as IIfcRoot).GlobalId);
            }

            return info;
        }
    }
}