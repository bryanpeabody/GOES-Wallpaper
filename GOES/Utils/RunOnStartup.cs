using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace GOES.Utils
{
    public static class RunOnStartup
    {
        private const string AppTitle = "GOES Wallpaper";

        public static bool AddToStartup(string AppName, string AppPath)
        {
            RegistryKey rk;            

            try
            {
                rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.SetValue(AppTitle, AppPath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool RemoveFromStartup(string AppPath)
        {
            RegistryKey rk;

            try
            {
                rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (AppPath == null)
                {
                    rk.DeleteValue(AppTitle);
                }
                else
                {
                    if (rk.GetValue(AppTitle).ToString().ToLower() == AppPath.ToLower())
                    {
                        rk.DeleteValue(AppTitle);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }        
    }
}
