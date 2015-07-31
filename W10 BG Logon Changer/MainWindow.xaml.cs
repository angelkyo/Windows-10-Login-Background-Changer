﻿using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Windows;
using System.Windows.Media.Imaging;
using HelperLibrary;
using MahApps.Metro.Controls;
using W10_BG_Logon_Changer.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace W10_BG_Logon_Changer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private string _selectedFile;

        public string SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                WallpaperViewer.Source = new BitmapImage(new Uri(value));
                _selectedFile = value;
            }
        }

        private readonly string _tempPriFile = Path.Combine(Path.GetTempPath(), "bak_temp_pri.pri");

        private readonly string _newPriLocation = Path.Combine(Path.GetTempPath(), "new_temp_pri.pri");

        public MainWindow()
        {
            InitializeComponent();
            SettingFlyout.Content = new BGEditorControl(this);
            SettingFlyout.IsOpen = true;

            HelperLib.TakeOwnership(Config.LogonFolder);
            HelperLib.TakeOwnership(Config.PriFileLocation);

            if (!File.Exists(Config.BakPriFileLocation))
            {
                Debug.WriteLine("Orginal file didn't exist D:");
                File.Copy(Config.PriFileLocation, Config.BakPriFileLocation);
            }

            HelperLib.TakeOwnership(Config.BakPriFileLocation);

            File.Copy(Config.BakPriFileLocation, _tempPriFile, true);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingFlyout.IsOpen = !SettingFlyout.IsOpen;
        }

        public void ApplyChanges()
        {
            File.Copy(Config.BakPriFileLocation, _tempPriFile, true);

            byte[] ps1File = Properties.Resources.CLW_Script;
            string file = string.Empty;
            
            using (var stream = new StreamReader(new MemoryStream(ps1File)))
                file = stream.ReadToEnd();

            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(file);
                var pars = new Dictionary<string, string>
                {
                    {"p1", _tempPriFile},
                    {"p2", _newPriLocation},
                    {"p3", SelectedFile}
                };

                PowerShellInstance.AddParameters(pars);

                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // loop through each output object item
                foreach (PSObject outputItem in PSOutput)
                {
                    // if null object was dumped to the pipeline during the script then a null
                    // object may be present here. check for null to prevent potential NRE.
                    if (outputItem != null)
                    {
                        //TODO: do something with the output item 
                        // outputItem.BaseOBject
                        Debug.WriteLine(outputItem.BaseObject);
                    }
                }
            }

            File.Copy(_newPriLocation, Config.PriFileLocation, true);
        }
    }
}
