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

public partial class _Default : Page
{
    private XMLCommonUtil xmlCommonUtil = new XMLCommonUtil();

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

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string gubun = xmlCommonUtil.GUBUN;

            if (!xmlCommonUtil.CheckSessionID() && !PassCheckSessionID)
            {
                return;
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
                    xmlCommonUtil.WriteXML(true);
                    break;
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
                    SendMobileMSGCommon smmc = new SendMobileMSGCommon();
                    smmc.SendMobileMSG();
                    break;

                case "csv":
                    ExcelDownload ed = new ExcelDownload();
                    ed.DownLoadCSVFile();
                    break;

                case "send_email":
                    SendEmail se = new SendEmail();
                    se.send();
                    break;

                case "upload_twitpic":
                    UploadAndPostTwitPic ut = new UploadAndPostTwitPic();
                    ut.UploadAndPost();
                    break;

                /*기본 xml return *********************************/
                default:
                    if (!string.IsNullOrEmpty(xmlCommonUtil.QueryString[XMLCommonUtil.PROC_KEY_STRING]))
                    {
                        xmlCommonUtil.WriteXML(false);
                    }
                    else
                    {
                        xmlCommonUtil.ResponseWriteErrorMSG("필수 매개변수를 넘기지 않았습니다.");
                        return;
                    }
                    break;
            }
        }
    }

}
