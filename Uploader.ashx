<%@ WebHandler Language="C#" Class="Uploader" %>

#define TRY_CATCH //������ ĳġ ó���� ���ΰ�?

using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Configuration;

public class Uploader : IHttpHandler, IReadOnlySessionState
{
    private XMLCommonUtil xmlCommonUtil;
    private FileCommonUtil fileCommon;
    private string fileFullPath;
    private string attachment_filename;
    private string attachment_fileformat;
    private long attachment_filesize;

    public bool IsReusable
    {
        get { return true; }
    }
    
    public void ProcessRequest(HttpContext _context)
    {
        string xmldata = XMLCommonUtil.XMLHeader;
#if TRY_CATCH
        try
        {
#endif //TRY_CATCH
            xmlCommonUtil = new XMLCommonUtil();
            fileCommon = new FileCommonUtil();

            
            /*������ ���� ���� üũ*/
            string sessionID = xmlCommonUtil.sessionID;
            string client_SessionID = xmlCommonUtil.ClientSessionID;

            if (XMLCommonUtil.SESSION_CHECK && client_SessionID == "")
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("���������� ���̰ų�, ���� ������ �ѱ��� �ʽ��ϴ�. �α��� �� ���ٶ��ϴ�.");
                _context.Response.Write(xmldata);
                return;
            }
            else if (XMLCommonUtil.SESSION_CHECK && sessionID != client_SessionID)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("���� ������ �ùٸ��� �ʽ��ϴ�. �ٽ� �α����� �� ���ٶ��ϴ�.");
                _context.Response.Write(xmldata);
                return;
            }

            
            /*���� ���ε� ���� ����*/
            if (_context.Request.Files.Count == 0)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("���ε� ������ �����ϴ�.");
                _context.Response.Write(xmldata);
                return;
            }
            else if (_context.Request.Files.Count > 1)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("������ �ϳ����� ���ε� �����մϴ�.");
                _context.Response.Write(xmldata);
                return;
            }

            
            
            
            
            
            /*
             * ������ġ�� �����ϰ�,
             */
            string uploadDir = ConfigurationManager.AppSettings["etc"];
#if TRY_CATCH
            try
            {
#endif //TRY_CATCH
                uploadDir = fileCommon.ATTACHMENT_UPLOAD_PATH;

                //���丮 üũ
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
#if TRY_CATCH
            }
            catch
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("���ϰ�θ� ���� �� �����ϴ�.");
                _context.Response.Write(xmldata);
                return;
            }
#endif //TRY_CATCH

            
            
            
            
            
            
            /*
             * ������ �����ϰ�,(+ �̹��� ������ ��� �������̹����� �����ϰ�-�����ȵ�)
             */
#if TRY_CATCH
            try
            {
#endif //TRY_CATCH
                foreach (string fileKey in _context.Request.Files)
                {
                    HttpPostedFile file = _context.Request.Files[fileKey];

                    //�����̸��� �����Ͽ� ������ ���ΰ�? file_name
                    string namedFileName = xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_FILENAME_key];
                    if (!string.IsNullOrEmpty(namedFileName))
                    {
                        fileFullPath = uploadDir + namedFileName;
                    }
                    else
                    {
                        fileFullPath = uploadDir + file.FileName;   
                    }
                    
                    fileFullPath = fileCommon.MakeUniqueFileName(fileFullPath);
                    file.SaveAs(fileFullPath);

                    FileInfo fileInfo = new FileInfo(fileFullPath);
                    this.attachment_filename = fileInfo.Name;
                    this.attachment_fileformat = fileInfo.Extension;
                    this.attachment_filesize = fileInfo.Length;
                }
#if TRY_CATCH
            }
            catch (System.Exception ex)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("������ ������ �� �����ϴ�.", ex);
                _context.Response.Write(xmldata);
                return;
            }
#endif //TRY_CATCH

            
            
            
            
            
            
            
            
            
            
            /*
             * ��� �����ϰ�,
             */
            string resultXML = null;
            
            //��� ������ ���ΰ�? db_work
            string isDBWork = xmlCommonUtil.QueryString[FileCommonUtil.DB_WORK_GUBUN_value];
            string return_msg = string.Empty;
            
            if (!string.IsNullOrEmpty(isDBWork) && !isDBWork.Equals("pass"))
            {
                //db_work=pass ��������� ǥ���� ��� ��� �۾� ����.
            }
            else
            {
                return_msg = fileCommon.Attachment_C(attachment_filename,
                                                            attachment_fileformat,
                                                            attachment_filesize,
                                                            out resultXML);
            }
            
            
            
            
            
            
            
            
            
            
            
            /*
             * �޽����� �����Ѵ�.
             */
            if (!string.IsNullOrEmpty(resultXML) && string.IsNullOrEmpty(return_msg))
            {   
                xmldata += resultXML;
            }
            else
            {
                if (File.Exists(fileFullPath))
                {
                    File.Delete(fileFullPath);
                }
                xmldata += return_msg;
            }
            _context.Response.Write(xmldata);
            return;
#if TRY_CATCH
        }
        catch (System.Exception ex)
        {
            //throw ex;//test!!
            xmldata += xmlCommonUtil.returnErrorMSGXML("���Ͼ��ε� ������ �߻��߽��ϴ�.", ex);
            _context.Response.Write(xmldata);
            return;
        }
#endif //TRY_CATCH
    }
}