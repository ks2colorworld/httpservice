using HttpService.Lib;
using HttpService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DefaultController: ControllerBase
    {
        public DefaultController(
            XMLCommonUtil xmlCommonUtil,
            ExcelDownload excelDownload,
            SendMobileMSGCommon sendMobileMSGCommon,
            SendEmail sendEmail,
            UploadAndPostTwitPic uploadAndPostTwitPic)
        {
            this.xmlCommonUtil = xmlCommonUtil;
            this.excelDownload = excelDownload;
            this.sendMobileMSGCommon = sendMobileMSGCommon;
            this.sendEmail = sendEmail;
            this.uploadAndPostTwitPic = uploadAndPostTwitPic;
        }

        [HttpGet]
        public IActionResult Get()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public IActionResult Post(RequestModel model)
        {
            string gubun = xmlCommonUtil.GUBUN;
            
            if (!xmlCommonUtil.CheckSessionID() && !PassCheckSessionID)
            {
                // TODO 반환값 정의
                return StatusCode(401);
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

                    return Ok(userLoginData);
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
                        return File(fileResponse.Content, fileResponse.ContentType, fileResponse.FileName);
                    }
                    else
                    {
                        return Ok(response);
                    }
                //break;

                case "send_email":
                    //SendEmail se = new SendEmail();
                    //se.send();
                    var sendemailResult = sendEmail.send();
                    return Ok(sendemailResult);
                //break;

                case "upload_twitpic":
                    //UploadAndPostTwitPic ut = new UploadAndPostTwitPic();
                    //ut.UploadAndPost();

                    var uploadAndPostTwitPicResult = uploadAndPostTwitPic.UploadAndPost();
                    return Ok(uploadAndPostTwitPicResult);
                //break;

                /*기본 xml return *********************************/
                default:
                    //if (!string.IsNullOrEmpty(xmlCommonUtil.QueryString[XMLCommonUtil.PROC_KEY_STRING]))
                        if (!string.IsNullOrEmpty(xmlCommonUtil.RequestData.Proc))
                        {
                        var dataResult = xmlCommonUtil.WriteXML(false);
                        return Ok(dataResult);
                    }
                    else
                    {
                        var messageResult = xmlCommonUtil.ResponseWriteErrorMSG("필수 매개변수를 넘기지 않았습니다.");
                        // TODO 반환값 정의
                        return Ok(messageResult);

                    }
                    //break;
            }

            //return Ok();
            return StatusCode(400);
        }

        

        private readonly string[] noCheckSessionIDGubun = {
                                                       "menu_ctrl_bind",
                                                   };
        //"send_public_sms",

        private bool PassCheckSessionID
        {
            get
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
            }
        }

        private readonly XMLCommonUtil xmlCommonUtil; //= new XMLCommonUtil();
        private readonly ExcelDownload excelDownload;
        private readonly SendMobileMSGCommon sendMobileMSGCommon;
        private readonly SendEmail sendEmail;
        private readonly UploadAndPostTwitPic uploadAndPostTwitPic;
    }
}
