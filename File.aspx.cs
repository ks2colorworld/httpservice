using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _file : System.Web.UI.Page
{
    private XMLCommonUtil xmlCommonUtil;
    private FileCommonUtil fileCommonUtil;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            return;
        }

        xmlCommonUtil = new XMLCommonUtil();
        fileCommonUtil = new FileCommonUtil();

        string gubun = xmlCommonUtil.GUBUN;
        string webGubun = xmlCommonUtil.WEB_GUBUN;

        bool passCheckSessionID = (gubun == "web" && webGubun != string.Empty);
        
        if (passCheckSessionID)
        {
            gubun = webGubun;
        }

        if (!(passCheckSessionID || xmlCommonUtil.CheckSessionID()))
        {
            //세션 채크함.
            return;
        }

        switch (gubun)
        {
            case FileCommonUtil.FILE_DOWNLOAD_GUBUN_value:
                fileCommonUtil.DownloadFile();
                break;
            case FileCommonUtil.FILE_DELETE_GUBUN_value:
                fileCommonUtil.DeleteFile();
                break;
            
            case FileCommonUtil.FILE_INFO_GUBUN_value:
                fileCommonUtil.GetFileInfo();
                break;
            
            case FileCommonUtil.FILE_LIST_GUBUN_value:
                fileCommonUtil.GetFileNameList();
                break;
            case FileCommonUtil.FILE_RENAME_GUBUN_value:
                fileCommonUtil.FileRename();
                break;
        }
    
    }
}
