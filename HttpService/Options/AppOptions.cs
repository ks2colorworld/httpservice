using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Options
{
    public class AppOptions
    {
        public string DbConnectionConfig { get; set; }

        public string DbSchema { get; set; }

        public string Test { get; set; }

        public string Etc { get; set; }

        public string Mms { get; set; }

        public string MmsPublicKey { get; set; }

        public IList<string> AllowProcedureList { get; set; } = new List<string>();

        public IDictionary<string, string> Properties = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (Properties.ContainsKey(key))
                {
                    return Properties[key];
                }

                return null;
            }
        } 
    }
}
