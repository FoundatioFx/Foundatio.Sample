using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Foundatio.Logging;
using Newtonsoft.Json;
using Samples.Core.Extensions;

namespace Samples.Core.Utility {
    public abstract class SettingsBase<T> : SingletonBase<T>, IInitializable where T : class {
        public abstract void Initialize();

        protected static bool GetBool(string name, bool defaultValue = false) {
            string value = GetString(name);

            bool boolean;
            return !String.IsNullOrEmpty(value) && Boolean.TryParse(value, out boolean) ? boolean : defaultValue;
        }

        protected static string GetConnectionString(string name, string defaultValue = null) {
            string value = GetEnvironmentVariable(name) ?? GetConfigVariable(name);
            if (!String.IsNullOrEmpty(value))
                return value;

            var connectionString = ConfigurationManager.ConnectionStrings[name];
            return connectionString != null ? connectionString.ConnectionString : defaultValue;
        }

        protected static TEnum GetEnum<TEnum>(string name, TEnum? defaultValue = null) where TEnum : struct {
            string value = GetEnvironmentVariable(name) ?? GetConfigVariable(name);
            if (String.IsNullOrEmpty(value))
                return ConfigurationManager.AppSettings.GetEnum(name, defaultValue);

            try {
                return (TEnum)Enum.Parse(typeof(TEnum), value, true);
            } catch (ArgumentException ex) {
                if (defaultValue is TEnum)
                    return (TEnum)defaultValue;

                string message = String.Format("Configuration key '{0}' has value '{1}' that could not be parsed as a member of the {2} enum type.", name, value, typeof(TEnum).Name);
                throw new ConfigurationErrorsException(message, ex);
            }
        }

        protected static int GetInt(string name, int defaultValue = 0) {
            string value = GetString(name);

            int number;
            return !String.IsNullOrEmpty(value) && Int32.TryParse(value, out number) ? number : defaultValue;
        }
        
        protected static double GetDouble(string name, double defaultValue = 0.0) {
            string value = GetString(name);

            double number;
            return !String.IsNullOrEmpty(value) && Double.TryParse(value, out number) ? number : defaultValue;
        }

        protected static Version GetVersion(string name, Version defaultValue = null) {
            string value = GetString(name);

            Version version;
            return !String.IsNullOrEmpty(value) && Version.TryParse(value, out version) ? version : defaultValue;
        }
        
        protected static DateTime GetDateTime(string name, DateTime defaultValue) {
            string value = GetString(name);

            DateTime dateTime;
            return !String.IsNullOrEmpty(value) && DateTime.TryParse(value, out dateTime) ? dateTime : defaultValue;
        }
        
        protected static string GetString(string name, string defaultValue = null) {
            return GetEnvironmentVariable(name) ?? GetConfigVariable(name) ?? ConfigurationManager.AppSettings[name] ?? defaultValue;
        }

        protected static List<string> GetStringList(string name, string defaultValues = null, char[] separators = null) {
            string value = GetEnvironmentVariable(name) ?? GetConfigVariable(name);
            if (String.IsNullOrEmpty(value))
                return ConfigurationManager.AppSettings.GetStringList(name, defaultValues, separators);

            if (separators == null)
                separators = new[] { ',' };

            return value.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }

        private static Dictionary<string, string> _configVariables;

        protected static string GetConfigVariable(string name) {
            if (String.IsNullOrEmpty(name))
                return null;

            if (_configVariables == null) {
                try {
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
                    // check to see if environment specific config exists and use that instead
                    if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\", "config.json")))
                        configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\", "config.json");

                    if (!File.Exists(configPath)) {
                        _configVariables = new Dictionary<string, string>();
                        return null;
                    }

                    _configVariables = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(configPath));
                } catch (Exception ex) {
                    Logger.Error().Exception(ex).Message("Unable to load config.json file. Error: {0}", ex.Message);
                    _configVariables = new Dictionary<string, string>();
                    return null;
                }
            }

            return _configVariables.ContainsKey(name) ? _configVariables[name] : null;
        }

        protected static string EnvironmentVariablePrefix { get; set; }

        private static Dictionary<string, string> _environmentVariables;

        private static string GetEnvironmentVariable(string name) {
            if (String.IsNullOrEmpty(name))
                return null;

            if (_environmentVariables == null) {
                try {
                    _environmentVariables = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().ToDictionary(e => e.Key.ToString(), e => e.Value.ToString());
                } catch (Exception ex) {
                    _environmentVariables = new Dictionary<string, string>();

                    Logger.Error().Exception(ex).Message("Error while reading environmental variables.").Write();
                    return null;
                }
            }

            if (!_environmentVariables.ContainsKey(EnvironmentVariablePrefix + name))
                return null;

            return _environmentVariables[EnvironmentVariablePrefix + name];
        }
    }
}
