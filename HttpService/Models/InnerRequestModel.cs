using System.Collections.Generic;

namespace HttpService.Models
{
    public class InnerRequestModel : Dictionary<string, string>
    {
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
