﻿#define SESSION_CHECK //세션을 체크할 것인가?
#define USING_TRANS //트랜잭션을 사용할 것인가? -> 사용시 TRY_CATCH같이 사용할 것.
#define TRY_CATCH //오류를 캐치 처리할 것인가?
//#define TRY_CATCH_MENU //메뉴처리에서 오류를 캐치 처리할 것인가?(임시)
#define USING_GET //get타입으로 데이터를 처리하는 것을 허용할 것인가? ->기본값 주석처리
//#define RETURN_xmlns_SESSIONID //데이터리턴시 세션아이디를 같이 보낼 것인가? -> 기본값 주석처리
#define DEBUG_CATCH //자세한 오류정보를 제공할 것인가? -> 기본값 주석처리

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;

using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.Xml;
using System.Text.RegularExpressions;

public class XMLCommonUtil
{
    private HttpContext _httpContext;

    public XMLCommonUtil()
    {
        this._httpContext = HttpContext.Current;
    }

#if SESSION_CHECK
    public const bool SESSION_CHECK = true;
#else
    public const bool SESSION_CHECK = false;
#endif

    public const string XMLHeader =
@"<?xml version=""1.0"" encoding=""utf-8""?>
";
    public const bool INCLUDE_ORGANIZATION_KEY = false;
    public const string ORGANIZATION_KEY_GUBUN = "organization_key";
    public const string OPERATOR_KEY_GUBUN = "operator_key";
    public const string OPERATOR_IP_GUBUN = "operator_ip";

    public const string GUBUN_KEY_STRING = "gubun";
    public const string PROC_KEY_STRING = "proc";

    public static string DB_SCHEMA
    {
        get
        {
            return ConfigurationManager.AppSettings["dbschema"].ToString();
        }
    }


    public const string USER_LOGIN_GUBUN = "userlogin";
    public const string SESSIONID_CHECK_GUBUN = "sessionID_check";
    public const string GET_SESSIONID_GUBUN = "get_sessionID";

    public const string NAMESPACE_STR = "http://service.kesso.kr/";


    private const string ALLOW_PROC_KEY = "allow_proc_list";

    private string[] allowProc
    {
        get
        {
            string ProcStrings = ConfigurationManager.AppSettings[ALLOW_PROC_KEY];

            if (string.IsNullOrEmpty(ProcStrings))
            {
                string tempString = XMLCommonUtil.XMLHeader +
                    this.returnErrorMSGXML("허가된 프로시져 목록이 없습니다. config파일을 설정하세요.");
                _httpContext.Response.Clear();
                _httpContext.Response.Write(tempString);
                _httpContext.Response.End();

            }
            ProcStrings = Regex.Replace(ProcStrings, @"[\s]", "");
            string[] tempAllowProc = ProcStrings.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            return tempAllowProc;
        }
    }



    private const string MSG_XML =
@"<values xmlns=""{0}"">
    <item>
        <return_code>{1}</return_code> 
        <return_message>{2}</return_message> 
  </item>
</values>";

    public const string SESSIONID_KEY_STRING = "sessionID";
    private const string DB_CONFIG_STRING = "dbconnectionconfig";

    public const string DATASET_NAME = "values";
    public const string TABLE_NAME = "item";

    private const string WEB_GUBUN_KEY = "web_gubun";

    public string sessionID
    {
        get
        {
            //return this.Session.SessionID;
            return _httpContext.Session["guid"].ToString();
        }
    }
    public string PROCEDURE_TITLE
    {
        get
        {
            string proc_gubun = QueryString[PROC_KEY_STRING];
            string web_gubun = QueryString[WEB_GUBUN_KEY];

            bool isWebQuery = false;
            bool isAllowProc = false;
            
            if (!string.ReferenceEquals(web_gubun, null))
            {
                isWebQuery = true;

                //*
                foreach (string str in this.allowProc)
                {
                    if (string.Equals(str, proc_gubun))
                    {
                        isAllowProc = true;
                        break;
                    }
                }//*/
            }

            if ((isWebQuery && !isAllowProc) || string.ReferenceEquals(proc_gubun, null))
            {
                string errorMSG = XMLHeader + this.returnErrorMSGXML("실행이 허가되지 않았거나, 유효하지 않은 프로시져명입니다.");

                this.ResponseWrite(errorMSG);
                return null;
            }
            else
            {
                return DB_SCHEMA + "." + proc_gubun;
            }
        }
    }

