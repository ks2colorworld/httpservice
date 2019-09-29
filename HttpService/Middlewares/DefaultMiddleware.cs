using HttpService.Lib;
using HttpService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HttpService.Middlewares
{
    public static class DefaultMiddleware
    {
        public static void UseDefault(this IApplicationBuilder app)
        {
            app.Map("", DefaultHandler);
        }

        private static void DefaultHandler(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                string[] noCheckSessionIDGubun = {
                "menu_ctrl_bind",
            };

                if (context.Request.Method.ToLower() == "post")
                {
                    var xmlCommonUtil = app.ApplicationServices.GetService<XMLCommonUtil>();
                    var excelDownload = app.ApplicationServices.GetService<ExcelDownload>();
                    var sendMobileMSGCommon = app.ApplicationServices.GetService<SendMobileMSGCommon>();
                    var sendEmail = app.ApplicationServices.GetService<SendEmail>();
                    var uploadAndPostTwitPic = app.ApplicationServices.GetService<UploadAndPostTwitPic>();

                    var data = String.Empty;
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        data = await reader.ReadToEndAsync();
                        reader.Close();
                    }

                    RequestModel model = new RequestModel();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        var jsonSerializerOptions= new JsonSerializerOptions {
                            AllowTrailingCommas = true,
                            IgnoreNullValues = true,
                            PropertyNameCaseInsensitive = true,
                            MaxDepth = 2,
                        };

                        jsonSerializerOptions.Converters.Add(new HttpService.Serializer.RequestModelJsonConverter());

                        model = JsonSerializer.Deserialize<RequestModel>(data, jsonSerializerOptions);
                    }                  

                    xmlCommonUtil.SetReqestModel(model);

                    string gubun = xmlCommonUtil.GUBUN;

                    Func<bool> passCheckSessionIDFunc = () =>
                    {
                        bool isPass = false;
                        for (int i = 0; i < noCheckSessionIDGubun.Length; i++)
                        {
                            if (noCheckSessionIDGubun[i] == xmlCommonUtil.GUBUN)
                            {
                                isPass = true;
                                break;
                            }
                        }
                        return isPass;
                    };

                    var PassCheckSessionID = passCheckSessionIDFunc();

                    if (!xmlCommonUtil.CheckSessionID() && !PassCheckSessionID)
                    {
                        // TODO 반환값 정의
                        //return StatusCode(401);
                        context.Response.StatusCode = 401;
                    }

                    switch (gubun)
                    {
                        //sessionID_check
                        case XMLCommonUtil.SESSIONID_CHECK_GUBUN:
                        case XMLCommonUtil.GET_SESSIONID_GUBUN:
                            xmlCommonUtil.returnSessionID();
                            break;
                        //userlogin
                        case XMLCommonUtil.USER_LOGIN_GUBUN:
                            var userLoginData = xmlCommonUtil.WriteXML(true);

                            //return Ok(userLoginData);
                            break;
                        //break;
                        case "menu_ctrl_bind":
                            xmlCommonUtil.ReturnMenuXML();
                            break;

                        /*/sendSMS - 사용안함.
                        case "send_public_sms":
                            string dataMmsPublicKey = xmlCommonUtil.QueryString["mms_public_key"];
                            if (string.IsNullOrEmpty(dataMmsPublicKey))
                            {
                                break;
                            }
                            string mmsPublicKey = ConfigurationManager.AppSettings["mms_public_key"].ToString();
                            if (!string.Equals(dataMmsPublicKey, mmsPublicKey))
                            {
                                break;
                            }

                            SendMobileMSGCommon smmc2 = new SendMobileMSGCommon();
                            smmc2.SendMobileMSG();
                            break;
                        //*/

                        case "send_sms":
                            //SendMobileMSGCommon smmc = new SendMobileMSGCommon();
                            //smmc.SendMobileMSG();
                            sendMobileMSGCommon.SendMobileMSG();
                            break;

                        case "csv":
                            //ExcelDownload ed = new ExcelDownload();
                            //ed.DownLoadCSVFile();

                            var response = excelDownload.DownLoadCSVFile();
                            if (response is FileResponseModel)
                            {
                                var fileResponse = response as FileResponseModel;
                                //return File(fileResponse.Content, fileResponse.ContentType, fileResponse.FileName);
                                break;
                            }
                            else
                            {
                                //return Ok(response);
                                break;
                            }
                        //break;

                        case "send_email":
                            //SendEmail se = new SendEmail();
                            //se.send();
                            var sendemailResult = sendEmail.send();
                            //return Ok(sendemailResult);

                            break;

                        case "upload_twitpic":
                            //UploadAndPostTwitPic ut = new UploadAndPostTwitPic();
                            //ut.UploadAndPost();

                            var uploadAndPostTwitPicResult = uploadAndPostTwitPic.UploadAndPost();
                            //return Ok(uploadAndPostTwitPicResult);
                            break;

                        /*기본 xml return *********************************/
                        default:
                            //if (!string.IsNullOrEmpty(xmlCommonUtil.QueryString[XMLCommonUtil.PROC_KEY_STRING]))
                            //if (!string.IsNullOrEmpty(xmlCommonUtil.RequestData.Proc))
                            if (!string.IsNullOrEmpty(xmlCommonUtil.RequestData.GetValue(XMLCommonUtil.PROC_KEY_STRING)))
                            {
                                var dataResult = xmlCommonUtil.WriteXML(false);

                                //dataResult = ResponseModel.Sampe;
                                //dataResult = ResponseModel.Empty;

                                await context.ExecuteResponseModelResult(dataResult);
                            }
                            else
                            {
                                var messageResult = xmlCommonUtil.ResponseWriteErrorMSG("필수 매개변수를 넘기지 않았습니다.");
                                // TODO 반환값 정의
                                //return Ok(messageResult);
                            }
                            break;
                    }

                    return;
                }

                await next.Invoke();
            });
        }



    }
}
