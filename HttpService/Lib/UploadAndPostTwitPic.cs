#define FOR_TEST //test를 위해 상세한 정보를 리턴할 것인가?

using System;
using System.Collections.Generic;
using System.Web;
using HttpService.Models;
//using GmailSend;
using Microsoft.AspNetCore.Http;

namespace HttpService.Lib
{
    /* 매개변수 설명
  * gubun = upload_twitpic
  * sessionID = {사용자 인증 후 발급받은 번호}
  * pole_num = {기기구분값-로그기록에서 사용}
  * 
  * twit_id = {|구분}
  * message = {@twit_id, 필요한 빈값, twitpic-url 포함하여 140자}
  * attachment_key = {첨부파일 고유키}
  */
    public class UploadAndPostTwitPic
    {
        private readonly HttpContext _httpContext;
        private readonly XMLCommonUtil xmlCommonUtil;
        private readonly FileCommonUtil fileCommonUtil;
        private readonly Gmail gmail;

        private const string TWIT_ID_key = "twit_id";
        private const string MESSAGE_key = "message";

        public UploadAndPostTwitPic(
            IHttpContextAccessor httpContextAccessor,
            XMLCommonUtil xmlCommonUtil,
            FileCommonUtil fileCommonUtil,
            Gmail gmail)
        {
            this._httpContext = httpContextAccessor.HttpContext;
            this.xmlCommonUtil = xmlCommonUtil;
            this.fileCommonUtil = fileCommonUtil;
            this.gmail = gmail;
            //this._httpContext = HttpContext.Current;
            //this.xmlCommonUtil = new XMLCommonUtil();
        }

        public ResponseModel UploadAndPost()
        {
            //FileCommonUtil fcu = new FileCommonUtil();
            //string fileFullPath = fcu.CheckAttachmentKeyAndReturnFullFilePath();//첨부파일 1개기준

            string fileFullPath = fileCommonUtil.CheckAttachmentKeyAndReturnFullFilePath(); //첨부파일 1개기준

            if (string.IsNullOrEmpty(fileFullPath))
            {
                return xmlCommonUtil.ResponseWriteErrorMSG("Could not find file path.!!(" + fileFullPath + ")");
            }

            //gmail mensagem = new gmail();
            var mensagem = gmail;
            mensagem.auth("no_reply_01@kesso.kr", "lee1004"); //1)web.config포함 -> 2)cms관리 포함
            mensagem.fromAlias = "kesso.kr";//1)web.config포함 -> 2)cms관리 포함
            mensagem.To = "service_kesso.2020@twitpic.com"; //1)web.config포함 -> 2)cms관리 포함

            mensagem.Message = "";
            mensagem.Priority = 0;
            mensagem.Html = false;

            mensagem.Subject = this.MESSAGE;//"@ks2colorworld test 한글2 ";

            mensagem.attach(fileFullPath);

            /*
            if (!string.IsNullOrEmpty(this.ATTACHMENT_KEY))//attachment_key
            {
                FileCommonUtil fcu = new FileCommonUtil();
                mensagem.attach(fcu.ReturnFileFullPath(this.ATTACHMENT_KEY));
                //mensagem.attach(@"C:\_WAS\1_service_kesso_kr\upload\photoservice\-\photoservice_20100830121937.jpg");
            }
            */
            //mensagem.zip("nomedoficheiro.zip", "password");
            bool success = mensagem.send();

            if (!success)
            {
                //추후 error로그 저장 후 메시지 전송할 것.
#if FOR_TEST
           return xmlCommonUtil.ResponseWriteErrorMSG("UpdateTwitPic Fail!!(" + fileFullPath + ")");
#else
                return xmlCommonUtil.ResponseWriteErrorMSG("UpdateTwitPic Fail!!");
#endif
                //return;
            }

            //추후 로그 저장 후 메시지 전송할 것.
#if FOR_TEST
        return xmlCommonUtil.ResponseWriteMSG("1", "UpdateTwitPic Success!!(" + fileFullPath + ")");
#else
           return xmlCommonUtil.ResponseWriteMSG("1", "UpdateTwitPic Success!!");
#endif
        }

        /* 사용안함.
        private string CheckAttachmentKeyAndReturnFullFilePath()
        {
            string fileFullPath = null;

            if (!string.IsNullOrEmpty(this.ATTACHMENT_KEY))//attachment_key
            {
                FileCommonUtil fcu = new FileCommonUtil();
                fileFullPath = fcu.ReturnFileFullPath(this.ATTACHMENT_KEY);
            }
            return fileFullPath;
        }

        private string ATTACHMENT_KEY
        {
            get
            {
                return xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_KEY_key] == null ?
                    string.Empty :
                    xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_KEY_key].ToString();
            }
        }
        //*/
        private string TWIT_ID
        {
            get
            {
                string return_str = string.Empty;
                string id = String.Empty;
                    //xmlCommonUtil.QueryString[TWIT_ID_key] == null ?
                    //string.Empty :
                    //xmlCommonUtil.QueryString[TWIT_ID_key].ToString();

                if (xmlCommonUtil.RequestData.Data.ContainsKey(TWIT_ID_key))
                {
                    id = xmlCommonUtil.RequestData.Data[TWIT_ID_key];
                }

                id = id.Replace("@", " ");
                string[] ids = id.Split(new string[] { ",", "|", " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in ids)
                {
                    if (string.IsNullOrEmpty(item.Trim()))
                    {
                        continue;
                    }

                    return_str += "@" + item + " ";
                }

                return return_str;
            }
        }
        private string MESSAGE
        {
            get
            {
                string return_str = this.TWIT_ID;
                string msg = String.Empty;
                //xmlCommonUtil.QueryString[MESSAGE_key] == null ?
                //string.Empty :
                //xmlCommonUtil.QueryString[MESSAGE_key].ToString();

                if (xmlCommonUtil.RequestData.Data.ContainsKey(MESSAGE_key))
                {
                    msg = xmlCommonUtil.RequestData.Data[MESSAGE_key];
                }

                return return_str + " " + msg;
            }
        }
    }
}
