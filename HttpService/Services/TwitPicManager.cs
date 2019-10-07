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
    public interface ITwitPicManager
    {
        Task<ResponseModel> Post(RequestModel model);
    }

    public class TwitPicManager: ITwitPicManager
    {
        public TwitPicManager(
            Gmail gmail,
            IFileManager fileManager,
            IOptionsMonitor<EmailOptions> emailOptionsMonitor,
            IOptionsMonitor<GmailOptions> gmailOptionsMonitor)
        {
            this.gmail = gmail;
            this.fileManager = fileManager;
            emailOptions = emailOptionsMonitor.CurrentValue;
            gmailOptions = gmailOptionsMonitor.CurrentValue;
        }


        public async Task<ResponseModel> Post(RequestModel model)
        {
            var filePath = await fileManager.ReturnFileFullPath(model);

            if (String.IsNullOrEmpty(filePath))
            {
                throw new ServiceException($"Could not find file path.!! ({ filePath })");
            }

            gmail.auth(gmailOptions.Username, gmailOptions.Password);
            gmail.fromAlias = emailOptions.SenderName;
            gmail.To = "service_kesso.2020@twitpic.com";
            gmail.Message = String.Empty;
            gmail.Priority = 0;
            gmail.Html = false;
            gmail.Subject = GetMessage(model);

            gmail.attach(filePath);

            var success = gmail.send();

            if (!success)
            {
                throw new ServiceException($"UpdateTwitPic Fail!!({filePath})");
            }

            return ResponseModel.Message("1", $"UpdateTwitPic Success!!({ filePath })");
        }

        private string GetTWIT_ID(RequestModel model)
        {
            string return_str = string.Empty;
            string id = String.Empty;


            id = model.GetValue(Constants.TWIT_ID_key);

            id = id.Replace("@", " ");
            string[] ids = id.Split(new string[] { ",", "|", " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in ids)
            {
                if (String.IsNullOrEmpty(item.Trim()))
                {
                    continue;
                }

                return_str += "@" + item + " ";
            }

            return return_str;
        }

        private string GetMessage(RequestModel model)
        {
            var twitId = GetTWIT_ID(model);
            var message=model.GetValue(Constants.MESSAGE_key);

            return $"{twitId} {message}";
        }

        private readonly Gmail gmail;
        private readonly EmailOptions emailOptions;
        private readonly GmailOptions gmailOptions;
        private readonly IFileManager fileManager;
        
    }
}