    public string GUBUN
    {
        get
        {
            return this.QueryString[GUBUN_KEY_STRING] == null ?
                string.Empty :
                this.QueryString[GUBUN_KEY_STRING].ToString();
        }
    }
    public string WEB_GUBUN
    {
        get
        {
            return this.QueryString[WEB_GUBUN_KEY] == null ?
                string.Empty :
                this.QueryString[WEB_GUBUN_KEY].ToString();
        }
    }
    public NameValueCollection QueryString
    {
        get
        {
            string httpMethod = _httpContext.Request.HttpMethod.ToUpper();
            switch (httpMethod)
            {
                case "POST":
                    return _httpContext.Request.Form;
#if USING_GET
                case "GET":
                    return _httpContext.Request.QueryString;
                default:
                    return _httpContext.Request.Params;
#else
                default:
                    return _httpContext.Request.Form;
#endif
            }
        }
    }
    public SqlConnection SQLCONNECTION
    {
        get
        {
            string connectionstring = ConfigurationManager.AppSettings[DB_CONFIG_STRING].ToString();
            SqlConnection connection = new SqlConnection(connectionstring);
            connection.Open();

            return connection;
        }
    }
    public SqlParameter[] SQLPARAMETERS
    {
        get
        {
            if (QueryString.Count == 0)
            {
                return null;
            }

            List<SqlParameter> temp_param = new List<SqlParameter>();

            bool include_organization_key = XMLCommonUtil.INCLUDE_ORGANIZATION_KEY;

            for (int i = 0; i < QueryString.Count; i++)
            {
                string qKey = QueryString.GetKey(i).ToString();

                if (PROC_KEY_STRING.Equals(qKey))
                {
                    continue;
                }
                else if (SESSIONID_KEY_STRING.Equals(qKey))
                {
                    continue;
                }
                else if (!include_organization_key && ORGANIZATION_KEY_GUBUN.Equals(qKey))
                {
                    continue;
                }

                qKey = "@" + qKey;
                string qValue = QueryString.GetValues(i).GetValue(0).ToString();
                temp_param.Add(new SqlParameter(qKey, qValue));
            }

            temp_param.Add(new SqlParameter("@operator_ip", _httpContext.Request.ServerVariables["REMOTE_ADDR"]));
            
            SqlParameter[] sp = null;

            if (temp_param.Count > 0)
            {
                sp = temp_param.ToArray();
            }

            return sp;
        }
    }

    private string ROOT_NODE
    {
        get
        {
            return string.Format("<values xmlns=\"{0}\">\r\n</values>",
#if RETURN_xmlns_SESSIONID
 this.sessionID
#else
 NAMESPACE_STR
#endif
);
        }
    }

    public string returnErrorMSGXML(string msg)
    {
        return this.returnMSGXML("100", msg);
    }
    public string returnErrorMSGXML(string msg, Exception ex)
    {
        return this.returnErrorMSGXML(msg
#if DEBUG_CATCH

 + " / " + ex.Source + " / " + ex.Message

#endif //DEBUG_CATCH
);
    }
    public string returnMSGXML(string code, string msg)
    {
        return string.Format(MSG_XML,
#if RETURN_xmlns_SESSIONID
 this.sessionID,
#else
 NAMESPACE_STR,
#endif
 code, msg);
    }
    public void returnSessionID()
    {
        this.ResponseWriteMSG("1", this.sessionID);
        
        /*
        string xmldata = XMLHeader;
        xmldata += this.returnMSGXML("1", this.sessionID);

        _httpContext.Response.Clear();
        _httpContext.Response.Write(xmldata);
        _httpContext.Response.End();
        //*/
    }
    public bool CheckSessionID(bool autoMessage)
    {
        bool issameSessionID = true;

#if SESSION_CHECK
        bool isWebQuery = !string.IsNullOrEmpty(QueryString[WEB_GUBUN_KEY]);

        string client_SessionID = QueryString[SESSIONID_KEY_STRING] == null ? string.Empty : QueryString[SESSIONID_KEY_STRING].ToString();
        string proc = QueryString[PROC_KEY_STRING] == null ? string.Empty : QueryString[PROC_KEY_STRING].ToString();
        string gubun = QueryString[GUBUN_KEY_STRING] == null ? string.Empty : QueryString[GUBUN_KEY_STRING].ToString();
        bool is예외상황 = isWebQuery || gubun.Equals(USER_LOGIN_GUBUN) || gubun.Equals(GET_SESSIONID_GUBUN);//예외는 로그인시 및 test를 위한 sessionid check시에만 사용함.   || (gubun.Equals(GET_USER_INFO_GUBUN) && gubun.Equals(USER_R_DETAIL_GUBUN));

        if (!is예외상황 && !client_SessionID.Equals(this.sessionID))
        {
            if (autoMessage)
            {
                string xmldata = XMLHeader;
                if (client_SessionID.Equals(string.Empty))
                {
                    xmldata += this.returnErrorMSGXML(SESSIONID_KEY_STRING + "값을 넘기지 않았습니다.");
                }
                else
                {
                    xmldata += this.returnMSGXML("999", "서버와의 세션유지시간이 초과하였습니다.");
                }

                this.ResponseWrite(xmldata);
            }

            issameSessionID = false;//sessionID가 다르면 데이터를 처리하지 않고 에러메시지를 리턴한다.
        }
#endif

        return issameSessionID;
    }

