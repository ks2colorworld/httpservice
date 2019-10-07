/**
 * 매개변수 설명
 * //(필요없음)proc = _SendMobileMSG
 * 
 * gubun = send_sms (사용안함 -> send_public_sms)
 * sessionID = {사용자 인증 후 발급받은 번호}
 * pole_num = {기기구분값-로그기록에서 사용}
 * 
 * receiver_phone = {받을 사람 전화번호 -형식 :010-1234-5678}
 * sender_phone = {보내는 사람 전화번호 -형식 : 010-1234-5678}
 * 
 * msg_type = {MMS|SMS|URL}
 * 
 * msg = {
 *      mms 발송 내용 - MMS일 경우 - 한글 1000자 기준, 
 *      SMS일 경우 - 한글 40자/영문 80자, 
 *      URL일 경우 - “http:// wap.test.co.kr 제목타이틀”
 * }
 * 
 * (아래 이하는 msg_type = MMS일 경우)
 * mms_subject = {mms 발송 제목 - 한글 20자 기준}
 * 
 * attachment_key = {로컬 파일 사용시 mms_file_name1~5, mms_file_url1~5 대신 사용, 파일 1개 기준}
 * 
 * mms_file_cnt = {1~5사이 값, attachment_key값이 있을 경우 무시}
 * mms_file_type1~5 = IMG (자세한 사항 아래 참조, attachment_key값을 사용할 경우 mms_file_type1만 사용)
 * 
 * mms_file_name1~5 = {발송할 이미지 파일명 -예: xxx.jpg, attachment_key값이 있을 경우 무시}
 * mms_file_url1~5 = {발송할 이미지를 가져올 url -예: http://mediapole2.cafe24.com/.../savePic/, attachment_key값이 있을 경우 무시}
 */
