using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace DemoCutterGUI
{
    public partial class CombineCutter : Window
    {

        string currentlyActiveProjectFile = null;

        public class ProjectSaveFileData
        {
            public List<DemoLinePoint> pointList { get; init; } = new List<DemoLinePoint>();
            public List<Demo> demoList { get; init; } = new List<Demo>();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(false);
        }
        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(true);
        }
        
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "json";
            ofd.Filter = "JSON Project files (*.json)|*.json|All files (*.*)|*.*";
            if(ofd.ShowDialog() != true)
            {
                return;
            }
            string jsonData = File.ReadAllText(ofd.FileName);

            JsonSerializerOptions opts = new JsonSerializerOptions();
            opts.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            ProjectSaveFileData deSerialized = JsonSerializer.Deserialize<ProjectSaveFileData>(jsonData, opts);
            if(deSerialized == null)
            {
                MessageBox.Show("Loading failed.");
                return;
            }

            currentlyActiveProjectFile = ofd.FileName;

            points.Clear();
            demos.Clear();
            foreach(DemoLinePoint point in deSerialized.pointList)
            {
                points.addPoint(point);
            }

            Demo last = null;
            foreach(Demo demo in deSerialized.demoList)
            {
                demos.Add(demo, last);
                last = demo;
            }

        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            currentlyActiveProjectFile = null;
            points.Clear();
            demos.Clear();
        }

        private void SaveProject(bool forceFileDialogue = false)
        {

            string saveFile = currentlyActiveProjectFile;
            if(saveFile == null || forceFileDialogue)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "json";
                sfd.Filter = "JSON Project files (*.json)|*.json|All files (*.*)|*.*";
                if(sfd.ShowDialog() != true)
                {
                    return;
                }
                saveFile = sfd.FileName;
            }

            ProjectSaveFileData saveFileData = new ProjectSaveFileData();
            points.Foreach((in DemoLinePoint p) => {
                saveFileData.pointList.Add(p);
            });
            demos.Foreach((in Demo p) => {
                saveFileData.demoList.Add(p);
            });

            JsonSerializerOptions opts = new JsonSerializerOptions();
            opts.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            opts.WriteIndented = true;
            string saveFileDataJson = JsonSerializer.Serialize(saveFileData, opts);
            File.WriteAllText(saveFile, saveFileDataJson);

            currentlyActiveProjectFile = saveFile;


        }


        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Pick a base filename for the export. Multiple files will be created.";
            sfd.AddExtension = true;
            sfd.DefaultExt = "dummy";
            sfd.Filter = "Dummy extension (*.dummy)|*.dummy|All files (*.*)|*.*";
            if(sfd.ShowDialog() == true)
            {
                string baseFilePath = Path.Combine(Path.GetDirectoryName(sfd.FileName), Path.GetFileNameWithoutExtension(sfd.FileName));
                Export(baseFilePath);
            }
        }

        private void Export(string filenameWithoutExtension)
        {
            StringBuilder sb = new StringBuilder();
            // Capture
            sb.Append("<capture>\n");
            sb.Append("\t<start>0</start>\n");
            sb.Append("\t<end>0</end>\n");
            sb.Append("\t<speed>1.00</speed>\n");
            sb.Append("\t<view>chase</view>\n");
            sb.Append("\t<view>chase</view>\n");
            sb.Append("</capture>\n");

            // Camera
            sb.Append("<camera>\n");
            sb.Append("\t<smoothPos>2</smoothPos>\n");
            sb.Append("\t<smoothAngles>1</smoothAngles>\n");
            sb.Append("\t<locked>0</locked>\n");
            sb.Append("\t<target>-1</target>\n");
            sb.Append("\t<flags>3</flags>\n");
            sb.Append("</camera>\n");

            // Chase
            sb.Append("<chase>\n");
            sb.Append("\t<locked>0</locked>\n");
            sb.Append("</chase>\n");

            // Line (timeline)
            sb.Append("<line>\n");
            sb.Append("\t<offset>0</offset>\n");
            sb.Append("\t<speed>1.00</speed>\n");
            sb.Append("\t<locked>1</locked>\n");

            points.Foreach((in DemoLinePoint point)=> {

                sb.Append("\t<point>\n");
                sb.Append($"\t\t<time>{point.time}</time>\n");
                sb.Append($"\t\t<demotime>{point.demoTime}</demotime>\n");
                sb.Append("\t</point>\n");
            });

            sb.Append("</line>\n");

            // DOF
            sb.Append("<dof>\n");
            sb.Append("\t<locked>0</locked>\n");
            sb.Append("\t<target>-1</target>\n");
            sb.Append("</dof>\n");

            // Weather
            sb.Append("<weather>\n");
            sb.Append("\t<sun>\n");
            sb.Append("\t\t<active>0</active>\n");
            sb.Append("\t\t<size>1.0000</size>\n");
            sb.Append("\t\t<precision>10.0000</precision>\n");
            sb.Append("\t\t<yaw>45.0000</yaw>\n");
            sb.Append("\t\t<pitch>45.0000</pitch>\n");
            sb.Append("\t</sun>\n");
            sb.Append("\t<rain>\n");
            sb.Append("\t\t<active>0</active>\n");
            sb.Append("\t\t<number>100</number>\n");
            sb.Append("\t\t<range>1000.0000</range>\n");
            sb.Append("\t\t<back>0</back>\n");
            sb.Append("\t</rain>\n");
            sb.Append("</weather>\n");


            string mmeProject = sb.ToString();
            sb.Clear();


            int demoIndex = 0;
            demos.Foreach((in Demo demo)=> {
                sb.Append($"[demo{demoIndex++}]\n");
                sb.Append($"path={demo.name}\n");
                sb.Append("players=0\n"); // hmm. Gotta just manually edit one way or another ig.
                sb.Append("allPlayers=1\n");
                float delay = points.lineAtSimple(demo.highlightDemoTime)-(float)demo.highlightOffset;
                string delayString = delay.ToString("#.####", CultureInfo.InvariantCulture);
                sb.Append($"delay={delayString}\n\n");
            });

            string combinerProject = sb.ToString();
            sb.Clear();

            sb.Append($"DemoCombiner.exe {filenameWithoutExtension}_combined.dm_15 {filenameWithoutExtension}_combinerConfig.ini");
            sb.Append("pause");

            string combinerBatch = sb.ToString();
            sb.Clear();

            string mmeFile = $"{filenameWithoutExtension}_mme.cfg";
            string combineBatchFile = $"{filenameWithoutExtension}_combine.bat";
            string combinerProjectFile = $"{filenameWithoutExtension}_combinerConfig.ini";

            if (File.Exists(mmeFile))
            {
                if (MessageBox.Show($"{mmeFile} already exists. Overwrite?", "Overwrite file?", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            if (File.Exists(combineBatchFile))
            {
                if (MessageBox.Show($"{combineBatchFile} already exists. Overwrite?", "Overwrite file?", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            if (File.Exists(combinerProjectFile))
            {
                if (MessageBox.Show($"{combinerProjectFile} already exists. Overwrite?", "Overwrite file?", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            File.WriteAllText(mmeFile, mmeProject);
            File.WriteAllText(combineBatchFile, combinerBatch);
            File.WriteAllText(combinerProjectFile, combinerProject);
        }
    }
}
