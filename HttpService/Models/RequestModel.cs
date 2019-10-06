using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HttpService.Models
{
    public class RequestModel : Dictionary<string, string> 
    {
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

        public string GetValue(string key, string defaultValue = "")
        {
            if (Count > 0 && ContainsKey(key))
            {
                return this[key];
            }            

            if (Data.Count > 0)
            {
                if (Data.ContainsKey(key))
                {
                    return Data[key];
                }
            }
       
            return defaultValue;
        }

        public bool HasKey(string key)
        {
            if (Count > 0 && ContainsKey(key))
            {
                return true;
            }

            if (Data.Count > 0)
            {
                if (Data.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
