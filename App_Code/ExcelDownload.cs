using System;
using System.Collections.Generic;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Text;

public class ExcelDownload
{
    private HttpContext _httpContext;
    private XMLCommonUtil xmlCommonUtil;

    private const string GUBUN_CSV = "csv";

    public ExcelDownload()
    {
        this._httpContext = HttpContext.Current;
        this.xmlCommonUtil = new XMLCommonUtil();
    }

    public void DownLoadCSVFile()
    {
        string out_msg = string.Empty;
        string proc_name = xmlCommonUtil.PROCEDURE_TITLE;
        SqlParameter[] sqlparams = xmlCommonUtil.SQLPARAMETERS;
        DataSet ds = null;

        try
        {
            ds = xmlCommonUtil.ReturnDataSet_Common(proc_name, sqlparams, false, out out_msg);
        }
        catch (Exception ex)
        {
            out_msg = xmlCommonUtil.returnErrorMSGXML("DownLoadCSVFile", ex);
        }

        if (!string.IsNullOrEmpty(out_msg))
        {
            xmlCommonUtil.ResponseWrite(out_msg);
            return;
        }

        if (ds == null || ds.Tables.Count == 0)
        {
            xmlCommonUtil.ResponseWriteErrorMSG("DownLoadCSVFile 리턴 데이터에 오류가 있습니다.");
            return;
        }

        this.MakeCSVFile(ds);
    }

    private void MakeCSVFile(DataSet ds)
    {
        string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

        HttpResponse response = this._httpContext.Response;

        response.Clear();
        response.ClearHeaders();
        response.ClearContent();

        response.ContentType = "text/csv";
        response.ContentEncoding = Encoding.UTF8;
        response.HeaderEncoding = Encoding.UTF8;
        
        response.AppendHeader("Content-Disposition", "attachment; fileName=\"" + _httpContext.Server.UrlPathEncode(fileName));

        byte[] bom = { 0xEF, 0xBB, 0xBF};//EF BB BF; utf-8 with BOM
        response.BinaryWrite(bom);
        
        DataTable dt = ds.Tables[0];

        DataColumnCollection dcc = dt.Columns;
        DataRowCollection drc = dt.Rows;

        StringBuilder sb = new StringBuilder();

        //column 타이틀 설정
        for (int columnIndex = 0; columnIndex < dcc.Count; columnIndex++)
        {
            AddComma(dcc[columnIndex].ColumnName, sb);
        }
        sb.Remove(sb.Length - 2, 2);
        sb.AppendLine();

        //row별 데이터 입력
        for (int rowIndex = 0; rowIndex < drc.Count; rowIndex++)
        {
            DataRow dr = drc[rowIndex];
            for (int columnIndex = 0; columnIndex < dr.ItemArray.Length; columnIndex++)
            {
                AddComma(dr[columnIndex].ToString(), sb);
            }
            sb.Remove(sb.Length - 2, 2);

            if (rowIndex.Equals(drc.Count - 1))
            {
                break;
            }

            sb.AppendLine();
        }

        response.Write(sb.ToString());
        response.End();
    }

    private static void AddComma(string value, StringBuilder stringBuilder)
    {
        stringBuilder.Append(value.Replace(',', ' '));
        stringBuilder.Append(", ");
    }
}
