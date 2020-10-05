using GOES.Models;
using GOES.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GOES
{    
    public partial class MainForm : Form
    {
        private string _imageSavePath;
        private string _currentImageName;
        private string _settingsFile;        
        private string _imageSourcesFile;
        private Settings _settings;
        private List<SourceImages> _imageSources;

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public MainForm(bool startMinimized)
        {
            InitializeComponent();

            if (startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
                Form1_Resize(null, null);
            }

            try
            {
                // Image sources location
                this._imageSourcesFile = "sources.json";                

                // Settings
                this._settingsFile = "settings.json";

                // Image save locations
                this._imageSavePath = AppDomain.CurrentDomain.BaseDirectory + "images\\";

                // Load up combo box data
                this.LoadImageSources();
                this.LoadUpdateIntervals();                
                
                // Check if save directory exists, create if not
                if (!Directory.Exists(_imageSavePath))
                {
                    Directory.CreateDirectory(this._imageSavePath);
                }
                
                // Check is settings file exists, create if not
                if (!File.Exists(_settingsFile))
                {
                    // Create default settings
                    this._settings = new Settings();
                    this._settings.ImageSource = this._imageSources[0].url.full;
                    this._settings.UpdateInterval = IntervalUtils.INTERVAL_5_MINUTES;
                    this._settings.KeepOldImages = false;
                    this._settings.SetAsWallpaper = false;
                    this._settings.RunOnStartup = false;

                    // File the settings out
                    SettingsFileUtils.SaveSettings(this._settings, this._settingsFile);

                    // Set controls default values
                    comboBoxImageSource.SelectedIndex = 0;
                    comboBoxInterval.SelectedIndex = 0;
                }
                else
                {
                    // Read settings from file
                    this._settings = SettingsFileUtils.LoadSettings(this._settingsFile);

                    // Set control values
                    comboBoxImageSource.SelectedIndex = comboBoxImageSource.FindStringExact((this._imageSources.First(x => x.url.full == this._settings.ImageSource).name));
                    comboBoxInterval.SelectedIndex = comboBoxInterval.FindStringExact(SettingsFileUtils.ConvertIntervalFromSettingsFile(this._settings.UpdateInterval));
                    checkBoxKeepOldImages.Checked = this._settings.KeepOldImages;
                    checkBoxSetWallpaper.Checked = this._settings.SetAsWallpaper;
                    //checkBoxStartAutomatically.Checked = this._settings.RunOnStartup;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("An unexpected error has occured! The app will now close.\n\n" + e);
                Application.Exit();
            }
        }       

        private void LoadImageSources()
        {
            var sourceJson = File.ReadAllText(this._imageSourcesFile);
            this._imageSources = JsonConvert.DeserializeObject<List<SourceImages>>(sourceJson);

            foreach (var source in this._imageSources)
            {
                comboBoxImageSource.Items.Add(source.name);
            }            
        }

        private void LoadUpdateIntervals()
        {
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_1_MINUTE_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_5_MINUTES_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_30_MINUTES_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_1_HOUR_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_3_HOURS_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_6_HOURS_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_12_HOURS_STRING);
            comboBoxInterval.Items.Add(IntervalUtils.INTERVAL_24_HOURS_STRING);
        }        

        private void UpdateStatus(string status)
        {
            toolStripStatusLabel3.Text = status;
        }

        #region Download and events
        private void InitTimer()
        {
            // Stop and disable timer if running
            timer1.Stop();                
            timer1.Enabled = false;
            
            // Setup interval and tick event
            timer1.Interval = Convert.ToInt32(this._settings.UpdateInterval) * 60 * 1000;
            timer1.Tick += new EventHandler(Timer1_Tick);

            // Enable timer
            timer1.Enabled = true;
            
            DateTime future = DateTime.Now;

            int updateInterval = Convert.ToInt32(this._settings.UpdateInterval);

            if (updateInterval < 60)
            {
                future = future.AddMinutes(updateInterval);
            }
            else
            {
                future = future.AddHours(updateInterval);
            }
            

            UpdateStatus("Next update at: " + future.ToString());
        }
        private void DownloadLatestImage()
        {            
            this._currentImageName = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".jpg";

            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);                
                wc.DownloadFileAsync(new Uri(_settings.ImageSource), this._imageSavePath + this._currentImageName);
            }
        }
        
        private async void UpdateWallpaper()
        {
            await Task.Run(() =>
            { 
                // Make sure wallpaper is set to fit
                float aspect = this._imageSources.First(x => x.url.full == this._settings.ImageSource).aspect;
                int wallpaperStyle = (aspect == 1 ? ImageUtils.WALLPAPER_STYLE_FIT : ImageUtils.WALLPAPER_STYLE_FILL);

                // Update the registry
                var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                key.SetValue(@"WallpaperStyle", wallpaperStyle.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());

                // Change the wallpaper
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, this._imageSavePath + this._currentImageName, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            });
        }
        
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            UpdateStatus("Last download: " + DateTime.Now.ToString());
            
            if (checkBoxSetWallpaper.Checked)
            {
                UpdateWallpaper();                
            }        
            
            if (!checkBoxKeepOldImages.Checked)
            {
                DeleteOldWallpaper();
            }
        }        

        private void DeleteOldWallpaper()
        {            
            string[] files = Directory.GetFiles(this._imageSavePath);

            foreach (string file in files)
            {
                if (file != this._imageSavePath + this._currentImageName)
                {
                    File.Delete(file);
                }
            }
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateStatus("Downloading latest image...");
        }

        private void Timer1_Tick(object Sender, EventArgs e)
        {
            DownloadLatestImage();
        }
       
        private void comboBoxInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;            
            string selected = cb.Items[cb.SelectedIndex].ToString();
            this._settings.UpdateInterval = SettingsFileUtils.ConvertIntervalFromControl(selected);

            SettingsFileUtils.SaveSettings(this._settings, this._settingsFile);

            // Reset timer
            InitTimer();
        }

        private void comboBoxImageSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            string selected = cb.Items[cb.SelectedIndex].ToString();

            this._settings.ImageSource = _imageSources.First<SourceImages>(x => x.name == selected).url.full;
            
            SettingsFileUtils.SaveSettings(this._settings, this._settingsFile);            
        }        

        private void checkBoxKeepOldImages_CheckedChanged(object sender, EventArgs e)
        {
            this._settings.KeepOldImages = ((CheckBox)sender).Checked;
            SettingsFileUtils.SaveSettings(this._settings, this._settingsFile);
        }
        
        private void checkBoxSetWallpaper_CheckedChangedAsync(object sender, EventArgs e)
        {
            if (((CheckBox) sender).Checked)
            {
                this._settings.SetAsWallpaper = true;
                SettingsFileUtils.SaveSettings(this._settings, this._settingsFile);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // If the form is minimized hide it from the task bar  
            // and show the system tray icon
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;                 
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var message = "This will prevent the app from updating images.\n\nAre you sure you want to close?";
            var window = MessageBox.Show(message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            e.Cancel = (window == DialogResult.No);
        }       

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("GOES Wallpaper, created by Bryan Peabody.\n\nFor more information visit https://public.bryanpeabody.com", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Open")
            {
                notifyIcon1_MouseDoubleClick(null, null);
            }
            else if (e.ClickedItem.Text == "Exit")
            {
                Application.Exit();
            }
        }       

        private void checkBoxStartAutomatically_CheckedChanged(object sender, EventArgs e)
        {
            // Command to run
            var command = "\"" + AppDomain.CurrentDomain.BaseDirectory + "GOES.exe\" --min";

            // Update settings file
            this._settings.RunOnStartup = ((CheckBox)sender).Checked;
            SettingsFileUtils.SaveSettings(this._settings, this._settingsFile);

            if (this._settings.RunOnStartup)
            {
                RunOnStartup.AddToStartup("GOES Wallpaper", command);
            }
            else
            {
                RunOnStartup.RemoveFromStartup(command);
            }
        }

        private void buttonDonate_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO");
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            
        }
        #endregion
    }
}
