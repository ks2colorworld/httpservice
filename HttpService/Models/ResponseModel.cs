using System.Collections.Generic;

namespace HttpService.Models
{
    public class ResponseModel
    {
        //public IList<ResponseValueModel> Values { get; set; } = new List<ResponseValueModel>();

        //public Dictionary<string, IEnumerable<DynamicRow>> DataSet { get; set; } = new Dictionary<string, IEnumerable<DynamicRow>>();

        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Values { get; set; } = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();


        public static ResponseModel Sampe => new ResponseModel
        {
            Values = new Dictionary<string, IEnumerable<Dictionary<string, object>>>
            {
                ["item"] = new List<Dictionary<string, object>>
                                    {
                                        new Dictionary<string, object>
                                        {
                                            ["COL1"] ="Hello",
                                            ["COL2"] ="World",
                                            ["COL3"] ="!",
                                        },
                                        new Dictionary<string, object>
                                        {
                                            ["COL1"] ="Hello2",
                                            ["COL2"] ="World2",
                                            ["COL3"] ="!2",
                                        }
                                    }
            }
        };

        public static ResponseModel Empty = new ResponseModel();
    }
}
