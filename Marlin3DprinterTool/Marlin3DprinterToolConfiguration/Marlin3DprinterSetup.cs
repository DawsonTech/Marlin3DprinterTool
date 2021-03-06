﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using AutoUpdater;
using Microsoft.Win32;


namespace Marlin3DprinterToolConfiguration
{
    public partial class Marlin3DprinterSetup : Form
    {
       
        private Version _currentVersion;

        public Version CurrentVersion
        {
            get { return _currentVersion; }
            set
            {
                _currentVersion = value;
                Text = $@"Marlin3DprinterTool Version: {_currentVersion}";

            }
        }

        public Marlin3DprinterSetup()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            Configuration.GetInstance.LicenseKey = txtBxUnlockKey.Text;
            Close();
        }


        private void ShowLicense()
        {
            if (String.IsNullOrEmpty(txtBxUnlockKey.Text)) return;

            try
            {
                txtBxShowLicence.Text = Configuration.GetInstance.Decrypt(txtBxUnlockKey.Text).Replace(";", Environment.NewLine);
            }
            catch (Exception e)
            {
                //MessageBox.Show($"Decryptation Error: {e.Message}");
            }
            
        }

        private void txtBxUnlockKey_TextChanged(object sender, EventArgs e)
        {
            ShowLicense();
        }

        private void btnPayPal_Click(object sender, EventArgs e)
        {
            const string url = "https://www.paypal.com/cgi-bin/webscr" +
                               @"?cmd=" + "_donations" +
                               @"&business=" + "cabbagecreek@gmail.com" +
                               @"&lc=" + "US" +
                               @"&item_name=" + "Marlin 3D printer Tool Donation" +
                               @"&notify_url=" + @"http://marlin3dprintertool.se/PayPal/PayPalIPN.php" +
                               @"&amount=5" +
                               @"&currency_code=" + "USD" +
                               @"&bn=" + "PP%2dDonationsBF";
            try
            {
                Process myProcess = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName = url
                    }
                };



                // true is the default, but it is important not to set it to false
                myProcess.Start();
            }
            catch (Exception webbroserException)
            {
                MessageBox.Show(@"Cant open the default webbrowser with connection to PayPal" + Environment.NewLine +
                                Environment.NewLine +
                                @"Reason: " + webbroserException.Message, @"Cant find default browser", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Clipboard.SetText(url);

                MessageBox.Show(@"The webaddress and parameters are copied to ClipBoard" + Environment.NewLine + Environment.NewLine +
                                @"Do the donation manually by paste it (ctrl+V) in your webbrowser address field", @"Manual PayPal donation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        








        private void btnDirectoryCurrentFirmware_Click(object sender, EventArgs e)
        {
            
            FolderBrowserDialog currentFirmwareBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = Configuration.GetInstance.CurrentFirmware,
                Description = @"Directory to current Firmware"
            };
            DialogResult result = currentFirmwareBrowserDialog.ShowDialog();
            if (result == DialogResult.OK) txtBxDirectoryCurrentFirmware.Text = currentFirmwareBrowserDialog.SelectedPath;
        }

        private void btnDirectoryNewFirmware_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog newFirmwareBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = Configuration.GetInstance.NewFirmware,
                Description = @"Directory to new Firmware"
            };
            DialogResult result = newFirmwareBrowserDialog.ShowDialog();
            if (result == DialogResult.OK) txtBxDirectoryNewFirmware.Text = newFirmwareBrowserDialog.SelectedPath;
        }

        private void btnArduinoIDE_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog arduinoIdeBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = Configuration.GetInstance.ArduinoIde,
                Description = @"Directory to Arduino IDE"
            };
            DialogResult result = arduinoIdeBrowserDialog.ShowDialog();
            if (result == DialogResult.OK) txtBxArduinoIDE.Text = arduinoIdeBrowserDialog.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Configuration.GetInstance.CurrentFirmware = txtBxDirectoryCurrentFirmware.Text;
            Configuration.GetInstance.NewFirmware = txtBxDirectoryNewFirmware.Text;
            Configuration.GetInstance.ArduinoIde = txtBxArduinoIDE.Text;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Marlin3DprinterSetup_Load(object sender, EventArgs e)
        {
            txtBxDirectoryCurrentFirmware.Text = Configuration.GetInstance.CurrentFirmware;
            txtBxDirectoryNewFirmware.Text = Configuration.GetInstance.NewFirmware;
            txtBxArduinoIDE.Text = Configuration.GetInstance.ArduinoIde;

            txtBxUnlockKey.Text = Configuration.GetInstance.LicenseKey;
            ShowLicense();

        }

        private void btnForceUpdate_Click(object sender, EventArgs e)
        {
            Updater autoUpdater = new Updater();
            autoUpdater.CurrentVersion = CurrentVersion;
            autoUpdater.ForceUpdate();
        }

        private void btnClearLicense_Click(object sender, EventArgs e)
        {
            txtBxUnlockKey.Text = "";
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public static class FileAssociation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="progId"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        /// <param name="application"></param>
        // Associate file extension with progID, description, icon and application
        public static void Associate(string extension,
            string progId, string description, string icon, string application)
        {
            RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey(extension);
            registryKey?.SetValue("", progId);


            if (string.IsNullOrEmpty(progId)) return;
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progId))
            {
                if (description != null)
                {
                    key?.SetValue("", description);
                }
                if (icon != null)
                {
                    var subKey = key?.CreateSubKey("DefaultIcon");
                    subKey?.SetValue("", ToShortPathName(icon));
                }
                if (application != null)
                {
                    var subKey = key?.CreateSubKey(@"Shell\Open\Command");
                    subKey?.SetValue("", ToShortPathName(application) + " \"%1\"");
                }
            }
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }


        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath,
            [Out] StringBuilder lpszShortPath, uint cchBuffer);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            GetShortPathName(longName, s, iSize);
            return s.ToString();
        }

        //    // Tell explorer the file association has been changed
        //    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