using HttpService.Models;
using HttpService.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public class MobileMessageManager : IMobileMessageManager
    {
        public MobileMessageManager(
            IFileManager fileManager,
            IDatabaseManager databaseManager,
            IHttpContextManager httpContextManager,
            IResponsePreprocessManager responsePreprocessManager,
            IOptionsMonitor<AppOptions> appOptionsMonitor)
        {
            this.fileManager = fileManager;
            this.databaseManager = databaseManager;
            this.httpContextManager = httpContextManager;
            this.responsePreprocessManager = responsePreprocessManager;
            appOptions = appOptionsMonitor.CurrentValue;
        }

        public async Task<ResponseModel> Send(RequestModel model)
        {
            ResponseModel result = null;
            var mobileMessageType = GetMSG_TYPE(model);
            DataSet resultDataSet = null;

            try
            {
                var mmsSeq = -1;
                if (mobileMessageType == MobileMessageType.MMS)
                {
                    mmsSeq = await SaveMMSFiles(model);
                }

                resultDataSet = await SendSms(model, mmsSeq);

                result = responsePreprocessManager.ProcessDataSet(resultDataSet);
            }
            catch (Exception ex)
            {
                result = responsePreprocessManager.ProcessException(ex);
            }

            return result;
        }

        private async Task<int> SaveMMSFiles(RequestModel model)
        {
            var mms_subject = GetMMS_SUBJECT(model);
            var mms_body = GetSEND_MSG_STRING(model);
            var filePath = await fileManager.ReturnFileFullPath(model);
            IEnumerable<IMmsFile> files = new List<IMmsFile>();

            if (!String.IsNullOrEmpty(filePath))
            {
                var fileType = model.GetValue($"{Constants.FILE_TYPE}1");

                files = new List<IMmsFile>
                {
                    new LocalMmsFile(fileType, filePath)
                };
            }
            else
            {
                files = GetFILE_LIST(model);

                // download remote files
                foreach (var file in files)
                {
                    await fileManager.SaveRemoteFile(file.FileDownloadUrl, file.FileLocalPath);
                }
            }

            string proc_name = Constants.PROC_SEND_MOBILE_MSG_CUD;
            if (!String.IsNullOrEmpty(appOptions.DbSchema))
            {
                proc_name = $"{appOptions.DbSchema}.{proc_name}";
            }

            var parameters = new Dictionary<string, object>();            
            parameters.Add(Constants.GUBUN_KEY_STRING, Constants.GUBUN_SAVE_MMS_FILE);
            parameters.Add(Constants.OPERATOR_IP_GUBUN, httpContextManager.GetRemoteIpAddress());
            parameters.Add(Constants.OPERATOR_KEY_GUBUN, model.GetValue(Constants.OPERATOR_KEY_GUBUN));

            //*
            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;
            if (include_organization_key)
            {

                parameters.Add(Constants.ORGANIZATION_KEY_GUBUN, model.GetValue(Constants.ORGANIZATION_KEY_GUBUN));
            }
            //*/

            parameters.Add(Constants.FILE_CNT_KEY, files.Count());
            parameters.Add(Constants.MMS_BODY_KEY, mms_body);
            parameters.Add(Constants.MMS_SUBJECT_key, mms_subject);

            var fileNumber = 1;

            foreach (var file in files)
            {
                parameters.Add($"{Constants.FILE_TYPE_KEY}{fileNumber}", file.FileType.ToString());
                parameters.Add($"{Constants.FILE_NAME_KEY}{fileNumber}", file.FileLocalPath);

                fileNumber++;
            }

            DataSet dataSet = null;

            try
            {
                dataSet = await databaseManager.ExecuteQueryAsync(proc_name, parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (dataSet == null && dataSet.Tables.Count == 0 || !dataSet.Tables[0].Columns.Contains("mms_seq"))
            {
                throw new ServiceException($"{nameof(SaveMMSFiles)} 리턴 데이터에 오류가 있습니다");
            }

            var row = dataSet.Tables[0].Select().FirstOrDefault();
            int mmsSeq = -1;

            if (row != null)
            {
                var mms_seqRaw = $"{row["mms_seq"]}";
                if (!int.TryParse(mms_seqRaw, out mmsSeq))
                {
                    throw new ServiceException($"MMS 발송 중 오류 발생 (첨부파일 데이터의 데이터베이스 저장이 올바르게 이루어지지 않았습니다. (seq={mms_seqRaw}))");
                }
            }

            return mmsSeq;
        }

        private async Task<DataSet> SendSms(RequestModel model, int mmsSeq = -1)
        {
            var sender_phone = model.GetValue(Constants.SENDER_PHONE_key);
            var receiver_phone = model.GetValue(Constants.RECEIVER_PHONE_key);
            var send_date = model.GetValue(Constants.DATE_key);
            var msg_type = model.GetValue(Constants.MSG_TYPE_key);
            var msg = model.GetValue(Constants.MSG_key);

            var messageType = MobileMessageType.SMS;
            if (Enum.TryParse<MobileMessageType>(msg_type, out messageType))
            {
                messageType = MobileMessageType.SMS;
            }

            if (ValidatePhoneNumer(sender_phone))
            {
                throw new ServiceException($"[{ Constants.SENDER_PHONE_key }={sender_phone }] 전화번호 형식이 올바르지 않습니다.(예:010-1234-5678)");
            }

            if (!ValidatePhoneNumer(receiver_phone))
            {
                throw new ServiceException($"[{ Constants.RECEIVER_PHONE_key }={receiver_phone }] 전화번호 형식이 올바르지 않습니다.(예:010-1234-5678)");
            }

            if (!String.IsNullOrEmpty(send_date) && !ValidateDateTime(send_date))
            {
                throw new ServiceException($"[{Constants.DATE_key }={send_date }]예약 날짜 형식이 올바르지 않습니다. (예:{DateTime.Now:yyyy-MM-dd HH:mm})");
            }

            string proc_name = Constants.PROC_SEND_MOBILE_MSG_CUD;
            if (!String.IsNullOrEmpty(appOptions.DbSchema))
            {
                proc_name = $"{appOptions.DbSchema}.{proc_name}";
            }

            var parameters = new Dictionary<string, object>();
         
            parameters.Add(Constants.GUBUN_KEY_STRING, model.GetValue(Constants.GUBUN_KEY_STRING));
            parameters.Add(Constants.OPERATOR_IP_GUBUN, httpContextManager.GetRemoteIpAddress());
            parameters.Add(Constants.OPERATOR_KEY_GUBUN, model.GetValue(Constants.OPERATOR_KEY_GUBUN));
            
            //*
            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;
            if (include_organization_key)
            {
                parameters.Add(Constants.ORGANIZATION_KEY_GUBUN, model.GetValue(Constants.ORGANIZATION_KEY_GUBUN));
            }
            //*/

            parameters.Add(Constants.TRAN_PHONE, receiver_phone);
            parameters.Add(Constants.TRAN_CALLBACK, sender_phone);

            if (!String.IsNullOrEmpty(send_date))
            {
                parameters.Add(Constants.TRAN_DATE, send_date);
            }

            parameters.Add(Constants.TRAN_TYPE, messageType);

            if (messageType == MobileMessageType.MMS)
            {
                parameters.Add(Constants.TRAN_ETC4, mmsSeq);
            }

            parameters.Add(Constants.TRAN_MSG, msg);

            /* 추가 파라메터 */
            for (int seq = 1; seq < 4; seq++)
            {
                string pKey = "tran_etc" + seq.ToString();

                if (model.HasKey(pKey))
                {
                    parameters.Add(pKey, model.GetValue(pKey));
                }
            }

            var dataset = await databaseManager.ExecuteQueryAsync(proc_name, parameters);

            return dataset;
        }

        private MobileMessageType GetMSG_TYPE(RequestModel model)
        {
            MobileMessageType result = MobileMessageType.SMS;

            var messageTypeRaw = model.GetValue(Constants.MSG_TYPE_key);

            if (!Enum.TryParse<MobileMessageType>(messageTypeRaw, out result))
            {
                result = MobileMessageType.SMS;
            }

            return result;
        }

        private int GetFILE_COUNT(RequestModel model)
        {
            var fileCountRaw = model.GetValue(Constants.MMS_FILE_CNT_key);
            int count = 0;

            if (!int.TryParse(fileCountRaw, out count))
            {
                //count = -1;
                throw new ServiceException($"[{ fileCountRaw }]MMS 첨부파일 데이터 중 일부가 올바르지 않습니다.({Constants.MMS_FILE_CNT_key})");
            }

            return count;
        }

        private string GetSEND_MSG_STRING(RequestModel model)
        {
            return model.GetValue(Constants.MSG_key);
        }

        private string GetMMS_SUBJECT(RequestModel model)
        {
            return model.GetValue(Constants.MMS_SUBJECT_key);
        }

        private IEnumerable<IMmsFile> GetFILE_LIST(RequestModel model)
        {
            var fileCount = GetFILE_COUNT(model);

            if (fileCount <= 0)
            {
                throw new ServiceException($"MMS 첨부파일 데이터 갯수가 설정되지 않았습니다.");
            }

            if (fileCount > 5)
            {
                throw new ServiceException($"MMS 첨부파일 데이터 갯수는 5개를 초과할 수 없습니다.({Constants.MMS_FILE_CNT_key}={ fileCount })");
            }

            var files = new List<IMmsFile>();

            for (int i = 1; i <= fileCount; i++)
            {
                var fileTypeKey = $"{Constants.FILE_TYPE}{i}";
                var fileNameKey = $"{Constants.FILE_NAME }{i}";
                var fileUrlKey = $"{Constants.FILE_URL }{i}";

                string fileType = model.GetValue(fileTypeKey);
                string fileName = model.GetValue(fileNameKey);
                string fileUrl = model.GetValue(fileUrlKey);

                if (String.IsNullOrEmpty(fileType) ||
                    String.IsNullOrEmpty(fileName) ||
                    String.IsNullOrEmpty(fileUrl))
                {
                    throw new ServiceException($"MMS 첨부파일 데이터 중 일부가 올바르지 않습니다. ({fileTypeKey}={fileType}, {fileNameKey}={fileName}, {fileUrlKey}={fileUrl})");
                }

                var mmsFile = new RemoteMmsFie(fileType, fileUrl, appOptions.Mms, fileName);
                files.Add(mmsFile);
            }

            return files;
        }

        private bool ValidatePhoneNumer(string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            string patternPhoneNumber = @"^\d{2,3}-\d{3,4}-\d{4}$";

            return Regex.IsMatch(phoneNumber, patternPhoneNumber);
        }

        private bool ValidateDateTime(string dateAndTime)
        {
            if (String.IsNullOrEmpty(dateAndTime))
            {
                return false;
            }
            DateTime _;
            return DateTime.TryParse(dateAndTime, out _);
        }

        private readonly IFileManager fileManager;
        private readonly IHttpContextManager httpContextManager;
        private readonly IDatabaseManager databaseManager;
        private readonly IResponsePreprocessManager responsePreprocessManager;
        private readonly AppOptions appOptions;
    }
}