    public bool CheckSessionID()
    {
        return this.CheckSessionID(true);
    }

    public string ClientSessionID
    {
        get
        {
            string client_SessionID = QueryString[SESSIONID_KEY_STRING] == null ? 
                string.Empty : 
                QueryString[SESSIONID_KEY_STRING].ToString();

            return client_SessionID;
        }
    }

    public void WriteXML(bool include_sessionID)
    {
        this.WriteXML(this.PROCEDURE_TITLE, this.SQLPARAMETERS, include_sessionID);
    }

    public void WriteXML(string proc, SqlParameter[] sqlparams, bool include_sessionID)
    {
        DataSet result_ds = this.ReturnDataSet_Common(proc, sqlparams, include_sessionID);

        result_ds.DataSetName = DATASET_NAME;

        string xml = XMLCommonUtil.XMLHeader;

        if (result_ds.Tables.Count > 0)
        {
            result_ds.Tables[0].TableName = TABLE_NAME;
        }

        xml += result_ds.GetXml();

        this.ResponseWrite(xml);

        /*
        _httpContext.Response.Clear();
        _httpContext.Response.Write(xml);
        _httpContext.Response.End();
        //*/
    }

    public void ReturnMenuXML()
    {
        string xmldata = XMLHeader;
        DataSet ds = this.ReturnDataSet_Common(PROCEDURE_TITLE, SQLPARAMETERS, false);

        if (ds.Tables.Count == 0)
        {
            //에러 처리
            xmldata += this.returnErrorMSGXML("메뉴가 구성되지 않았습니다.");

            _httpContext.Response.Clear();
            _httpContext.Response.Write(xmldata);
            _httpContext.Response.End();

            return;
        }

        DataTable dt = ds.Tables[0];

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(this.ROOT_NODE);


#if TRY_CATCH_MENU
        try
        {
#endif //TRY_CATCH_MENU

            DataRow[] drs = dt.Select("up_menu_key = 'root'", "display_order");

            for (int i = 0; i < drs.Length; i++)
            {
                XmlNode addMenu = doc.CreateNode(XmlNodeType.Element, "menuitem", "");
                XmlElement root = doc.DocumentElement;

                XmlAttribute attr = doc.CreateAttribute("label");
                attr.Value = drs[i]["label"].ToString();
                addMenu.Attributes.Append(attr);

                attr = doc.CreateAttribute("data");
                attr.Value = drs[i]["data"].ToString();
                addMenu.Attributes.Append(attr);

                root.AppendChild(addMenu);

                DataRow[] drs_sub = dt.Select(string.Format("up_menu_key = '{0}'", drs[i]["menu_key"].ToString()), "display_order");

                if (drs_sub.Length > 0)
                {
                    SetSubMenu(doc, drs_sub, addMenu);
                }
            }

            xmldata += doc.OuterXml;

            this.ResponseWrite(xmldata);
            /*    
            _httpContext.Response.Clear();
            _httpContext.Response.Write(xmldata);
            _httpContext.Response.End();
            //*/

#if TRY_CATCH_MENU
        }
        catch (Exception ex)
        {
            //에러 처리
            xmldata = XMLHeader;

            xmldata += this.returnErrorMSGXML("ReturnMenuXML() Error", ex);

            _httpContext.Response.Clear();
            _httpContext.Response.Write(xmldata);
            _httpContext.Response.End();

            return;
        }
#endif //TRY_CATCH_MENU
    }


    public void ResponseWrite(string returnXML)
    {
        _httpContext.Response.Clear();
        _httpContext.Response.Write(returnXML);
        _httpContext.Response.End();
    }

    public void ResponseWriteErrorMSG(string errorMSG)
    {
        string xmldata = XMLHeader;
        xmldata += this.returnErrorMSGXML(errorMSG);

        this.ResponseWrite(xmldata);
    }

    public void ResponseWriteErrorMSG(string errorMSG, Exception ex)
    {
        string xmldata = XMLHeader;
        xmldata += this.returnErrorMSGXML(errorMSG, ex);

        this.ResponseWrite(xmldata);
    }
    
    public void ResponseWriteMSG(string code, string msg)
    {
        string xmldata = XMLHeader;
        xmldata += this.returnMSGXML(code, msg);

        this.ResponseWrite(xmldata);
    }

