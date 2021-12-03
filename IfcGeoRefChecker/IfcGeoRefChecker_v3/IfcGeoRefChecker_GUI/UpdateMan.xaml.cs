using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Serilog;
using IfcGeoRefChecker.Appl;
using IfcGeoRefChecker.IO;
using System.Text.RegularExpressions;

namespace IfcGeoRefChecker_GUI
{
    /// <summary>
    /// Interaktionslogik für UpdateMan.xaml
    /// </summary>
    public partial class UpdateMan : Window
    {
        private GeoRefChecker jsonMap;
        private string fileName;
        private string direc;

        public UpdateMan(GeoRefChecker jsonMap, string direc, string fileName)
        {
            this.direc = direc;
            this.fileName = fileName;
            this.jsonMap = jsonMap;

            InitializeComponent();

            GetCheckAttributes();
        }

        private void GetCheckAttributes()
        {
            // mehrere "try-Catch"-Blöcke --> Wenn einer nicht funktioniert, werden die anderen trotzdem noch durchgeführt
            // try-catch block for level 10 Attributes from building
            try
            {

                var lev10Bldg = (from l10Bldg in jsonMap.LoGeoRef10
                                 where l10Bldg.Reference_Object[1].Equals("IfcBuilding")
                                 select l10Bldg).Single();

                if (lev10Bldg.GeoRef10)
                {


                    if (lev10Bldg.AddressLines != null)
                    {
                        var k = lev10Bldg.AddressLines.Count;

                        switch (k)
                        {
                            case 1:
                                tb_adr0.Text = lev10Bldg.AddressLines[0];
                                break;

                            case 2:
                                tb_adr1.Text = lev10Bldg.AddressLines[1];
                                goto case 1;

                            case 3:
                                tb_adr2.Text = lev10Bldg.AddressLines[2];
                                goto case 2;
                        }
                    }

                    tb_plz.Text = lev10Bldg.Postalcode;
                    tb_town.Text = lev10Bldg.Town;
                    tb_region.Text = lev10Bldg.Region;
                    tb_country.Text = lev10Bldg.Country;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 10 building attributes to window. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 10 building attributes to window. Error: " + ex.Message);
            }

            // added for test purpose
            // try-catch block for level 10 Attributes from site
            try
            {
                var lev10Site = (from l10Site in jsonMap.LoGeoRef10
                                 where l10Site.Reference_Object[1].Equals("IfcSite")
                                 select l10Site).Single();
                if (lev10Site.GeoRef10)
                {

                    if (lev10Site.AddressLines != null)
                    {
                        var k = lev10Site.AddressLines.Count;

                        switch (k)
                        {
                            case 1:
                                tb_adr0.Text = lev10Site.AddressLines[0];
                                break;

                            case 2:
                                tb_adr1.Text = lev10Site.AddressLines[1];
                                goto case 1;

                            case 3:
                                tb_adr2.Text = lev10Site.AddressLines[2];
                                goto case 2;
                        }
                    }

                    tb_plz.Text = lev10Site.Postalcode;
                    tb_town.Text = lev10Site.Town;
                    tb_region.Text = lev10Site.Region;
                    tb_country.Text = lev10Site.Country;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 10 site attributes to window. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 10 site attributes to window. Error: " + ex.Message);
            }

            // try-catch block for level 20 Attributes from site
            try
            {
                //-------------

                var lev20site = (from l20site in jsonMap.LoGeoRef20
                                 where l20site.Reference_Object[1].Equals("IfcSite")
                                 select l20site).Single();
                if (lev20site.GeoRef20)
                {
                    
                    tb_lat.Text = lev20site.Latitude.ToString();

                    tb_lon.Text = lev20site.Longitude.ToString();

                    tb_OrthoHeight.Text = lev20site.Elevation.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 20 attributes to window. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 20 attributes to window. Error: " + ex.Message);
            }

            // try-catch block for level 40 Attributes from IfcProject
            try { 
                var convHelper = new Calc();

                var lev40proj = (from l40 in jsonMap.LoGeoRef40
                                 where l40.Reference_Object[1].Equals("IfcProject")
                                 select l40).Single();
                if (lev40proj.GeoRef40)
                {

                    var angleNorth = convHelper.GetAngleBetweenForXAxis(new System.Windows.Media.Media3D.Vector3D(lev40proj.TrueNorthXY[0], lev40proj.TrueNorthXY[1], 0));
                    tb_TrueNorth.Text = angleNorth.ToString();
                }

                //tb_elev.Text = lev20site.Elevation.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 40 attributes to window. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 40 attributes to window. Error: " + ex.Message);
            }

            // try-catch block for level 50 Attributes from IfcProject
            try
            {
                //------------------
                var convHelper = new Calc();
                var lev50proj = (from l50 in jsonMap.LoGeoRef50
                                 where l50.Reference_Object[1].Equals("IfcProject")
                                 select l50).Single();
                if (lev50proj.GeoRef50)
                {

                    string eastingComplete = lev50proj.Translation_Eastings.ToString();
                    tb_Zone.Text = eastingComplete.Substring(0, 2);
                    tb_eastings50.Text = eastingComplete.Substring(2);
                    //tb_eastings50.Text = lev50proj.Translation_Eastings.ToString();
                    tb_northings50.Text = lev50proj.Translation_Northings.ToString();
                    //tb_height50.Text = lev50proj.Translation_Orth_Height.ToString();

                    tb_scale50.Text = lev50proj.Scale.ToString();

                    tb_CRSname50.Text = Regex.Replace(lev50proj.CRS_Name, @"^EPSG:\s*", "", RegexOptions.IgnoreCase);

                    var angle = convHelper.GetAngleBetweenForXAxis(new System.Windows.Media.Media3D.Vector3D(lev50proj.RotationXY[0], lev50proj.RotationXY[1], 0));

                    tb_rotation50.Text = angle.ToString();

                    Log.Information("GeoRefUpdater: Write attributes to window successful.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 50 attributes to window. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 50 attributes to window. Error: " + ex.Message);
            }
        }

        private void bt_updateJsonMan_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (
                    (from l10 in jsonMap.LoGeoRef10 where l10.Reference_Object[1].Equals("IfcBuilding") select l10.Reference_Object[1]).Single().Equals("IfcBuilding")
                    &&
                    ((from l10 in jsonMap.LoGeoRef10 where l10.Reference_Object[1].Equals("IfcBuilding") select l10.GeoRef10).Single()
                    |
                    !(from l10 in jsonMap.LoGeoRef10 where l10.Reference_Object[1].Equals("IfcSite") select l10.GeoRef10).Single())
                    )
                // if (IFCBuilding – LoGeoRef 10 == false & IFCSite – LoGeoRef 10 == true) - new Address for IFCSite
                // if (IFCBuilding – LoGeoRef 10 == false & IFCSite – LoGeoRef 10 == false) - generate new Address for IFCBuilding on update
                // if (IFCBuilding – LoGeoRef 10 == true & IFCSite – LoGeoRef 10 == true) - both, IFCSite and IFCBuilding get the updated address
                // if (IFCBuilding – LoGeoRef 10 == true & IFCSite – LoGeoRef 10 == false) - new Address for IFCBuilding
                {
                    var lev10 = (from l10Site in jsonMap.LoGeoRef10
                                 where l10Site.Reference_Object[1].Equals("IfcBuilding")
                                 select l10Site).Single();

                    lev10.AddressLines.Clear();

                    if (!tb_adr0.Text.Equals(""))
                        lev10.AddressLines.Add(tb_adr0.Text);

                    if (!tb_adr1.Text.Equals(""))
                        lev10.AddressLines.Add(tb_adr1.Text);

                    if (!tb_adr2.Text.Equals(""))
                        lev10.AddressLines.Add(tb_adr2.Text);

                    lev10.Country = tb_country.Text;
                    lev10.Region = tb_region.Text;
                    lev10.Postalcode = tb_plz.Text;
                    lev10.Town = tb_town.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 10 IfcBuilding Attribute updates to update.json. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 10 IfcBuilding Attribute updates to update.json. Error: " + ex.Message);
            }
            try
            {
                if (
                    (from l10 in jsonMap.LoGeoRef10 where l10.Reference_Object[1].Equals("IfcSite") select l10.Reference_Object[1]).Single().Equals("IfcSite")
                    &&
                    ((from l10 in jsonMap.LoGeoRef10 where l10.Reference_Object[1].Equals("IfcSite") select l10.GeoRef10).Single()
                    |
                    !(from l10 in jsonMap.LoGeoRef10 select l10.Reference_Object[1]).Contains("IfcBuilding"))
                    // if there is no Instance of IFCBuilding it uses IFCSite for the new Address, whether or not IFCSite - LoGeoRef 10 == true
                    )
                {
                    var lev10 = (from l10Bldg in jsonMap.LoGeoRef10
                                    where l10Bldg.Reference_Object[1].Equals("IfcSite")
                                    select l10Bldg).Single();

                    lev10.AddressLines.Clear();

                    if (!tb_adr0.Text.Equals(""))
                        lev10.AddressLines.Add(tb_adr0.Text);

                    if (!tb_adr1.Text.Equals(""))
                        lev10.AddressLines.Add(tb_adr1.Text);

                    if (!tb_adr2.Text.Equals(""))
                        lev10.AddressLines.Add(tb_adr2.Text);

                    lev10.Country = tb_country.Text;
                    lev10.Region = tb_region.Text;
                    lev10.Postalcode = tb_plz.Text;
                    lev10.Town = tb_town.Text;

                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 10 IfcSite Attribute updates to update.json. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 10 IfcSite Attribute updates to update.json. Error: " + ex.Message);
            }

            //------------------------------------------
            try
            {
                var convHelper = new Calc();

                var lev20site = (from l20site in jsonMap.LoGeoRef20
                                 where l20site.Reference_Object[1].Equals("IfcSite")
                                 select l20site).Single();

                lev20site.Latitude = convHelper.ParseDouble(tb_lat.Text);
                lev20site.Longitude = convHelper.ParseDouble(tb_lon.Text);
                lev20site.Elevation = convHelper.ParseDouble(tb_OrthoHeight.Text);
                //lev20site.Latitude = Double.Parse(tb_lat.Text);
                //    lev20site.Longitude = Double.Parse(tb_lon.Text);
                //    lev20site.Elevation = Double.Parse(tb_OrthoHeight.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 20 Attribute updates to update.json. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 20 Attribute updates to update.json. Error: " + ex.Message);
            }

            //------------------------------------------
            try
            {

                var convHelper = new Calc();
                var lev40proj = (from l40 in jsonMap.LoGeoRef40
                                 where l40.Reference_Object[1].Equals("IfcProject")
                                 select l40).Single();

                if (!tb_TrueNorth.Text.Equals(""))
                {
                    //var rot50 = convHelper.ParseDouble(tb_rotation50.Text);
                    //var rot40 = Double.Parse(tb_TrueNorth.Text);
                    var rot40 = convHelper.ParseDouble(tb_TrueNorth.Text);
                    var vector = convHelper.GetVector3DForXAxis(rot40);

                    lev40proj.TrueNorthXY = new List<double>();

                    lev40proj.TrueNorthXY = new List<double>();

                    lev40proj.TrueNorthXY.Add(vector.X);
                    lev40proj.TrueNorthXY.Add(vector.Y);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GeoRefUpdater: Error occured while writing level 40 Attribute updates to update.json. Error: " + ex.Message);
                Log.Error("GeoRefUpdater: Error occured while writing level 40 Attribute updates to update.json. Error: " + ex.Message);
            }
            //------------------------------------------
            try {
                var convHelper = new Calc();
                var lev50proj = (from l50 in jsonMap.LoGeoRef50
                                 where l50.Reference_Object[1].Equals("IfcProject")
                                 select l50).Single();

                lev50proj.Translation_Eastings = convHelper.ParseDouble(tb_Zone.Text + tb_eastings50.Text);
                lev50proj.Translation_Northings = convHelper.ParseDouble(tb_northings50.Text);
                lev50proj.Translation_Orth_Height = convHelper.ParseDouble(tb_OrthoHeight.Text);
                lev50proj.Scale = convHelper.ParseDouble(tb_scale50.Text);

                //lev50proj.Translation_Eastings = Double.Parse(tb_Zone.Text + tb_eastings50.Text);
                //    lev50proj.Translation_Northings = Double.Parse(tb_northings50.Text);
                //    lev50proj.Translation_Orth_Height = Double.Parse(tb_OrthoHeight.Text);
                //    lev50proj.Scale = Double.Parse(tb_scale50.Text);

                if (Regex.IsMatch(tb_CRSname50.Text, "[0-9]"))
                {
                    lev50proj.CRS_Name = Regex.IsMatch(tb_CRSname50.Text, @"^EPSG:\s*[0-9]+$", RegexOptions.IgnoreCase) ? tb_CRSname50.Text : "EPSG:" + tb_CRSname50.Text;
                }
                else
                {
                    lev50proj.CRS_Name = "n/a";
                }

               

                if (!tb_rotation50.Text.Equals(""))
                {
                    var rot50 = convHelper.ParseDouble(tb_rotation50.Text);
                    //var rot50 = Double.Parse(tb_rotation50.Text);
                    var vector = convHelper.GetVector3DForXAxis(rot50);
                
                    lev50proj.RotationXY = new List<double>();

                    lev50proj.RotationXY = new List<double>();

                    lev50proj.RotationXY.Add(vector.X);
                    lev50proj.RotationXY.Add(vector.Y);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter valid numbers in textboxes that require numbers (like Elevation, Latidude, Longitude ...)");
                Log.Error("GeoRefUpdater: Error occured while writing level 50 Attribute updates to update.json. Error: " + ex.Message);
            }
            try
            {
                var write = new JsonOutput();
                write.JsonOutputDialog(this.jsonMap, this.direc, this.fileName + "update");

                Log.Information("GeoRefUpdater: Write updates to update.json file was successful.");
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }

        private void bt_quit_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("GeoRefUpdater: Closed without Saving.");
            this.Close();
        }

        private void Bt_Calculate_Click(object sender, RoutedEventArgs e)
        {
            bool isPosGeo = cb_PosGeo.IsChecked.Value;
            double lat = double.NaN, lon = double.NaN;
            double east = double.NaN, north = double.NaN;
            int? zone = null;
            var isSouth = cb_isSouth.IsChecked.Value;

            try
            {
                if (isPosGeo)
                {
                    lat = Calculations.StringToDeg(tb_lat.Text, false);
                    lon = Calculations.StringToDeg(tb_lon.Text, false);
                    //zone = chkPosForceZone.Checked ? Calculations.ParseInt(tbPosZone.Text) : (int?)null;
                    zone = (int?)null;
                }
                else
                {
                    east = Calculations.ParseDouble(tb_eastings50.Text);
                    north = Calculations.ParseDouble(tb_northings50.Text);
                    zone = Calculations.ParseInt(tb_Zone.Text);
                }

                double orthoHeight = double.NaN;
                orthoHeight = Calculations.ParseDouble(tb_OrthoHeight.Text);

                bool isRotGeo = cb_RotGeo.IsChecked.Value;
                double geoAzi = double.NaN, gridAzi = double.NaN;
                if (isRotGeo)
                {
                    geoAzi = Calculations.ParseDouble(tb_TrueNorth.Text);
                }
                else
                {
                    gridAzi = Calculations.ParseDouble(tb_rotation50.Text);
                }
                Calculations.GetGeoRef(isPosGeo, ref lat, ref lon, ref zone, ref east, ref north, ref isSouth, orthoHeight, isRotGeo, ref geoAzi, ref gridAzi, out var combinedScale);

                tb_lat.Text = Calculations.DegToString(lat, false);
                tb_lon.Text = Calculations.DegToString(lon, false);
                tb_Zone.Text = (zone ?? int.MaxValue).ToString();
                tb_eastings50.Text = Calculations.DoubleToString(east, 4);
                tb_northings50.Text = Calculations.DoubleToString(north, 4);
                cb_isSouth.IsChecked = isSouth;
                tb_OrthoHeight.Text = Calculations.DoubleToString(orthoHeight, 4);
                var loc = Calculations.AzimuthToLocalVector(geoAzi);
                var grid = Calculations.AzimuthToVector(gridAzi);
                tb_scale50.Text = Calculations.DoubleToString(combinedScale, 9);
                tb_TrueNorth.Text = Calculations.DoubleToString(geoAzi, 9);
                //tbRotGeoVecX.Text = Calculations.DoubleToString(loc[0], 9);
                //tbRotGeoVecY.Text = Calculations.DoubleToString(loc[1], 9);
                tb_rotation50.Text = Calculations.DoubleToString(gridAzi, 9);
                //tbRotGridVecE.Text = Calculations.DoubleToString(grid[0], 9);
                //tbRotGridVecN.Text = Calculations.DoubleToString(grid[1], 9);

                MessageBox.Show("Keep in mind: The calculation only transforms between geographical coordinates (latitude, longitude, orthogonal height) and UTM Projection on CRS ETRS89.");
            }
            catch
            {

            }

            

        }

        private void Cb_PosGeo_Checked(object sender, RoutedEventArgs e)
        {
            tb_lat.IsEnabled = true;
            tb_lon.IsEnabled = true;

            tb_eastings50.IsEnabled = false;
            tb_northings50.IsEnabled = false;
            tb_Zone.IsEnabled = false;

            cb_PosProj.IsChecked = false;
        }

        private void Cb_RotGeo_Checked(object sender, RoutedEventArgs e)
        {
            tb_TrueNorth.IsEnabled = true;

            tb_rotation50.IsEnabled = false;

            cb_RotProj.IsChecked = false;
        }

        private void Cb_PosProj_Checked(object sender, RoutedEventArgs e)
        {
            tb_lat.IsEnabled = false;
            tb_lon.IsEnabled = false;

            tb_eastings50.IsEnabled = true;
            tb_northings50.IsEnabled = true;
            tb_Zone.IsEnabled = true;

            cb_PosGeo.IsChecked = false;
        }

        private void Cb_RotProj_Checked(object sender, RoutedEventArgs e)
        {
            tb_TrueNorth.IsEnabled = false;

            tb_rotation50.IsEnabled = true;

            cb_RotGeo.IsChecked = false;
        }
    }
}
