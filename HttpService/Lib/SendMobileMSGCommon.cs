using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Configuration;
using System.Text.RegularExpressions;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using HttpService.Options;
using Microsoft.Extensions.Options;
using HttpService.Models;

namespace HttpService.Lib
{
    /* 매개변수 설명
   * //(필요없음)proc = _SendMobileMSG
   * gubun = send_sms (사용안함 -> send_public_sms)
   * sessionID = {사용자 인증 후 발급받은 번호}
   * pole_num = {기기구분값-로그기록에서 사용}

   * receiver_phone = {받을 사람 전화번호 -형식 :010-1234-5678}
   * sender_phone = {보내는 사람 전화번호 -형식 : 010-1234-5678}

   * msg_type = {MMS|SMS|URL}

   * msg = {mms 발송 내용 - MMS일 경우 - 한글 1000자 기준, SMS일 경우 - 한글 40자/영문 80자, URL일 경우 - “http://wap.test.co.kr 제목타이틀”}

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
   * 
   */

    /*mms_file_type 값에 대한 자세한 설명
        TXT
        MMS의 본문 정보
        규격: 최대 2,000byte 이내의 텍스트 파일, 코드형식은 EUC-KR
        * 핸드폰에서 표시 불가능한 특수문자를 입력하는 경우 전송이 실패될 수 있음
        * 본문의 작성은 자유롭게 작성이 가능

        IMG
        MMS의 이미지 정보
        규격: 해상도->176 x 144(권장), 파일크기:->20Kbyte이하, JPG
        *이미지의 해상도는 변경이 가능하지만 특정 폰에서 표시를 하지 못하는 경우가 있음(“컨텐츠에 오류가 있음”으로 표기됨)
        *이미지는 최대 3장까지 지정이 가능. 단, 모든 통신사에 3장이 모두 전송이 되는 것은 아님(수신 폰의 기종이나 통신사의 지원 여부를 확인 할 것)

        ADO
        MMS의 오디오 정보
        규격: 샘플링 16KHz 이하의 MA3형식
        *전송은 되지만 일부 단말기는 포맷을 지원하지 않음

        MOV
        MMS의 비디오 정보
        규격: 해상도-> 176x144(권장),  SKT:->파일크기 350KB 이하,  파일형식 .skm,  KT/LGT->파일크기 280KB 이하, 파일형식 .k3g
     */

    public class SendMobileMSGCommon
    {
        private HttpContext _httpContext;
        private XMLCommonUtil xmlCommonUtil;
        private readonly FileCommonUtil fileCommonUtil;
        private readonly AppOptions appOptions;
        private WebClient webClient;

        private const string RECEIVER_PHONE_key = "receiver_phone";
        private const string SENDER_PHONE_key = "sender_phone";
        private const string MSG_key = "msg";

        private const string DATE_key = "date";

        private const string MMS_SUBJECT_key = "mms_subject";
        private const string MSG_TYPE_key = "msg_type";

        private const string FILE_TYPE = "mms_file_type";//1,2,3,4,5
        private const string FILE_NAME = "mms_file_name";//1,2,3,4,5
        private const string FILE_URL = "mms_file_url";//1,2,3,4,5

        private const string MMS_FILE_CNT_key = "mms_file_cnt";

        //아래는 디비에서 사용하는 key
        private const string FILE_TYPE_KEY = "file_type";
        private const string FILE_NAME_KEY = "file_name";
        private const string FILE_CNT_KEY = "file_cnt";
        private const string MMS_BODY_KEY = "mms_body";

        private readonly string PROC_SEND_MOBILE_MSG_CUD; //= XMLCommonUtil.DB_SCHEMA + "._SendMobileMSG";


        private const string TRAN_PHONE = "tran_phone";
        private const string TRAN_CALLBACK = "tran_callback";
        private const string TRAN_DATE = "tran_date";
        private const string TRAN_MSG = "tran_msg";
        private const string TRAN_TYPE = "tran_type";
        private const string TRAN_ETC4 = "tran_etc4";

        private const string GUBUN_SAVE_MMS_FILE = "save_mms_file_info";
        private const string GUBUN_SEND_SMS = "send_sms";//or "send_public_sms";

