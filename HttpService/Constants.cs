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
    }
}
