using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAL
{
    class appsettings
    {
    }
    public class AppSettingsConfigure
    {
        public static String Read(String key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                return (result);
            }
            catch (ConfigurationErrorsException)
            {
                //Console.WriteLine("Error reading app settings");
            }
            return null;
        }
        public static void SetVal(String key, String value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
                //Console.WriteLine("Error writing app settings");
            }
        }
        static void Example_Code()
        {
            ReadAllSettings();
            ReadSetting("Setting1");
            ReadSetting("NotValid");
            AddUpdateAppSettings("NewSetting", "May 7, 2014");
            AddUpdateAppSettings("Setting1", "May 8, 2014");
            ReadAllSettings();
        }
        static void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }
        static void ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                Console.WriteLine(result);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }
        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
    public class ConnConfigure
    {
        static void Get_appSettings()
        {

            ConnectionStringSettingsCollection settings =
                ConfigurationManager.ConnectionStrings;

            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    String temps = String.Format("con_str:Name:{0};Provider:{1};ConStr:{2}", cs.Name, cs.ProviderName, cs.ConnectionString);
                    MessageBox.Show(temps);
                }
            }
        }
        static void GetConnectionStrings()
        {
            ConnectionStringSettingsCollection settings =
                ConfigurationManager.ConnectionStrings;
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    String temps = String.Format("con_str:Name:{0};Provider:{1};ConStr:{2}", cs.Name, cs.ProviderName, cs.ConnectionString);
                    MessageBox.Show(temps);
                }
            }
        }
        public static string GetConnectionStringByName(string name)
        {
            string returnValue = null;
            ConnectionStringSettings cs =
                ConfigurationManager.ConnectionStrings[name];
            if (cs != null)
            {
                returnValue = String.Format("con_str:Name:{0};Provider:{1};ConStr:{2}", cs.Name, cs.ProviderName, cs.ConnectionString);
            }
            return returnValue;
        }
        static string GetConnectonStringByProvider(string providerName)
        {
            string returnValue = null;
            ConnectionStringSettingsCollection settings =
                ConfigurationManager.ConnectionStrings;
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName)
                    {
                        returnValue = String.Format("con_str:Name:{0};Provider:{1};ConStr:{2}", cs.Name, cs.ProviderName, cs.ConnectionString);
                        break;
                    }
                }
            }
            return returnValue;
        }
        public static string ToggleConfigEncryption(string exeConfigName)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(exeConfigName);
                ConnectionStringsSection section =
                    config.GetSection("connectionStrings")
                as ConnectionStringsSection;
                if (section.SectionInformation.IsProtected)
                {
                    //   section.SectionInformation.UnprotectSection();
                }
                else
                {
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider"
                        );
                }
                config.Save();
                String msg = String.Format("Protected={0}", section.SectionInformation.IsProtected);

                return msg;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

    }
}