        public SendMobileMSGCommon(
            IHttpContextAccessor httpContextAccessor,
            XMLCommonUtil xmlCommonUtil,
            FileCommonUtil fileCommonUtil,
            IOptions<AppOptions> appOptionsAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
            this.xmlCommonUtil = xmlCommonUtil;
            this.appOptions = appOptionsAccessor.Value;
            this.fileCommonUtil = fileCommonUtil;
            //this._httpContext = HttpContext.Current;
            //this.xmlCommonUtil = new XMLCommonUtil();
            this.webClient = new WebClient();

            PROC_SEND_MOBILE_MSG_CUD = $"{appOptions.DbSchema}._SendMobileMSG";
        }

        public ResponseModel SendMobileMSG()
        {
            if (this.MSG_TYPE == MobileMessageType.MMS)
            {
               return this.SendMMS();
            }
            else
            {
                return this.SendSMS(RECEIVER_PHONE, SENDER_PHONE, SEND_MSG_STRING, SEND_DATE);
            }
        }

        //sms발송
        private ResponseModel SendSMS(string receiverPhoneNum, string senderPhoneNum, string msg, string datetime)
        {
            return this.SendSMS(receiverPhoneNum, senderPhoneNum, msg, datetime, this.MSG_TYPE, -1);
        }

