using System.Text;

namespace HttpService.Models
{
    public class FileResponseModel: ResponseModel
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public Encoding ContentEncoding { get; set; }

        public byte[] Content { get; set; }
    }    
}
