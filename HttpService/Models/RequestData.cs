using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Models
{
    public class RequestData
    {
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public string GetValue(string key, string defaultValue = "")
        {
            if (Data.Count > 0)
            {
                if (Data.ContainsKey(key))
                {
                    return Data[key];
                }
            }

            if (Parameters.Count > 0)
            {
                if (Parameters.ContainsKey(key))
                {
                    return Parameters[key];
                }
            }

            //if(Options.Count > 0) {
            //    if (Options.ContainsKey(key))
            //    {
            //        return Options[key];
            //    }
            //}

            return defaultValue;
        }

        public bool ContainsKey(string key)
        {
            if (Data.Count > 0)
            {
                if (Data.ContainsKey(key))
                {
                    return true;
                }
            }

            if (Parameters.Count > 0)
            {
                if (Parameters.ContainsKey(key))
                {
                    return true;
                }
            }

            //if(Options.Count > 0) {
            //    if (Options.ContainsKey(key))
            //    {
            //       return true;
            //    }
            //}

            return false;
        }
    }
}