        //mms발송 - SendMMS()에서호출
        private ResponseModel SendSMS(string receiverPhoneNum, string senderPhoneNum, string msg, string datetime, MobileMessageType msgType, int mms_seq)
        {
            string out_msg = string.Empty;

            //
            string proc_name = PROC_SEND_MOBILE_MSG_CUD;

            //
            List<SqlParameter> temp_param = new List<SqlParameter>();
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.GUBUN_KEY_STRING, xmlCommonUtil.GUBUN));
            //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_IP_GUBUN, _httpContext.Request.ServerVariables["REMOTE_ADDR"]));
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_IP_GUBUN, _httpContext.Connection.RemoteIpAddress));

            //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_KEY_GUBUN, xmlCommonUtil.QueryString[XMLCommonUtil.OPERATOR_KEY_GUBUN]));
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_KEY_GUBUN, xmlCommonUtil.RequestData.GetValue(XMLCommonUtil.OPERATOR_KEY_GUBUN)));

            //*
            bool include_organization_key = XMLCommonUtil.INCLUDE_ORGANIZATION_KEY;
            if (include_organization_key)
            {
                //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.ORGANIZATION_KEY_GUBUN, xmlCommonUtil.QueryString[XMLCommonUtil.ORGANIZATION_KEY_GUBUN].ToString()));

                temp_param.Add(new SqlParameter("@" + XMLCommonUtil.ORGANIZATION_KEY_GUBUN, xmlCommonUtil.RequestData.GetValue(XMLCommonUtil.ORGANIZATION_KEY_GUBUN)));
            }
            //*/

            temp_param.Add(new SqlParameter("@" + TRAN_PHONE, receiverPhoneNum));
            temp_param.Add(new SqlParameter("@" + TRAN_CALLBACK, senderPhoneNum));

            if (!string.IsNullOrEmpty(datetime))
            {
                temp_param.Add(new SqlParameter("@" + TRAN_DATE, datetime));
            }

            temp_param.Add(new SqlParameter("@" + TRAN_TYPE, msgType));

            if (msgType == MobileMessageType.MMS)
            {
                temp_param.Add(new SqlParameter("@" + TRAN_ETC4, mms_seq));
            }

            temp_param.Add(new SqlParameter("@" + TRAN_MSG, msg));

            /* 추가 파라메터 */
            for (int seq = 1; seq < 4; seq++)
            {
                string pKey = "tran_etc" + seq.ToString();
                //if (xmlCommonUtil.QueryString[pKey] != null)
                //{
                //    temp_param.Add(new SqlParameter("@" + pKey, xmlCommonUtil.QueryString[pKey].ToString()));
                //}

                if (xmlCommonUtil.RequestData.HasKey(pKey))
                {
                    temp_param.Add(new SqlParameter("@" + pKey, xmlCommonUtil.RequestData.GetValue(pKey)));
                }             
            }

            SqlParameter[] sqlparams = temp_param.ToArray();

            return xmlCommonUtil.WriteXML(proc_name, sqlparams, false);
        }

        private ResponseModel SendMMS()
        {
            int MMSSeq = -1;

            //FileCommonUtil fcu = new FileCommonUtil();
            //string filePath = fcu.CheckAttachmentKeyAndReturnFullFilePath();

            string filePath = fileCommonUtil.CheckAttachmentKeyAndReturnFullFilePath();

            //첨부할 파일이 로컬서버(attachment_key)에 있을 경우 - 첨부파일 1개 기준
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SaveMMSFiles(SEND_MSG_STRING, MMS_SUBJECT, filePath, ref MMSSeq);
            }
            //첨부할 파일이 외부서버(http)에 있을 경우
            else
            {
                this.SaveMMSFiles(FILE_COUNT, SEND_MSG_STRING, MMS_SUBJECT, FILE_LIST, ref MMSSeq);
            }

            if (int.Equals(-1, MMSSeq))
            {
                return xmlCommonUtil.ResponseWriteErrorMSG("MMS 발송 중 오류 발생(첨부파일 데이터 디비 저장이 올바르게 이루어지지 않았습니다.)");

            }

            return this.SendSMS(RECEIVER_PHONE, SENDER_PHONE, string.Empty, SEND_DATE, MSG_TYPE, MMSSeq);
        }

        private void SaveMMSFiles(string mms_body, string mms_subject, string filePath, ref int mms_seq)
        {
            this.SaveMMSFiles(-1, mms_body, mms_subject, filePath, null, ref mms_seq);
        }

        private void SaveMMSFiles(int file_cnt, string mms_body, string mms_subject, List<MMSFiles> file_list, ref int mms_seq)
        {
            this.SaveMMSFiles(file_cnt, mms_body, mms_subject, null, file_list, ref mms_seq);
        }

        private void SaveMMSFiles(int file_cnt, string mms_body, string mms_subject, string filePath, List<MMSFiles> file_list, ref int mms_seq)
        {
            if (file_cnt == -1 && string.IsNullOrEmpty(filePath))
            {
                xmlCommonUtil.ResponseWriteErrorMSG("mms_file 저장 중 오류가 발생했습니다.(파일이 존재하지 않거나, 파일 정보를 넘기지 않았습니다.)");
                return;
            }
            string out_msg = string.Empty;

            //
            string proc_name = PROC_SEND_MOBILE_MSG_CUD;

            //
            List<SqlParameter> temp_param = new List<SqlParameter>();
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.GUBUN_KEY_STRING, GUBUN_SAVE_MMS_FILE));
            //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_IP_GUBUN, _httpContext.Request.ServerVariables["REMOTE_ADDR"]));
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_IP_GUBUN, _httpContext.Connection.RemoteIpAddress));
            //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_KEY_GUBUN, xmlCommonUtil.QueryString[XMLCommonUtil.OPERATOR_KEY_GUBUN]));
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_KEY_GUBUN, xmlCommonUtil.RequestData.GetValue(XMLCommonUtil.OPERATOR_KEY_GUBUN)));


            //*
            bool include_organization_key = XMLCommonUtil.INCLUDE_ORGANIZATION_KEY;
            if (include_organization_key)
            {
                //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.ORGANIZATION_KEY_GUBUN, xmlCommonUtil.QueryString[XMLCommonUtil.ORGANIZATION_KEY_GUBUN].ToString()));
                temp_param.Add(new SqlParameter("@" + XMLCommonUtil.ORGANIZATION_KEY_GUBUN, xmlCommonUtil.RequestData.GetValue(XMLCommonUtil.ORGANIZATION_KEY_GUBUN)));
            }
            //*/

            temp_param.Add(new SqlParameter("@" + FILE_CNT_KEY, file_cnt == -1 ? 1 : file_cnt));
            temp_param.Add(new SqlParameter("@" + MMS_BODY_KEY, mms_body));
            temp_param.Add(new SqlParameter("@" + MMS_SUBJECT_key, mms_subject));

            if (file_cnt == -1 && !string.IsNullOrEmpty(filePath))
            {
                //temp_param.Add(new SqlParameter("@" + FILE_TYPE_KEY + "1", xmlCommonUtil.QueryString[FILE_TYPE + "1"]));
                temp_param.Add(new SqlParameter("@" + FILE_TYPE_KEY + "1", xmlCommonUtil.RequestData.GetValue(FILE_TYPE + "1")));
                temp_param.Add(new SqlParameter("@" + FILE_NAME_KEY + "1", filePath));
            }
            //기존 방식
            else
            {
                for (int i = 0; i < file_list.Count; i++)
                {
                    MMSFiles m = file_list[i];

                    try
                    {
                        webClient.DownloadFile(m.FileDownloadUrl, m.FileLocalPath);
                    }
                    catch (Exception ex)
                    {
                        xmlCommonUtil.ResponseWriteErrorMSG("첨부파일을 원격서버에서 로컬로 복사해 오던 중 오류발생하였습니다.(원격 : " + m.FileDownloadUrl + ", 로컬 : " + m.FileLocalPath + ")", ex);
                        return;
                    }
                    int c = i + 1;
                    temp_param.Add(new SqlParameter("@" + FILE_TYPE_KEY + c.ToString(), m.FileType.ToString()));
                    temp_param.Add(new SqlParameter("@" + FILE_NAME_KEY + c.ToString(), m.FileLocalPath));
                }
            }

            SqlParameter[] sqlparams = temp_param.ToArray();

            DataSet ds = null;
            //
            try
            {
                ds = xmlCommonUtil.ReturnDataSet_Common(proc_name, sqlparams, false, out out_msg);
            }
            catch (Exception ex)
            {
                //out_msg = xmlCommonUtil.returnErrorMSGXML("SaveMMSFiles", ex);
                xmlCommonUtil.ResponseWriteErrorMSG("SaveMMSFiles", ex);
                return;
            }

            if (!string.IsNullOrEmpty(out_msg))
            {
                xmlCommonUtil.ResponseWrite(out_msg);
                return;
            }

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0 || !ds.Tables[0].Columns.Contains("mms_seq"))
            {
                xmlCommonUtil.ResponseWriteErrorMSG("SaveMMSFiles 리턴 데이터에 오류가 있습니다.");
                return;
            }

            if (!int.TryParse(ds.Tables[0].Rows[0]["mms_seq"].ToString(), out mms_seq))
            {
                mms_seq = -1;
            }
        }


        private bool checkPhoneNumber(string num)
        {
            bool isOK = false;
            if (string.IsNullOrEmpty(num))
            {
                return isOK;
            }


            string sPattern = "^\\d{2,3}-\\d{3,4}-\\d{4}$";

            if (Regex.IsMatch(num, sPattern))
            {
                isOK = true;
            }

            return isOK;
        }

        private MobileMessageType MSG_TYPE
        {
            get
            {
                //sms, mms, url
                //return xmlCommonUtil.QueryString[MSG_TYPE_key] == null ?
                //    MSGType.SMS :
                //    (MSGType)Enum.Parse(typeof(MSGType), xmlCommonUtil.QueryString[MSG_TYPE_key]);

                if (xmlCommonUtil.RequestData.HasKey(MSG_TYPE_key))
                {
                    return (MobileMessageType)Enum.Parse(typeof(MobileMessageType), xmlCommonUtil.RequestData.GetValue(MSG_TYPE_key));
                }

                return MobileMessageType.SMS;
            }
        }
        private string SENDER_PHONE
        {
            get
            {
                //if (!this.checkPhoneNumber(xmlCommonUtil.QueryString[SENDER_PHONE_key]))
                var senderPhoneNumber = xmlCommonUtil.RequestData.GetValue(SENDER_PHONE_key);
                if (!this.checkPhoneNumber(senderPhoneNumber))
                {
                    //xmlCommonUtil.ResponseWriteErrorMSG(
                    //    "[" + SENDER_PHONE_key + "=" +
                    //    xmlCommonUtil.QueryString["sender_phone"].ToString() +
                    //    "]전화번호 형식이 올바르지 않습니다.(예:010-1234-5678)");

                    // TODO 예외처리가 더 좋지 않을까?
                    xmlCommonUtil.ResponseWriteErrorMSG(
                        "[" + SENDER_PHONE_key + "=" +
                        senderPhoneNumber +
                        "]전화번호 형식이 올바르지 않습니다.(예:010-1234-5678)");
                    
                    return null;
                }
                //010-1234-5678
                //return xmlCommonUtil.QueryString[SENDER_PHONE_key] == null ?
                //    null :
                //    xmlCommonUtil.QueryString[SENDER_PHONE_key].ToString();

                return String.IsNullOrWhiteSpace(senderPhoneNumber) ?
                    null :
                    senderPhoneNumber;
            }
        }
        private string RECEIVER_PHONE
        {
            get
            {
                var receiverPhoneNumber = xmlCommonUtil.RequestData.GetValue(RECEIVER_PHONE_key);

                //if (!this.checkPhoneNumber(xmlCommonUtil.QueryString[RECEIVER_PHONE_key]))

                if (!this.checkPhoneNumber(receiverPhoneNumber))
                {
                    //xmlCommonUtil.ResponseWriteErrorMSG(
                    //    "[" + RECEIVER_PHONE_key + "=" +
                    //    xmlCommonUtil.QueryString[RECEIVER_PHONE_key].ToString() +
                    //    "]전화번호 형식이 올바르지 않습니다.(예:010-1234-5678)");

                    // TODO 예외처리가 더 좋지 않을까?
                    xmlCommonUtil.ResponseWriteErrorMSG(
                   "[" + RECEIVER_PHONE_key + "=" +
                   receiverPhoneNumber +
                   "]전화번호 형식이 올바르지 않습니다.(예:010-1234-5678)");

                    return null;
                }

                //010-1234-5678
                //return xmlCommonUtil.QueryString[RECEIVER_PHONE_key] == null ?
                //    null :
                //    xmlCommonUtil.QueryString[RECEIVER_PHONE_key].ToString();

                return String.IsNullOrWhiteSpace(receiverPhoneNumber) ? 
                    null : 
                    receiverPhoneNumber;
            }
        }
        private string SEND_DATE
        {
            get
            {
                var dateValue = xmlCommonUtil.RequestData.GetValue(DATE_key);

                //if (object.Equals(null, xmlCommonUtil.QueryString[DATE_key]))
                if (String.IsNullOrWhiteSpace(dateValue))
                {
                    return null;
                }

                DateTime date;
                //if (!DateTime.TryParse(xmlCommonUtil.QueryString[DATE_key], out date))
                if (!DateTime.TryParse(dateValue, out date))
                {
                    //xmlCommonUtil.ResponseWriteErrorMSG(
                    //    "[" + DATE_key + "=" +
                    //    xmlCommonUtil.QueryString[DATE_key].ToString() +
                    //    "]예약 날짜 형식이 올바르지 않습니다.(예:2010-02-17 13:45)");
                    xmlCommonUtil.ResponseWriteErrorMSG(
                        "[" + DATE_key + "=" +
                        date +
                        "]예약 날짜 형식이 올바르지 않습니다.(예:2010-02-17 13:45)");
                    return null;
                }

                //2010-02-10 13:10
                //return xmlCommonUtil.QueryString[DATE_key] == null ?
                //    null :
                //    xmlCommonUtil.QueryString[DATE_key].ToString();

                return String.IsNullOrWhiteSpace(dateValue) ? null : dateValue;
            }
        }
        private string SEND_MSG_STRING
        {
            get
            {
                //return xmlCommonUtil.QueryString[MSG_key] == null ?
                //    "" :
                //    xmlCommonUtil.QueryString[MSG_key].ToString();

                return xmlCommonUtil.RequestData.GetValue(MSG_key);
            }
        }
        private List<MMSFiles> FILE_LIST
        {
            get
            {
                if (FILE_COUNT <= 0)
                {
                    return null;
                }

                if (5 < FILE_COUNT)
                {
                    xmlCommonUtil.ResponseWriteErrorMSG(
                        "MMS 첨부파일 데이터 갯수는 5개를 초과할 수 없습니다.(mms_file_cnt=" + FILE_COUNT.ToString() + ")");
                    return null;
                }
                //file_type1, file_name1, file_url1,
                List<MMSFiles> mmsList = new List<MMSFiles>();

                for (int i = 0; i < FILE_COUNT; i++)
                {
                    int c = i + 1;
                    //string fileType = xmlCommonUtil.QueryString[FILE_TYPE + c.ToString()] == null ?
                    //    string.Empty : xmlCommonUtil.QueryString[FILE_TYPE + c.ToString()];
                    //string fileName = xmlCommonUtil.QueryString[FILE_NAME + c.ToString()] == null ?
                    //    string.Empty : xmlCommonUtil.QueryString[FILE_NAME + c.ToString()];
                    //string fileUrl = xmlCommonUtil.QueryString[FILE_URL + c.ToString()] == null ?
                    //    string.Empty : xmlCommonUtil.QueryString[FILE_URL + c.ToString()];

                    string fileType = xmlCommonUtil.RequestData.GetValue(FILE_TYPE + c.ToString());
                    string fileName = xmlCommonUtil.RequestData.GetValue(FILE_NAME + c.ToString());
                    string fileUrl = xmlCommonUtil.RequestData.GetValue(FILE_URL + c.ToString());


                    if (string.IsNullOrEmpty(fileType) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileUrl))
                    {
                        xmlCommonUtil.ResponseWriteErrorMSG(
                            "MMS 첨부파일 데이터 중 일부가 올바르지 않습니다." +
                            "(" + FILE_TYPE + c.ToString() + "=" + fileType +
                            ", " + FILE_NAME + c.ToString() + "=" + fileName +
                            ", " + FILE_URL + c.ToString() + "=" + fileUrl +
                            ")");
                        return null;
                    }

                    MMSFiles m = new MMSFiles(fileType, fileUrl, appOptions.Mms, fileName);
                    mmsList.Add(m);
                }

                return mmsList;
            }
        }
        //mms_file_cnt
        private int FILE_COUNT
        {
            get
            {
                int fileCount = 0;
                var mmsFilecount = xmlCommonUtil.RequestData.GetValue(MMS_FILE_CNT_key);
                //if (!int.TryParse(xmlCommonUtil.QueryString[MMS_FILE_CNT_key], out fileCount))
                if(!int.TryParse(mmsFilecount, out fileCount))
                {
                    //xmlCommonUtil.ResponseWriteErrorMSG("[" + xmlCommonUtil.QueryString[MMS_FILE_CNT_key].ToString() + "]MMS 첨부파일 데이터 중 일부가 올바르지 않습니다.(mms_file_cnt)");
                    xmlCommonUtil.ResponseWriteErrorMSG($"[{ mmsFilecount }]MMS 첨부파일 데이터 중 일부가 올바르지 않습니다.(mms_file_cnt)");

                    return 0;
                }
                //return xmlCommonUtil.QueryString[MMS_FILE_CNT_key] == null ?
                //    0 :
                //    int.Parse(xmlCommonUtil.QueryString[MMS_FILE_CNT_key].ToString());

                return fileCount;
            }
        }
        //mms_subject
        private string MMS_SUBJECT
        {
            get
            {
                //return xmlCommonUtil.QueryString[MMS_SUBJECT_key] == null ?
                //    string.Empty :
                //    xmlCommonUtil.QueryString[MMS_SUBJECT_key].ToString();

                return xmlCommonUtil.RequestData.GetValue(MMS_SUBJECT_key);
            }
        }


        // enums 파일 이동
        //private enum MobileMessageType
        //{
        //    SMS = 4,
        //    URL = 5,
        //    MMS = 6,
        //}
    }

    public class MMSFiles
    {
        public MMSFiles(string fileType, string fileDownloadBasicUrl, string file_basic_local_path, string fileName)
        {
            this.file_type = fileType;
            this.file_download_basic_url = fileDownloadBasicUrl;
            this.file_name = fileName;
            this.file_basic_local_path = @"c:\temp\";
            try
            {

                //this.file_basic_local_path = ConfigurationManager.AppSettings["mms"].ToString();
                //this.file_basic_local_path = appOptions.Mms;
                this.file_basic_local_path = file_basic_local_path;
            }
            catch (Exception ex)
            {
                //XMLCommonUtil x = new XMLCommonUtil();
                //x.ResponseWriteErrorMSG("mms 경로 설정을 확인바랍니다.(appSettings)", ex);
                //xmlCommonUtil.ResponseWriteErrorMSG("mms 경로 설정을 확인바랍니다.(appSettings)", ex);

                throw new Exception("mms 경로 설정을 확인바랍니다.(appSettings)");
            }
        }

        private string file_type;
        private string file_basic_local_path;
        private string file_download_basic_url;
        private string file_name;

        public FILETYPE FileType
        {
            get
            {
                return (FILETYPE)Enum.Parse(typeof(FILETYPE), file_type);
            }
        }
        public string FileLocalPath
        {
            get
            {
                return file_basic_local_path + file_name;
            }
        }
        public string FileDownloadUrl
        {
            get
            {
                return file_download_basic_url + file_name;
            }
        }
        public string FileName
        {
            get
            {
                return file_name;
            }
        }
    }
}
