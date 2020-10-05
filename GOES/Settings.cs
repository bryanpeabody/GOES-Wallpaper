using System;
using System.Collections.Generic;
using System.Text;

namespace GOES
{
    public class Settings
    {
        public string ImageSource { get; set; }
        public string UpdateInterval { get; set; }
        public bool KeepOldImages { get; set; }
        public bool SetAsWallpaper { get; set; }
        public bool RunOnStartup { get; set; }
    }
}
