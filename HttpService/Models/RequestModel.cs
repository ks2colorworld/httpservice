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
    //[DataContract]
    public class RequestModel : Dictionary<string, string> 
    {
        //public string Gubun { get; set; }

        //public string Proc { get; set; }

        //public string Web_gubun { get; set; }

        //public string SessionId { get; set; }

        //public string Organization_key { get; set; }

        //public string Operator_key { get; set; }

        //public InnerRequestModel Data { get; set; } = new InnerRequestModel();

        //public InnerRequestModel Options { get; set; } = new InnerRequestModel();

        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public string GetValue(string key, string defaultValue = "")
        {
            if (Count > 0 && ContainsKey(key))
            {
                return this[key];
            }            

            if (Parameters.Count > 0)
            {
                if (Parameters.ContainsKey(key))
                {
                    return Parameters[key];
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

            if (Parameters.Count > 0)
            {
                if (Parameters.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
