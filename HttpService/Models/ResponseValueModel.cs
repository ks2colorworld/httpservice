using System.Collections.Generic;

namespace HttpService.Models
{
    public class ResponseValueModel
    {
        public IList<ResponseItemModel> Item { get; set; } = new List<ResponseItemModel>();
    }
}
