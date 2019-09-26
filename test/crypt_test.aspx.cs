using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class crypt_test : System.Web.UI.Page
{
    private XMLCommonUtil xmlCommonUtil = new XMLCommonUtil();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string message = xmlCommonUtil.QueryString["msg"];
            AESCipher aes = new AESCipher("427d0a5839078c2ba379ecc9772aefa8");
            string dec =
                aes.Decrypt(message);
            xmlCommonUtil.ResponseWriteErrorMSG(dec);
        }
    }
}