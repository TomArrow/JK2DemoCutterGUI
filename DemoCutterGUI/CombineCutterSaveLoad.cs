using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
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
            string saveFileDataJson = JsonSerializer.Serialize(saveFileData, opts);
            File.WriteAllText(saveFile, saveFileDataJson);


        }
    }
}
