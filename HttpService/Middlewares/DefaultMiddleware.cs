using HttpService.Lib;
using HttpService.Models;
using HttpService.Options;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace HttpService.Middlewares
{
    public static class DefaultMiddleware
    {
        public static void UseDefault(this IApplicationBuilder app)
        {
            app.MapWhen((context) =>
            {
                var paths = new[] { "/", "/Default", "/Default.aspx" };

                return context.Request.Path.HasValue && paths.Contains(context.Request.Path.Value, new PathComparer());
            }, DefaultHandler);

            app.MapWhen((context) =>
            {
                var paths = new[] { "/File", "/File.aspx" };

                return context.Request.Path.HasValue && paths.Contains(context.Request.Path.Value, new PathComparer());
            }, FileHandler);


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

                var requestDataParser = app.ApplicationServices.GetService<RequestDataParser>();
                var xmlCommonUtil = app.ApplicationServices.GetService<XMLCommonUtil>();
                var excelDownload = app.ApplicationServices.GetService<ExcelDownload>();
                var sendMobileMSGCommon = app.ApplicationServices.GetService<SendMobileMSGCommon>();
                var sendEmail = app.ApplicationServices.GetService<SendEmail>();
                var uploadAndPostTwitPic = app.ApplicationServices.GetService<UploadAndPostTwitPic>();

                RequestModel model = new RequestModel();

                model = await requestDataParser.Parse();

                //if (context.Request.Method.ToLower() == "post")
                //{
                //    var data = String.Empty;
                //    using (var reader = new StreamReader(context.Request.Body))
                //    {
                //        data = await reader.ReadToEndAsync();
                //        reader.Close();
                //    }
                //    if (!String.IsNullOrWhiteSpace(data))
                //    {
                //        var jsonSerializerOptions = new JsonSerializerOptions
                //        {
                //            AllowTrailingCommas = true,
                //            IgnoreNullValues = true,
                //            PropertyNameCaseInsensitive = true,
                //            MaxDepth = 2,
                //        };
                //        jsonSerializerOptions.Converters.Add(new HttpService.Serializer.RequestModelJsonConverter());
                //        model = JsonSerializer.Deserialize<RequestModel>(data, jsonSerializerOptions);
                //    }
                //}

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

                ResponseModel responseModel = null;
                try
                {
                    switch (gubun)
                    {
                        //sessionID_check
                        case XMLCommonUtil.SESSIONID_CHECK_GUBUN:
                        case XMLCommonUtil.GET_SESSIONID_GUBUN:
                            responseModel = xmlCommonUtil.returnSessionID();
                            break;
                        //userlogin
                        case XMLCommonUtil.USER_LOGIN_GUBUN:
                            responseModel = xmlCommonUtil.WriteXML(true);

                            //return Ok(userLoginData);
                            break;
                        //break;
                        case "menu_ctrl_bind":
                            responseModel = xmlCommonUtil.ReturnMenuXML();
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
                            responseModel = sendMobileMSGCommon.SendMobileMSG();
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
                                //var dataResult 
                                responseModel = xmlCommonUtil.WriteXML(false);

                                //dataResult = ResponseModel.Sampe;
                                //dataResult = ResponseModel.Empty;
                            }
                            else
                            {
                                var messageResult = xmlCommonUtil.ResponseWriteErrorMSG("필수 매개변수를 넘기지 않았습니다.");
                                // TODO 반환값 정의
                                //return Ok(messageResult);
                            }
                            break;
                    }
                }
                //catch (ServiceException ex)
                //{
                //    responseModel = ResponseModel.ErrorMessage(ex);
                //}
                catch (Exception ex)
                {
                    responseModel = ResponseModel.ErrorMessage(ex);
                }

                if (responseModel == null)
                {
                    responseModel = ResponseModel.DefaultMessage;
                }

                await context.ExecuteResponseModelResult(responseModel);

                return;


                //await next.Invoke();
            });
        }

        private static void FileHandler(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                var requestDataParser = app.ApplicationServices.GetService<RequestDataParser>();
                var xmlCommonUtil = app.ApplicationServices.GetService<XMLCommonUtil>();
                var fileCommonUtil = app.ApplicationServices.GetService<FileCommonUtil>();

                string gubun = xmlCommonUtil.GUBUN;
                string webGubun = xmlCommonUtil.WEB_GUBUN;
                RequestModel requestModel = null;
                ResponseModel responseModel = null;
                try
                {
                    requestModel = await requestDataParser.Parse();
                    xmlCommonUtil.SetReqestModel(requestModel);

                    bool passCheckSessionID = (gubun == "web" && webGubun != string.Empty);

                    if (passCheckSessionID)
                    {
                        gubun = webGubun;
                    }

                    if (!(passCheckSessionID || xmlCommonUtil.CheckSessionID()))
                    {
                        //세션 채크함.
                        return;
                    }                    

                    switch (gubun)
                    {
                        case FileCommonUtil.FILE_DOWNLOAD_GUBUN_value:
                            responseModel = fileCommonUtil.DownloadFile();
                            break;
                        case FileCommonUtil.FILE_DELETE_GUBUN_value:
                            responseModel = fileCommonUtil.DeleteFile();
                            break;

                        case FileCommonUtil.FILE_INFO_GUBUN_value:
                            responseModel = fileCommonUtil.GetFileInfo();
                            break;

                        case FileCommonUtil.FILE_LIST_GUBUN_value:
                            responseModel = fileCommonUtil.GetFileNameList();
                            break;
                        case FileCommonUtil.FILE_RENAME_GUBUN_value:
                            responseModel = fileCommonUtil.FileRename();
                            break;
                    }
                }
                //catch(ServiceException ex)
                //{
                //    responseModel = ResponseModel.ErrorMessage(ex);
                //}
                catch (Exception ex)
                {
                    responseModel = ResponseModel.ErrorMessage(ex);
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

                var appOptionsAccessor = app.ApplicationServices.GetService<IOptions<AppOptions>>();
                var requestDataParser = app.ApplicationServices.GetService<RequestDataParser>();
                var xmlCommonUtil = app.ApplicationServices.GetService<XMLCommonUtil>();
                var fileCommon = app.ApplicationServices.GetService<FileCommonUtil>();

                var appOptions = appOptionsAccessor.Value ?? new AppOptions();

                RequestModel requestModel = null;
                ResponseModel responseModel = null;

                try
                {
                    requestModel = await requestDataParser.Parse();
                    xmlCommonUtil.SetReqestModel(requestModel);

                    /*웹서버 세션 정보 체크*/
                    string sessionID = xmlCommonUtil.sessionID;
                    string client_SessionID = xmlCommonUtil.ClientSessionID;

                    if (XMLCommonUtil.SESSION_CHECK && client_SessionID == "")
                    {
                        //xmldata += xmlCommonUtil.returnErrorMSGXML("세션정보가 빈값이거나, 세션 정보를 넘기지 않습니다. 로그인 후 사용바랍니다.");
                        //_context.Response.Write(xmldata);
                        //return;

                        throw new ServiceException("세션정보가 빈값이거나, 세션 정보를 넘기지 않습니다. 로그인 후 사용바랍니다.");
                    }
                    else if (XMLCommonUtil.SESSION_CHECK && sessionID != client_SessionID)
                    {
                        //xmldata += xmlCommonUtil.returnErrorMSGXML("세션 정보가 올바르지 않습니다. 다시 로그인한 후 사용바랍니다.");
                        //_context.Response.Write(xmldata);
                        //return;

                        throw new ServiceException("세션 정보가 올바르지 않습니다. 다시 로그인한 후 사용바랍니다.");
                    }

                    /*파일 업로드 갯수 제한*/
                    if (context.Request.Form.Files.Count == 0)
                    {
                        //xmldata += xmlCommonUtil.returnErrorMSGXML("업로드 파일이 없습니다.");
                        //_context.Response.Write(xmldata);
                        //return;

                        throw new ServiceException("업로드 파일이 없습니다.");
                    }
                    else if (context.Request.Form.Files.Count > 1)
                    {
                        //xmldata += xmlCommonUtil.returnErrorMSGXML("파일은 하나씩만 업로드 가능합니다.");
                        //_context.Response.Write(xmldata);
                        //return;

                        throw new ServiceException("파일은 하나씩만 업로드 가능합니다.");
                    }

                    //저장위치를 지정
                    //string uploadDir = ConfigurationManager.AppSettings["etc"];
                    var uploadDir = appOptions.Etc;

                    uploadDir = fileCommon.ATTACHMENT_UPLOAD_PATH;

                    //디렉토리 체크
                    if (!Directory.Exists(uploadDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(uploadDir);
                        }
                        catch (Exception ex)
                        {
                            throw new ServiceException("파일경로를 만들 수 없습니다.", ex);
                        }
                    }

                    // 파일을 저장하고,(+이미지 파일일 경우 섬네일이미지를 생성하고 - 구현안됨)

                    foreach (var file in context.Request.Form.Files)
                    {
                        //HttpPostedFile file = _context.Request.Files[fileKey];

                        //파일이름을 지정하여 저장할 것인가? file_name
                        string namedFileName = xmlCommonUtil.RequestData.GetValue(FileCommonUtil.ATTACHMENT_FILENAME_key);
                        string fileFullPath = String.Empty;

                        if (!string.IsNullOrEmpty(namedFileName))
                        {
                            fileFullPath = uploadDir + namedFileName;
                        }
                        else
                        {
                            fileFullPath = uploadDir + file.FileName;
                        }

                        fileFullPath = fileCommon.MakeUniqueFileName(fileFullPath);
                        //file.SaveAs(fileFullPath);

                        using (var stream = file.OpenReadStream())
                        {
                            using (var destinationStream = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write))
                            {
                                await stream.CopyToAsync(destinationStream);
                                await destinationStream.FlushAsync();
                                destinationStream.Close();
                            }
                            stream.Close();
                        }

                        FileInfo fileInfo = new FileInfo(fileFullPath);
                        var attachment_filename = fileInfo.Name;
                        var attachment_fileformat = fileInfo.Extension;
                        var attachment_filesize = fileInfo.Length;

                        // 디비를 저장하고
                        // 디비에 저장할 것인가? db_work
                        string isDBWork = xmlCommonUtil.RequestData.GetValue(FileCommonUtil.DB_WORK_GUBUN_value);
                        string return_msg = string.Empty;
                        string resultXML = String.Empty;

                        if (!string.IsNullOrEmpty(isDBWork) && !isDBWork.Equals("pass"))
                        {
                            //db_work=pass 명시적으로 표시할 경우 디비 작업 없음.
                        }
                        else
                        {
                            responseModel = fileCommon.Attachment_C(
                                attachment_filename,
                                attachment_fileformat,
                                attachment_filesize,
                                out resultXML);
                        }
                    }


                }
                catch (ServiceException ex)
                {
                    responseModel = ResponseModel.ErrorMessage(ex);
                }
                catch (Exception ex)
                {
                    responseModel = ResponseModel.Message("100", "파일업로드 에러가 발생했습니다.");
                    //responseModel = ResponseModel.ErrorMessage(ex);
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
