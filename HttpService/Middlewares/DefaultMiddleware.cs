using HttpService.Lib;
using HttpService.Models;
using HttpService.Options;
using HttpService.Services;
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
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace HttpService.Middlewares
{
    public static class DefaultMiddleware
    {
        public static void UseDefaultMiddlewares(this IApplicationBuilder app)
        {
            /**
             * 요청을 처리합니다.
             * 대상 URI: / , /Default, /Default.aspx 
             */
            app.MapWhen((context) =>
            {
                var paths = new[] { "/", "/Default", "/Default.aspx" };

                return context.Request.Path.HasValue && paths.Contains(context.Request.Path.Value, new PathComparer());
            }, DefaultHandler);

            /**
             * 요청을 처리합니다.
             * 대상 URI: /File, /File.aspx 
             */
            app.MapWhen((context) =>
            {
                var paths = new[] { "/File", "/File.aspx" };

                return context.Request.Path.HasValue && paths.Contains(context.Request.Path.Value, new PathComparer());
            }, FileHandler);

            /**
             * 요청을 처리합니다.
             * 대상 URI: /Uploader , /Uploader.aspx, /Uploader.ashx 
             */
            app.MapWhen((context) =>
            {
                var paths = new[] { "/Uploader", "/Uploader.aspx", "/Uploader.ashx" };

                return context.Request.Path.HasValue && paths.Contains(context.Request.Path.Value, new PathComparer());
            }, UploadHandler);
        }

        private static void DefaultHandler(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                if (context.Request.Path.HasValue)
                {
                    var path = context.Request.Path.Value;
                }

                string[] noCheckSessionIDGubun = {
                    "menu_ctrl_bind",
                };

                // 의존 객체의 인스턴스를 생성합니다.
                var databaseManager  = app.ApplicationServices.GetService<IDatabaseManager>();
                var fileManager = app.ApplicationServices.GetService<IFileManager>();
                var mobileMessageManager=app.ApplicationServices.GetService<IMobileMessageManager>();
                var emailManager = app.ApplicationServices.GetService<IEmailManager>();
                var httpContextManager = app.ApplicationServices.GetService<IHttpContextManager>();
                var responsePreprocessManager = app.ApplicationServices.GetService<IResponsePreprocessManager>();
                var twitPicManager = app.ApplicationServices.GetService<ITwitPicManager>();

                RequestModel model = new RequestModel();

                // HTTP 요청 본문 또는 FormData 를 객체로 변환합니다. 
                model = await httpContextManager.ParseRequestData();

                string gubun = model.GetValue(Constants.GUBUN_KEY_STRING);

                Func<bool> passCheckSessionIDFunc = () =>
                {
                    bool isPass = false;
                    for (int i = 0; i < noCheckSessionIDGubun.Length; i++)
                    {
                        if (noCheckSessionIDGubun[i] == gubun)
                        {
                            isPass = true;
                            break;
                        }
                    }
                    return isPass;
                };

                var PassCheckSessionID = passCheckSessionIDFunc();                

                ResponseModel responseModel = null;
                DataSet dataSet = null;

                try
                {
                    if (!PassCheckSessionID && !httpContextManager.Check(model))
                    {
                        var clientSessionId = model.GetValue(Constants.SESSIONID_KEY_STRING);

                        if (String.IsNullOrEmpty(clientSessionId))
                        {
                            throw new ServiceException($"{Constants.SESSIONID_KEY_STRING}값을 넘기지 않았습니다.");
                        }

                        throw new ServiceException("999", "서버와 세션유지시간이 초과하였습니다.");
                    }

                    switch (gubun)
                    {
                        //sessionID_check
                        case Constants.SESSIONID_CHECK_GUBUN:
                        case Constants.GET_SESSIONID_GUBUN:
                            var currentSessionId = httpContextManager.GetSessionId();
                            responseModel = ResponseModel.Message("1", currentSessionId);
                            break;
                        //userlogin
                        case Constants.USER_LOGIN_GUBUN:
                            dataSet = await databaseManager.ExecuteQueryAsync(model, true);
                            responseModel = responsePreprocessManager.ProcessDataSet(dataSet);
                            break;
                        //break;
                        case "menu_ctrl_bind":
                            // 메뉴 데이터 처리 
                            dataSet = await databaseManager.ExecuteQueryAsync(model, true);
                            responseModel = responsePreprocessManager.ProcessDataSet(dataSet);
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
                            responseModel = await mobileMessageManager.Send(model);
                            break;

                        case "csv":
                            responseModel = await fileManager.DownloadCsv(model);
                            break;

                        case "send_email":
                            responseModel = await emailManager.Send(model);
                            break;

                        case "upload_twitpic":
                            responseModel =await twitPicManager.Post(model);                            
                            break;

                        /*기본 xml return *********************************/
                        default:

                            if (!string.IsNullOrEmpty(model.GetValue(XMLCommonUtil.PROC_KEY_STRING)))
                            {
                                dataSet = await databaseManager.ExecuteQueryAsync(model, false);

                                // 예제 데이터 확인
                                //dataResult = ResponseModel.Sampe;
                                //dataResult = ResponseModel.Empty;

                                responseModel = responsePreprocessManager.ProcessDataSet(dataSet);
                            }
                            else
                            {
                                throw new ServiceException("필수 매개변수를 넘기지 않았습니다.");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    responseModel = responsePreprocessManager.ProcessException(ex);
                }

                if (responseModel == null)
                {
                    responseModel = ResponseModel.DefaultMessage;
                }

                await context.ExecuteResponseModelResult(responseModel);

                return;
            });
        }

        private static void FileHandler(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                var httpContextManager = app.ApplicationServices.GetService<IHttpContextManager>();
                var fileManager = app.ApplicationServices.GetService<IFileManager>();
                var responsePreprocessManager = app.ApplicationServices.GetService<IResponsePreprocessManager>();

                string gubun = String.Empty;// = xmlCommonUtil.GUBUN;
                string webGubun = String.Empty; // xmlCommonUtil.WEB_GUBUN;

                RequestModel requestModel = null;
                ResponseModel responseModel = null;
                try
                {
                    requestModel = await httpContextManager.ParseRequestData();
                    //xmlCommonUtil.SetReqestModel(requestModel);

                    gubun = requestModel.GetValue(Constants.GUBUN_KEY_STRING);
                    webGubun = requestModel.GetValue(Constants.WEB_GUBUN_KEY);

                    bool passCheckSessionID = (gubun == "web" && webGubun != string.Empty);

                    if (!passCheckSessionID && !httpContextManager.Check(requestModel))
                    {
                        var clientSessionId = requestModel.GetValue(Constants.SESSIONID_KEY_STRING);

                        if (String.IsNullOrEmpty(clientSessionId))
                        {
                            throw new ServiceException($"{Constants.SESSIONID_KEY_STRING}값을 넘기지 않았습니다.");
                        }

                        throw new ServiceException("999", "서버와 세션유지시간이 초과하였습니다.");
                    }

                    switch (gubun)
                    {
                        case Constants.FILE_DOWNLOAD_GUBUN_value:
                            //responseModel = fileCommonUtil.DownloadFile();
                            responseModel = await fileManager.DownloadFile(requestModel);
                            break;
                        case Constants.FILE_DELETE_GUBUN_value:
                            //responseModel = fileCommonUtil.DeleteFile();
                            responseModel = await fileManager.DeleteFile(requestModel);
                            break;

                        case Constants.FILE_INFO_GUBUN_value:
                            //responseModel = fileCommonUtil.GetFileInfo();
                            responseModel = await fileManager.GetFileInfo(requestModel);
                            break;

                        case Constants.FILE_LIST_GUBUN_value:
                            //responseModel = fileCommonUtil.GetFileNameList();
                            responseModel = await fileManager.GetFileNameList(requestModel);
                            break;
                        case Constants.FILE_RENAME_GUBUN_value:
                            //responseModel = fileCommonUtil.FileRename();
                            responseModel = await fileManager.FileRename(requestModel);
                            break;
                        default:
                            throw new ServiceException("유효하지 않은 요청입니다.");
                    }
                }
                catch (Exception ex)
                {
                    responseModel = responsePreprocessManager.ProcessException(ex);
                }

                if (responseModel == null)
                {
                    responseModel = ResponseModel.DefaultMessage;
                }

                await context.ExecuteResponseModelResult(responseModel);
            });
        }

        private static void UploadHandler(IApplicationBuilder app)
        {
            app.Run(async (context) => {

                var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("UploadHandler");
                var httpContextManager = app.ApplicationServices.GetService<IHttpContextManager>();
                var fileManager = app.ApplicationServices.GetService<IFileManager>();
                
                RequestModel requestModel = null;
                ResponseModel responseModel = null;

                try
                {
                    requestModel = await httpContextManager.ParseRequestData();

                    responseModel = await fileManager.UploadFile(requestModel);
                }
                catch (ServiceException ex)
                {
                    responseModel = ResponseModel.ErrorMessage(ex);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(UploadHandler)}: {ex.Message}");

                    responseModel = ResponseModel.Message("100", "파일업로드 에러가 발생했습니다.");
                }

                if (responseModel == null)
                {
                    responseModel = ResponseModel.DefaultMessage;
                }

                await context.ExecuteResponseModelResult(responseModel);
            });         
        }
    }
}
