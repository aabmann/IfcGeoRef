﻿using System;
using System.Collections.Generic;
using System.Windows;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace IfcGeoRefChecker.Appl
{
    public class Level40
    {
        public bool GeoRef40 { get; set; }

        public IList<string> Reference_Object { get; set; }

        public IList<string> Instance_Object_WCS { get; set; }

        public IList<string> Instance_Object_North { get; set; }

        public IList<double> ProjectLocationXYZ { get; set; }

        public IList<double> ProjectRotationX { get; set; }

        public IList<double> ProjectRotationZ { get; set; }

        public IList<double> TrueNorthXY { get; set; }

        private PlacementXYZ plcm = new PlacementXYZ();

        private IIfcAxis2Placement3D plcm3D;

        //private IIfcDirection dir;

        private IIfcGeometricRepresentationContext prjCtx;

        private IfcStore model;

        //GeoRef 40: read the WorldCoordinateSystem and TrueNorth attribute of IfcGeometricRepresentationContext
        //-------------------------------------------------------------------------------------------------------

        public Level40(IfcStore model)
        {
            try
            {
                this.model = model;

                this.prjCtx = new ContextReader(model).ProjCtx;

                this.Reference_Object = new List<string>
                    {
                        {"#" + prjCtx.GetHashCode() },
                        {prjCtx.GetType().Name }
                    };

                //variable for the WorldCoordinatesystem attribute
                this.plcm3D = (IIfcAxis2Placement3D)prjCtx.WorldCoordinateSystem;

                this.Instance_Object_WCS = new List<string>();

                if(this.plcm3D != null)
                {
                    this.Instance_Object_WCS.Add("#" + this.plcm3D.GetHashCode());
                    this.Instance_Object_WCS.Add(this.plcm3D.GetType().Name);
                }
                else
                {
                    this.Instance_Object_WCS.Add("IfcAxis2Placment3D");
                    this.Instance_Object_WCS.Add("n/a");
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Error occured while checking for LoGeoRef40: \r\n" + e.Message + e.StackTrace);
            }
        }

        public void GetLevel40()
        {
            this.plcm.GetPlacementXYZ(this.plcm3D);

            this.GeoRef40 = this.plcm.GeoRefPlcm;
            this.ProjectLocationXYZ = this.plcm.LocationXYZ;
            this.ProjectRotationX = this.plcm.RotationX;
            this.ProjectRotationZ = this.plcm.RotationZ;

            //variable for the TrueNorth attribute
            var dir = prjCtx.TrueNorth;

            this.Instance_Object_North = new List<string>();
            this.TrueNorthXY = new List<double>();

            if(dir != null)
            {
                this.Instance_Object_North.Add("#" + dir.GetHashCode());
                this.Instance_Object_North.Add(dir.GetType().Name);

                this.TrueNorthXY.Add(dir.DirectionRatios[0]);
                this.TrueNorthXY.Add(dir.DirectionRatios[1]);
            }
            else
            {
                this.Instance_Object_North.Add("IfcDirection");
                this.Instance_Object_North.Add("n/a");

                //if omitted, default values (see IFC schema for IfcGeometricRepresentationContext):

                this.TrueNorthXY.Add(0);
                this.TrueNorthXY.Add(1);
            }
        }

        public void UpdateLevel40()
        {
            using(var txn = this.model.BeginTransaction(model.FileName + "_transedit"))
            {
                this.plcm.LocationXYZ = this.ProjectLocationXYZ;
                this.plcm.RotationX = this.ProjectRotationX;
                this.plcm.RotationZ = this.ProjectRotationZ;

                this.plcm.UpdatePlacementXYZ(model);

                var schema = model.IfcSchemaVersion.ToString();

                if(schema == "Ifc4")
                {
                    this.prjCtx.TrueNorth = model.Instances.New<Xbim.Ifc4.GeometryResource.IfcDirection>(d => d.SetXY(this.TrueNorthXY[0], this.TrueNorthXY[1]));
                }
                else if(schema == "Ifc2X3")
                {
                    this.prjCtx.TrueNorth = model.Instances.New<Xbim.Ifc2x3.GeometryResource.IfcDirection>(d => d.SetXY(this.TrueNorthXY[0], this.TrueNorthXY[1]));
                }

                txn.Commit();
            }

            model.SaveAs(model.FileName + "_edit");
        }

        public string LogOutput()
        {
            string logLevel40 = "";
            string line = "\r\n________________________________________________________________________________________________________________________________________";
            string dashline = "\r\n----------------------------------------------------------------------------------------------------------------------------------------";

            logLevel40 += "\r\n \r\nProject context attributes for georeferencing (Location: WorldCoordinateSystem / Rotation: TrueNorth)"
            + dashline + "\r\n Project context element:" + this.Reference_Object[0] + "=" + this.Reference_Object[1]
            + "\r\n Placement referenced in " + this.Instance_Object_WCS[0] + "=" + this.Instance_Object_WCS[1];

            logLevel40 += "\r\n  X = " + this.ProjectLocationXYZ[0] + "\r\n  Y = " + this.ProjectLocationXYZ[1] + "\r\n  Z = " + this.ProjectLocationXYZ[2];

            logLevel40 += $"\r\n  Rotation X-axis = ({this.ProjectRotationX[0]}/{this.ProjectRotationX[1]}/{this.ProjectRotationX[2]})";

            logLevel40 += $"\r\n  Rotation Z-axis = ({this.ProjectRotationZ[0]}/{this.ProjectRotationZ[1]}/{this.ProjectRotationZ[2]})";

            if(this.Instance_Object_North.Contains("n/a"))

            {
                logLevel40 += "\r\n \r\n No rotation regarding True North mentioned.";
            }
            else
            {
                logLevel40 += "\r\n \r\n True North referenced in " + this.Instance_Object_North[0] + "=" + this.Instance_Object_North[1]
                    + "\r\n  X-component =" + this.TrueNorthXY[0]
                    + "\r\n  Y-component =" + this.TrueNorthXY[1];
            }

            logLevel40 += "\r\n \r\n LoGeoRef 40 = " + this.GeoRef40 + "\r\n" + line;

            return logLevel40;
        }
    }
}