    //미완성 !!! - 기본적인 로그를 기록한다. 신규 로그가 추가될 경우 로그형태에 대한 기록을 남긴다.
    public bool RecordLog(string pole_num, string type1, string type2, string logData)
    {
        bool isSuccess = false;

        //RecordLog code 작성
        
        return isSuccess;
    }


    private static void SetSubMenu(XmlDocument doc, DataRow[] drs, XmlNode upNode)
    {
        for (int i = 0; i < drs.Length; i++)
        {
            XmlNode addMenu = doc.CreateNode(XmlNodeType.Element, "menuitem", "");

            XmlAttribute attr = doc.CreateAttribute("label");
            attr.Value = drs[i]["label"].ToString();
            addMenu.Attributes.Append(attr);

            attr = doc.CreateAttribute("data");
            attr.Value = drs[i]["data"].ToString();
            addMenu.Attributes.Append(attr);

            upNode.AppendChild(addMenu);
        }
    }

    public DataSet ReturnDataSet_Common(string proc_name, SqlParameter[] sqlparams, bool include_sessionID, out string error_msg)
    {
        string out_msg = string.Empty;
        DataSet return_dataset = null;

#if TRY_CATCH
        string xml_error_data = XMLHeader; // 에러 메시지 처리
#endif //TRY_CATCH

        SqlConnection connection = null;




#if TRY_CATCH
        try
        {
#endif //TRY_CATCH
            connection = this.SQLCONNECTION;
#if TRY_CATCH
        }
        catch (Exception ex)
        {
            xml_error_data += this.returnErrorMSGXML("ReturnDataSet_Common.Error check(connection)", ex);

            out_msg = xml_error_data;
        }
#endif //TRY_CATCH







#if USING_TRANS
        using (SqlTransaction trans = connection.BeginTransaction())
        {
#endif //USING_TRANS

#if TRY_CATCH
        try
        {
#endif //TRY_CATCH

            DataSet ds = new DataSet();

#if USING_TRANS
                if (sqlparams != null && sqlparams.Length > 0)
                {
                    ds = SqlHelper.ExecuteDataset(trans, CommandType.StoredProcedure, proc_name, sqlparams);
                }
                else
                {
                    ds = SqlHelper.ExecuteDataset(trans, CommandType.StoredProcedure, proc_name);
                }

                trans.Commit();
#else
            if (sqlparams != null && sqlparams.Length > 0)
            {
                ds = SqlHelper.ExecuteDataset(
                    connection,
                    CommandType.StoredProcedure,
                    proc_name,
                    sqlparams);
            }
            else
            {
                ds = SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, proc_name);
            }
#endif //USING_TRANS

#if RETURN_xmlns_SESSIONID
#else
            //*
                DataTable dt ;
                DataRow[] drs = null;

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns.Contains("return_code"))
                {
                    dt = ds.Tables[0];
                    drs = dt.Select("return_code = 1");
                }
            
                string web_gubun = QueryString[WEB_GUBUN_KEY];
    
            //*/
                if (include_sessionID && drs != null && drs.Length > 0)
                {
#endif
                    string ns = this.sessionID;
                    ds.Namespace = ns;
#if RETURN_xmlns_SESSIONID
#else

                }
                //*
                else if (!string.ReferenceEquals(web_gubun, null))
                {
                }
                else
                {
                    //string ns = NAMESPACE_STR;
                    //ds.Namespace = ns;
                }
                //*/
#endif
            return_dataset = ds;
#if TRY_CATCH
        }
        catch (Exception ex)
        {
#if USING_TRANS

                trans.Rollback();

#endif //USING_TRANS

            if (connection != null)
                connection.Dispose();

            xml_error_data += this.returnErrorMSGXML("ReturnDataSet_Common.Error check(ExecuteDataset)", ex);

            out_msg = xml_error_data;
        }
        finally
        {
#endif //TRY_CATCH
            if (connection != null)
                connection.Dispose();
#if TRY_CATCH

        }
#endif
        error_msg = out_msg;

        return return_dataset;
#if USING_TRANS

        }
#endif
    }

    public DataSet ReturnDataSet_Common(string proc_name, SqlParameter[] sqlparams, bool include_sessionID)
    {
        string str = string.Empty;
        DataSet ds = this.ReturnDataSet_Common(proc_name, sqlparams, include_sessionID, out str);
        if (!string.IsNullOrEmpty(str))
        {
            this.ResponseWrite(str);
            
            /*
            _httpContext.Response.Clear();
            _httpContext.Response.Write(str);
            _httpContext.Response.End();
            //*/
            
            return null;
        }

        return ds;
    }
}