using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wangsu.WcsLib.Core;
using Wangsu.WcsLib.Utility;

namespace TestCommon
{
    public class EnvUtility
    {
        public static Auth EnvAuth()
        {
            string[] aksk = GetEnvValues("WcsLibAkSk");
            if (null != aksk && 2 <= aksk.Length)
            {
                Mac mac = new Mac(aksk[0], aksk[1]);
                return new Auth(mac);
            }

            throw new Exception("Please set EnvironmentVariable \"WcsLibAkSk\" to \"AccessKey,SecretKey\".");
        }

        public static Config EnvConfig()
        {
            string[] configs = GetEnvValues("WcsLibConfig");
            if (null != configs && 2 <= configs.Length)
            {
                bool useHttps = false;
                if (3 == configs.Length && 0 == configs[2].CompareTo("true"))
                {
                    useHttps = true;
                }
                return new Config(configs[0], configs[1], useHttps);
            }

            Console.WriteLine("EnvironmentVariable \"WcsLibConfig\" not found, or invalid, use default Config().");
            return new Config();
        }

        private static string[] GetEnvValues(string variable)
        {
            string envVar = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(envVar))
            {
                envVar = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(envVar))
                {
                    envVar = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
                }
            }
            if (!string.IsNullOrEmpty(envVar))
            {
                return envVar.Split(',');
            }
            return null;
        }
    }
}
