using System;
using System.Diagnostics;
using Samples.Core.Utility;

namespace Samples.Core {
    public class Settings : SettingsBase<Settings> {
        public string RedisConnectionString { get; private set; }

        public bool EnableRedis { get; private set; }

        public string Version { get; private set; }

        public override void Initialize() {
            RedisConnectionString = GetConnectionString("RedisConnectionString");
            EnableRedis = GetBool("EnableRedis", !String.IsNullOrEmpty(RedisConnectionString));

            Version = FileVersionInfo.GetVersionInfo(typeof(Settings).Assembly.Location).ProductVersion;
        }
    }
}