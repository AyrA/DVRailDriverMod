using DVRailDriverMod.Calibration;
using System;
using System.IO;

namespace DVRailDriverMod.RailDriverDevice
{
    internal static class DeviceCalibration
    {
        public static readonly string CalibrationFile = Environment.ExpandEnvironmentVariables(@"%APPDATA%\DVRailDriver\calibration.bin");

        public static bool HasCalibrationData()
        {
            return File.Exists(CalibrationFile);
        }

        public static void SaveCalibrationData(CalibrationData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            Directory.CreateDirectory(Path.GetDirectoryName(CalibrationFile));
            using (var fs = File.Create(CalibrationFile))
            {
                data.Serialize(fs);
            }
        }

        public static bool DeleteCalibrationData()
        {
            try
            {
                File.Delete(CalibrationFile);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static CalibrationData GetCalibrationData()
        {
            try
            {
                var ret = new CalibrationData();
                using (var fs = File.OpenRead(CalibrationFile))
                {
                    ret.Deserialize(fs);
                }
                return ret;
            }
            catch
            {
                return new CalibrationData();
            }
        }
    }
}
