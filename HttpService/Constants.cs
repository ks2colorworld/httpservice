using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService
{
    public class Constants
    {
        public const bool SESSION_CHECK = true;
        public const bool INCLUDE_ORGANIZATION_KEY = false;
        public const string ORGANIZATION_KEY_GUBUN = "organization_key";
        public const string OPERATOR_KEY_GUBUN = "operator_key";
        public const string OPERATOR_IP_GUBUN = "operator_ip";

        public const string GUBUN_KEY_STRING = "gubun";
        public const string PROC_KEY_STRING = "proc";
        public const string USER_LOGIN_GUBUN = "userlogin";
        public const string SESSIONID_CHECK_GUBUN = "sessionID_check";
        public const string GET_SESSIONID_GUBUN = "get_sessionID";

        public const string NAMESPACE_STR = "http://service.kesso.kr/";
        public const string ALLOW_PROC_KEY = "allow_proc_list";
        public const string SESSIONID_KEY_STRING = "sessionID";
        private const string DB_CONFIG_STRING = "dbconnectionconfig";

        public const string DATASET_NAME = "values";
        public const string TABLE_NAME = "item";

        public const string WEB_GUBUN_KEY = "web_gubun";

        public const string FILE_DOWNLOAD_GUBUN_value = "file_download";
        public const string FILE_DELETE_GUBUN_value = "file_delete";

        public const string FILE_INFO_GUBUN_value = "file_info";

        public const string FILE_RENAME_GUBUN_value = "file_rename";
        public const string FILE_LIST_GUBUN_value = "file_list";

        public const string DB_WORK_GUBUN_value = "db_work";

        public const string ATTACHMENT_KEY_key = "attachment_key";
        public const string ATTACHMENT_GUBUN_key = "attachment_gubun";
        public const string ATTACHMENT_DETAIL_CODE_key = "attachment_detail_code";

        public const string ATTACHMENT_FILENAME_key = "file_name";

        public const string ATTACHMENT_FILESIZE_key = "file_size";
        public const string ATTACHMENT_FILEFORMAT_key = "file_format";

        public const string ATTACHMENT_FILETARGET_key = "target_file";

        //아직 구현안됨.
        //private const string ATTACHMENT_THUMBNAIL_PATH_string = "thumbnail_path";

        public const string ATTACHMENT_CUD_value = "attachment_CUD";
        public const string ATTACHMENT_R_value = "attachment_R";

        // 전자우편
        public const string TO_key = "to";
        public const string P_TYPE_key = "p_type";

        public const string SUBJECT_key = "subject";
        public const string MESSAGE_key = "message";

        public const string FORM_KEY_key = "form_key";//추후 구현할 키값
        public const string MESSAGE_PARAMS_key = "message_params";//추후 구현할 키값
        public const string SUBJECT_PARAMS_key = "subject_params";//추후 구현할 키값

        public const string P_TYPE_DIRECT_value = "direct";
        public const string P_TYPE_FORM_value = "form";//추후 구현할 value값

        // 모바일 메시지
        public const string RECEIVER_PHONE_key = "receiver_phone";
        public const string SENDER_PHONE_key = "sender_phone";
        public const string MSG_key = "msg";

        public const string DATE_key = "date";

        public const string MMS_SUBJECT_key = "mms_subject";
        public const string MSG_TYPE_key = "msg_type";

        public const string FILE_TYPE = "mms_file_type";//1,2,3,4,5
        public const string FILE_NAME = "mms_file_name";//1,2,3,4,5
        public const string FILE_URL = "mms_file_url";//1,2,3,4,5

        public const string MMS_FILE_CNT_key = "mms_file_cnt";

        //아래는 디비에서 사용하는 key
        public const string FILE_TYPE_KEY = "file_type";
        public const string FILE_NAME_KEY = "file_name";
        public const string FILE_CNT_KEY = "file_cnt";
        public const string MMS_BODY_KEY = "mms_body";

        //public readonly string PROC_SEND_MOBILE_MSG_CUD; //= XMLCommonUtil.DB_SCHEMA + "._SendMobileMSG";
        public const string PROC_SEND_MOBILE_MSG_CUD = "_SendMobileMSG";

        public const string TRAN_PHONE = "tran_phone";
        public const string TRAN_CALLBACK = "tran_callback";
        public const string TRAN_DATE = "tran_date";
        public const string TRAN_MSG = "tran_msg";
        public const string TRAN_TYPE = "tran_type";
        public const string TRAN_ETC4 = "tran_etc4";

        public const string GUBUN_SAVE_MMS_FILE = "save_mms_file_info";
        public const string GUBUN_SEND_SMS = "send_sms";//or "send_public_sms";

        // twitpic
        public const string TWIT_ID_key = "twit_id";
        //public const string MESSAGE_key = "message";
    }
}
