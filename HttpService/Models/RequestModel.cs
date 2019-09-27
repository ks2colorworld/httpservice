using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpService.Models
{
    public class RequestModel
    {
        public string Gubun { get; set; }

        public string Proc { get; set; }

        public string WebGubun { get; set; }


    }

    public class ResponseModel
    {
        public IList<ResponseValueModel> Values { get; set; } = new List<ResponseValueModel>();

        public IDictionary<string, IEnumerable<dynamic>> DataSet { get; set; } = new Dictionary<string, IEnumerable<dynamic>>();
    }

    public class ResponseValueModel
    {
        public IList<ResponseItemModel> Item { get; set; } = new List<ResponseItemModel>();
    }

    public class ResponseItemModel 
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }

    public class FileResponseModel: ResponseModel
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public Encoding ContentEncoding { get; set; }

        public byte[] Content { get; set; }
    }
}
