
using System.Configuration;
using System;

namespace EnumerateFolders.Utils
{
    public class Generic
    {
        //  https://stackoverflow.com/questions/5274829/configurationmanager-appsettings-how-to-modify-and-save
        public static void AddOrUpdateAppSettings(string exeConfigPath, string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }
}
