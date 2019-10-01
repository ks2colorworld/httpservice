using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HttpService.Models
{
    public class ResponseModel
    {
        //public IList<ResponseValueModel> Values { get; set; } = new List<ResponseValueModel>();

        //public Dictionary<string, IEnumerable<DynamicRow>> DataSet { get; set; } = new Dictionary<string, IEnumerable<DynamicRow>>();

        //public Dictionary<string, IEnumerable<Dictionary<string, object>>> Values { get; set; } = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();

        public ValuesModel Values { get; set; } = new ValuesModel();

        public static ResponseModel Sample => new ResponseModel
        {
            //Values = new Dictionary<string, IEnumerable<Dictionary<string, object>>>
            //{
            //    ["item"] = new List<Dictionary<string, object>>
            //                        {
            //                            new Dictionary<string, object>
            //                            {
            //                                ["COL1"] ="Hello",
            //                                ["COL2"] ="World",
            //                                ["COL3"] ="!",
            //                            },
            //                            new Dictionary<string, object>
            //                            {
            //                                ["COL1"] ="Hello2",
            //                                ["COL2"] ="World2",
            //                                ["COL3"] ="!2",
            //                            }
            //                        }
            //}

            Values = new ValuesModel()
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

        public static ResponseModel DefaultMessage => new ResponseModel
        {
            //Values = new Dictionary<string, IEnumerable<Dictionary<string, object>>>
            //{
            //    ["item"] = new List<Dictionary<string, object>>
            //                        {
            //                            new Dictionary<string, object>
            //                            {
            //                                // TODO 메시지 응답 코드 기본값 설정
            //                                ["Code"] = "100", 
            //                                ["Message"] = "올바른 응답 데이터를 제공할 수 없습니다.",
            //                            },
                                    
            //                        }
            //}
            Values = new ValuesModel
            {
                ["item"] = new List<Dictionary<string, object>>
                                        {
                                            new Dictionary<string, object>
                                            {
                                                // TODO 메시지 응답 코드 기본값 설정
                                                ["Code"] = "100",
                                                ["Message"] = "올바른 응답 데이터를 제공할 수 없습니다.",
                                            },

                                        }
            }
        };

        public static ResponseModel ErrorMessage(Exception ex)
        {
            var code = ServiceException.DEFAULT_CODE;
            var message = String.Empty;

            if (ex is ServiceException)
            {
                var serviceEx = (ServiceException)ex;
                code = serviceEx.Code;
            }

            message = ex.Message;

            return ResponseModel.Message(code, message);
        }

        public static ResponseModel Message(string code, string message)
        {
            return new ResponseModel
            {
                //Values = new Dictionary<string, IEnumerable<Dictionary<string, object>>>
                //{
                //    ["item"] = new List<Dictionary<string, object>>
                //                    {
                //                        new Dictionary<string, object>
                //                        {
                //                            // TODO 메시지 응답 코드 기본값 설정
                //                            ["Code"] = code,
                //                            ["Message"] = message
                //                        },

                //                    }
                //}
                Values = new ValuesModel
                {
                    ["item"] = new List<Dictionary<string, object>>
                                        {
                                            new Dictionary<string, object>
                                            {
                                                // TODO 메시지 응답 코드 기본값 설정
                                                ["Code"] = code,
                                                ["Message"] = message
                                            },

                                        }
                }
            };
        }

        public static ResponseModel Empty = new ResponseModel();
    }

    public class ValuesModel : Dictionary<string, IEnumerable<Dictionary<string, object>>>
    {
        [XmlAttribute()]
        public string Namespace { get; set; }
    }
}
