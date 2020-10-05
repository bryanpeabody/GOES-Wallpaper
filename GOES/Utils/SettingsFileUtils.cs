using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GOES.Utils
{
    public static class SettingsFileUtils
    {        
        public static string ConvertIntervalFromSettingsFile(string interval)
        {
            string newInterval = string.Empty;

            switch (interval)
            {
                case IntervalUtils.INTERVAL_1_MINUTE:
                    newInterval = IntervalUtils.INTERVAL_1_MINUTE_STRING;
                    break;
                case IntervalUtils.INTERVAL_5_MINUTES:
                    newInterval = IntervalUtils.INTERVAL_5_MINUTES_STRING;
                    break;
                case IntervalUtils.INTERVAL_30_MINUTES:
                    newInterval = IntervalUtils.INTERVAL_30_MINUTES_STRING;
                    break;
                case IntervalUtils.INTERVAL_1_HOUR:
                    newInterval = IntervalUtils.INTERVAL_1_HOUR_STRING;
                    break;
                case IntervalUtils.INTERVAL_3_HOURS:
                    newInterval = IntervalUtils.INTERVAL_3_HOURS_STRING;
                    break;
                case IntervalUtils.INTERVAL_6_HOURS:
                    newInterval = IntervalUtils.INTERVAL_6_HOURS_STRING;
                    break;
                case IntervalUtils.INTERVAL_12_HOURS:
                    newInterval = IntervalUtils.INTERVAL_6_HOURS_STRING;
                    break;
                case IntervalUtils.INTERVAL_24_HOURS:
                    newInterval = IntervalUtils.INTERVAL_24_HOURS_STRING;
                    break;
                default:
                    newInterval = IntervalUtils.INTERVAL_30_MINUTES_STRING;
                    break;
            }

            return newInterval;
        }

        public static string ConvertIntervalFromControl(string interval)
        {
            string newInterval = string.Empty;

            switch (interval)
            {
                case IntervalUtils.INTERVAL_1_MINUTE_STRING:
                    newInterval = IntervalUtils.INTERVAL_1_MINUTE;
                    break;
                case IntervalUtils.INTERVAL_5_MINUTES_STRING:
                    newInterval = IntervalUtils.INTERVAL_5_MINUTES;
                    break;
                case IntervalUtils.INTERVAL_30_MINUTES_STRING:
                    newInterval = IntervalUtils.INTERVAL_30_MINUTES;
                    break;
                case IntervalUtils.INTERVAL_1_HOUR_STRING:
                    newInterval = IntervalUtils.INTERVAL_1_HOUR;
                    break;
                case IntervalUtils.INTERVAL_3_HOURS_STRING:
                    newInterval = IntervalUtils.INTERVAL_3_HOURS;
                    break;
                case IntervalUtils.INTERVAL_6_HOURS_STRING:
                    newInterval = IntervalUtils.INTERVAL_6_HOURS;
                    break;
                case IntervalUtils.INTERVAL_12_HOURS_STRING:
                    newInterval = IntervalUtils.INTERVAL_12_HOURS;
                    break;
                case IntervalUtils.INTERVAL_24_HOURS_STRING:
                    newInterval = IntervalUtils.INTERVAL_24_HOURS;
                    break;
                default:
                    newInterval = IntervalUtils.INTERVAL_30_MINUTES;
                    break;
            }

            return newInterval;
        }

        public static Settings LoadSettings(string settingsFile)
        {
            var json = File.ReadAllText(settingsFile);
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public static void SaveSettings(Settings settings, String settingsFile)
        {
            var json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(settingsFile, json);
        }
    }
}
