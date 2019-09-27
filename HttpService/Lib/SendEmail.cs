using HttpService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Lib
{
    /* 매개변수 설명
 * gubun = send_email
 * sessionID = {사용자 인증 후 발급받은 번호}
 * pole_num = {기기구분값-로그기록에서 사용}
 * 
 * to = {|로 구분}
 * attachment_key = {옵션 - 존재하는 키 값을 넘김, 첨부파일로 보내짐}
 * p_type = {direct|form(추후 구현)}
 * 
 * p_type = direct ->   subject = {보낼 제목}
 *                      message = {보낼 내용}
 * p_type = form ->     form_key = {등록된 html 폼 key - 등록시 message내 이미지(attachment_key) 추가여부 체크, message내 날짜/시간 추가여부 체크}
 *                      message_params = {|구분, 옵션 - 값이 있으면 해당 값에 맞춰서 변경됨 }
 *                      subject_params = {|구분, 옵션 - 값이 있으면 해당 값에 맞춰서 변경됨}
 */
    public class SendEmail
    {
        private Gmail mensagem;
        private HttpContext _httpContext;
        private XMLCommonUtil xmlCommonUtil;
        private readonly FileCommonUtil fileCommonUtil;

        private const string TO_key = "to";
        private const string P_TYPE_key = "p_type";

        private const string SUBJECT_key = "subject";
        private const string MESSAGE_key = "message";

        private const string FORM_KEY_key = "form_key";//추후 구현할 키값
        private const string MESSAGE_PARAMS_key = "message_params";//추후 구현할 키값
        private const string SUBJECT_PARAMS_key = "subject_params";//추후 구현할 키값

        private const string P_TYPE_DIRECT_value = "direct";
        private const string P_TYPE_FORM_value = "form";//추후 구현할 value값
        
        
        
        public SendEmail(
            XMLCommonUtil xmlCommonUtil,
            FileCommonUtil fileCommonUtil,
            Gmail gmail,
            IHttpContextAccessor httpContextAccessor)
        {
            this.xmlCommonUtil = xmlCommonUtil;
            this.fileCommonUtil = fileCommonUtil;
            this.mensagem = gmail;
            this._httpContext = httpContextAccessor.HttpContext;

            //mensagem = new Gmail();
            //this._httpContext = HttpContext.Current;
            //this.xmlCommonUtil = new XMLCommonUtil();
        }

        public ResponseModel send()
        {
            mensagem.auth("no_reply_01@kesso.kr", "lee1004"); //1)web.config포함 -> 2)cms관리 mail 포함
            mensagem.fromAlias = "kesso.kr";//1)web.config포함 -> 2)cms관리 mail 포함

            mensagem.Priority = 0;

            if (this.P_TYPE != P_TYPE_DIRECT_value)
            {
                return xmlCommonUtil.ResponseWriteErrorMSG("[p_type = direct] 이외 아직까지 구현되지 않았습니다.");
            }

            mensagem.Html = false;

            if (string.IsNullOrEmpty(this.SUBJECT))
            {
                return xmlCommonUtil.ResponseWriteErrorMSG("제목이 없습니다.[subject = ?]");
            }
            mensagem.Subject = this.SUBJECT;//"test 한글2";
            mensagem.Message = this.MESSAGE;// "test 한글3";

            if (TO.Length == 0)
            {
                return xmlCommonUtil.ResponseWriteErrorMSG("받을 사람이 없습니다.[to = ?]");
            }
            foreach (string to in TO)
            {
                mensagem.To = to;
            }

            //FileCommonUtil fcu = new FileCommonUtil();
            //string fileFullPath = fcu.CheckAttachmentKeyAndReturnFullFilePath();//첨부파일 1개기준

            string fileFullPath = fileCommonUtil.CheckAttachmentKeyAndReturnFullFilePath();

            if (!string.IsNullOrEmpty(fileFullPath))
            {
                mensagem.attach(fileFullPath);
            }

            bool success = mensagem.send();

            if (!success)
            {
                //추후 error로그 저장 후 메시지 전송할 것.
#if FOR_TEST
            return xmlCommonUtil.ResponseWriteErrorMSG("UpdateTwitPic Fail!!(" + fileFullPath + ")");
#else
                return xmlCommonUtil.ResponseWriteErrorMSG("UpdateTwitPic Fail!!");
#endif
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
                    null :
                    xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_KEY_key].ToString();
            }
        }
        //*/

        private string[] TO
        {
            get
            {
                string to = xmlCommonUtil.QueryString[TO_key] == null ?
                    string.Empty :
                    xmlCommonUtil.QueryString[TO_key].ToString();
                string[] tos = to.Split(new string[] { ",", "|", " " }, StringSplitOptions.RemoveEmptyEntries);

                return tos;
            }
        }
        private string MESSAGE
        {
            get
            {
                return xmlCommonUtil.QueryString[MESSAGE_key] == null ?
                    string.Empty :
                    xmlCommonUtil.QueryString[MESSAGE_key].ToString();
            }
        }
        private string SUBJECT
        {
            get
            {
                return xmlCommonUtil.QueryString[SUBJECT_key] == null ?
                    null :
                    xmlCommonUtil.QueryString[SUBJECT_key].ToString();
            }
        }
        private string P_TYPE
        {
            get
            {
                return xmlCommonUtil.QueryString[P_TYPE_key] == null ?
                    null :
                    xmlCommonUtil.QueryString[P_TYPE_key].ToString();
            }
        }
    }




    /*
     This is a DLL usefull to send e-mails using GMail.

    This DLL also zip the attachments and is easy to use without SMTP configuration.

    This DLL is developed using Microsoft C# (CS) .Net with .Net Framework 2.0


    You can use this DLL (i.e. in C#) declaring it like above:

    using GmailSend;
    gmail mensagem = new gmail();
    mensagem.auth("mailto@domain.com", "password");
    mensagem.assunto = "Aqui esta o Assunto";
    mensagem.mensagem = "Aqui Está a Mensagem";
    mensagem.To = "destino@dominio.qqcoisa";
    mensagem.To = "destino2@dominio.qqcoisa";
    mensagem.Cc = "teste@123.com";
    mensagem.Cc = "teste2@123.com";
    mensagem.Bcc = "hide@cool.com";
    mensagem.Bcc = "hide2@cool.com";
    mensagem.Priority = 1;
    mensagem.Attach = @"c:\abc.txt";
    mensagem.Attach = @"c:\bcd.sql";
    mensagem.zip("nomedoficheiro.zip", "password");
    mensagem.send();
     */
}
