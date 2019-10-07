using HttpService.Lib;
using HttpService.Models;
using HttpService.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public class EmailManager : IEmailManager
    {
        public EmailManager(
            IFileManager fileManager,
            IResponsePreprocessManager responsePreprocessManager,
            IOptionsMonitor<EmailOptions> emailOptionsAccessor,
            IOptionsMonitor<GmailOptions> gmailOptionsAccessor,
            Gmail gmail)
        {
            this.fileManager = fileManager;
            this.responsePreprocessManager = responsePreprocessManager;
            this.gmail = gmail;
         
            emailOptions = emailOptionsAccessor.CurrentValue;
            gmailOptions = gmailOptionsAccessor.CurrentValue;
        }

        public async Task<ResponseModel> Send(RequestModel model)
        {
            ResponseModel responseModel = null;

            try
            {
                //gmail.auth("no_reply_01@kesso.kr", "lee1004"); //1)web.config포함 -> 2)cms관리 mail 포함
                //gmail.fromAlias = "kesso.kr";//1)web.config포함 -> 2)cms관리 mail 포함

                gmail.auth(gmailOptions.Username, gmailOptions.Password);
                gmail.fromAlias = emailOptions.SenderName;

                gmail.Priority = 0;

                var pType = model.GetValue(Constants.P_TYPE_key);

                if (pType != Constants.P_TYPE_DIRECT_value)
                {
                    throw new ServiceException("[p_type = direct] 이외 아직까지 구현되지 않았습니다.");
                }

                gmail.Html = false;

                var subject = model.GetValue(Constants.SUBJECT_key);
                var body = model.GetValue(Constants.MESSAGE_key);
                var toRaw = model.GetValue(Constants.TO_key);
                var toList = SplitEmailAddress(toRaw);

                //var attachment = model.GetValue(Constants.ATTACHMENT_KEY_key);

                if (String.IsNullOrEmpty(subject))
                {
                    //return xmlCommonUtil.ResponseWriteErrorMSG("제목이 없습니다.[subject = ?]");

                    throw new ServiceException("제목이 없습니다.[subject = ?]");
                }

                gmail.Subject = subject;//"test 한글2";
                gmail.Message = body;// "test 한글3";

                if (toList.Count() == 0)
                {
                    //return xmlCommonUtil.ResponseWriteErrorMSG("받을 사람이 없습니다.[to = ?]");
                    throw new ServiceException("받을 사람이 없습니다.[to = ?]");
                }

                foreach (string to in toList)
                {
                    gmail.To = to;
                }

                //FileCommonUtil fcu = new FileCommonUtil();
                //string fileFullPath = fcu.CheckAttachmentKeyAndReturnFullFilePath();//첨부파일 1개기준

                //string fileFullPath = fileCommonUtil.CheckAttachmentKeyAndReturnFullFilePath();
                string fileFullPath = await fileManager.ReturnFileFullPath(model);

                if (!String.IsNullOrEmpty(fileFullPath))
                {
                    gmail.attach(fileFullPath);
                }

                bool success = gmail.send();

                if (!success)
                {
                    // TODO 추후 error로그 저장 후 메시지 전송할 것.
                    throw new ServiceException("UpdateTwitPic Fail!!");
                }

                // TODO 추후 로그 저장 후 메시지 전송할 것.

                responseModel = ResponseModel.Message("1", "UpdateTwitPic Success!!");
            }
            catch (Exception ex)
            {
                responseModel = responsePreprocessManager.ProcessException(ex);
            }

            return responseModel;
        }

        private IEnumerable<string> SplitEmailAddress(string address)
        {
            if (String.IsNullOrWhiteSpace(address)) { return new List<string>(); }

            var list = address.Split(new[] { ",", "|", " " }, StringSplitOptions.RemoveEmptyEntries);

            return list;
        }

        private readonly Gmail gmail;
        private readonly IFileManager fileManager;
        private readonly IResponsePreprocessManager responsePreprocessManager;
        private readonly EmailOptions emailOptions;
        private readonly GmailOptions gmailOptions;
    }
}
