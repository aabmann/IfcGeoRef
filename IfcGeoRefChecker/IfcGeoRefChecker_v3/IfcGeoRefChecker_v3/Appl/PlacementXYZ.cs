using System.Collections.Generic;
using Xbim.Ifc4.Interfaces;

namespace IfcGeoRefChecker.Appl
{
    internal class PlacementXYZ
    {
        public IList<double> LocationXYZ { get; set; }

        public IList<double> RotationX { get; set; }

        public IList<double> RotationZ { get; set; }

        public IList<double> RotationXY { get; set; }

        public bool GeoRefPlcm { get; set; }

        //private IIfcAxis2Placement3D plcm3D;
        //private IIfcAxis2Placement2D plcm2D;

        //LoGeoRef20
        public PlacementXYZ(IIfcSite site)
        {

            if (site.RefLatitude.HasValue && site.RefLongitude.HasValue)
            {
                this.LocationXYZ = new List<double>
                {
                    site.RefLatitude.Value.AsDouble,
                    site.RefLongitude.Value.AsDouble,
                };
                
                if (System.Math.Abs(site.RefLatitude.Value.AsDouble) <= 90 && System.Math.Abs(site.RefLongitude.Value.AsDouble) <= 180)
                {
                    this.GeoRefPlcm = true;
                }
                else
                {
                    this.GeoRefPlcm = false;
                }
                
            }
            else
            {
                this.GeoRefPlcm = false;
            }

            if (site.RefElevation.HasValue)
            {
                this.LocationXYZ.Add(site.RefElevation.Value);// IfcLengthMeasure: Usually measured in millimeters (mm).
            }
        }

        //LoGeoRef30 & LoGeoRef40
        public PlacementXYZ(IIfcAxis2Placement plcm)
        {
            if(plcm is IIfcAxis2Placement3D)
            {
                var plcm3D = (IIfcAxis2Placement3D)plcm;

                this.LocationXYZ = new List<double> //must be given, if IfcAxis2Placment3D exists
                {
                    plcm3D.Location.X,
                    plcm3D.Location.Y,
                    plcm3D.Location.Z,
                };

                this.RotationX = new List<double>();

                if(plcm3D.RefDirection != null)

                {
                    this.RotationX.Add(plcm3D.RefDirection.DirectionRatios[0]);
                    this.RotationX.Add(plcm3D.RefDirection.DirectionRatios[1]);
                    this.RotationX.Add(plcm3D.RefDirection.DirectionRatios[2]);
                }
                else  //if omitted, default values (see IFC schema for IfcAxis2Placment3D)
                {
                    this.RotationX.Add(1);
                    this.RotationX.Add(0);
                    this.RotationX.Add(0);
                }

                this.RotationZ = new List<double>();

                if(plcm3D.Axis != null)

                {
                    this.RotationZ.Add(plcm3D.Axis.DirectionRatios[0]);
                    this.RotationZ.Add(plcm3D.Axis.DirectionRatios[1]);
                    this.RotationZ.Add(plcm3D.Axis.DirectionRatios[2]);
                }
                else  //if omitted, default values (see IFC schema for IfcAxis2Placment3D)
                {
                    this.RotationZ.Add(0);
                    this.RotationZ.Add(0);
                    this.RotationZ.Add(1);
                }

                if((plcm3D.Location.X > 0) && (plcm3D.Location.Y > 0) && (plcm3D.Location.Z >= 0))
                {
                    //by definition: ONLY in this case there could be an georeferencing
                    this.GeoRefPlcm = true;
                }
                else
                {
                    this.GeoRefPlcm = false;
                }
            }

            if(plcm is IIfcAxis2Placement2D)
            {
                var plcm2D = (IIfcAxis2Placement2D)plcm;

                this.LocationXYZ = new List<double> //must be given, if IfcAxis2Placment2D exists
                {
                    plcm2D.Location.X,
                    plcm2D.Location.Y,
                };

                this.RotationX = new List<double>();

                if(plcm2D.RefDirection != null)

                {
                    this.RotationX.Add(plcm2D.RefDirection.DirectionRatios[0]);
                    this.RotationX.Add(plcm2D.RefDirection.DirectionRatios[1]);
                }
                else  //if omitted, default values (see IFC schema for IfcAxis2Placment2D)
                {
                    this.RotationX.Add(1);
                    this.RotationX.Add(0);
                }

                if((plcm2D.Location.X > 0) && (plcm2D.Location.Y > 0))
                {
                    //by definition: ONLY in this case there could be an georeferencing
                    this.GeoRefPlcm = true;
                }
                else
                {
                    this.GeoRefPlcm = false;
                }
            }
        }

        // LoGeoRef50 - ifc4
        public PlacementXYZ(IIfcMapConversion mapc)
        {
            this.LocationXYZ = new List<double> //must be given, if IfcMapConversion exists
            {
            mapc.Eastings,
            mapc.Northings,
            mapc.OrthogonalHeight,
            };

            this.RotationXY = new List<double>();

            if (mapc.XAxisAbscissa != null && mapc.XAxisOrdinate != null)

            {
                this.RotationXY.Add(mapc.XAxisAbscissa.Value);
                this.RotationXY.Add(mapc.XAxisOrdinate.Value);
            }
            else  //if omitted, values for no rotation (angle = 0) applied (consider difference to True North)
            {
                this.RotationXY.Add(0);
                this.RotationXY.Add(1);
            }

            if (((mapc.Eastings > 0) && (mapc.Northings > 0) && ((mapc.OrthogonalHeight >= 0) | mapc.OrthogonalHeight == null))
                &&
                (System.Math.Round(System.Math.Pow(mapc.XAxisAbscissa.Value, 2) + System.Math.Pow(mapc.XAxisOrdinate.Value, 2),10) == 1))
            {
                //by definition: ONLY in this case there could be an georeferencing
                this.GeoRefPlcm = true;

            }
            else
            {
                this.GeoRefPlcm = false;
            }
         
            
        }
        
    }
}