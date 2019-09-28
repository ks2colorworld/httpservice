using System;
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
    public class RequestModel //: Dictionary<string, string> //==> 값이 JsonElement 형식
    {
        public string Gubun { get; set; }

        public string Proc { get; set; }

        public string Web_gubun { get; set; }

        public string SessionId { get; set; }

        public string Organization_key { get; set; }

        public string Operator_key { get; set; }

        //public InnerRequestModel Data { get; set; } = new InnerRequestModel();

        //public InnerRequestModel Options { get; set; } = new InnerRequestModel();

        public InnerRequestModel Parameters { get; set; } = new InnerRequestModel();

        public string GetValue(string key, string defaultValue = "")
        {
            //if (Data.Count > 0)
            //{
            //    if (Data.ContainsKey(key))
            //    {
            //        return Data[key];
            //    }
            //}

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
            //if (Data.Count > 0)
            //{
            //    if (Data.ContainsKey(key))
            //    {
            //        return true;
            //    }
            //}

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
