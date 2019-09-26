<%@ WebHandler Language="C#" Class="Uploader" %>

#define TRY_CATCH //오류를 캐치 처리할 것인가?

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

            
            /*웹서버 세션 정보 체크*/
            string sessionID = xmlCommonUtil.sessionID;
            string client_SessionID = xmlCommonUtil.ClientSessionID;

            if (XMLCommonUtil.SESSION_CHECK && client_SessionID == "")
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("세션정보가 빈값이거나, 세션 정보를 넘기지 않습니다. 로그인 후 사용바랍니다.");
                _context.Response.Write(xmldata);
                return;
            }
            else if (XMLCommonUtil.SESSION_CHECK && sessionID != client_SessionID)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("세션 정보가 올바르지 않습니다. 다시 로그인한 후 사용바랍니다.");
                _context.Response.Write(xmldata);
                return;
            }

            
            /*파일 업로드 갯수 제한*/
            if (_context.Request.Files.Count == 0)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("업로드 파일이 없습니다.");
                _context.Response.Write(xmldata);
                return;
            }
            else if (_context.Request.Files.Count > 1)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("파일은 하나씩만 업로드 가능합니다.");
                _context.Response.Write(xmldata);
                return;
            }

            
            
            
            
            
            /*
             * 저장위치를 지정하고,
             */
            string uploadDir = ConfigurationManager.AppSettings["etc"];
#if TRY_CATCH
            try
            {
#endif //TRY_CATCH
                uploadDir = fileCommon.ATTACHMENT_UPLOAD_PATH;

                //디렉토리 체크
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
#if TRY_CATCH
            }
            catch
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("파일경로를 만들 수 없습니다.");
                _context.Response.Write(xmldata);
                return;
            }
#endif //TRY_CATCH

            
            
            
            
            
            
            /*
             * 파일을 저장하고,(+ 이미지 파일일 경우 섬네일이미지를 생성하고-구현안됨)
             */
#if TRY_CATCH
            try
            {
#endif //TRY_CATCH
                foreach (string fileKey in _context.Request.Files)
                {
                    HttpPostedFile file = _context.Request.Files[fileKey];

                    //파일이름을 지정하여 저장할 것인가? file_name
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
                xmldata += xmlCommonUtil.returnErrorMSGXML("파일을 저장할 수 없습니다.", ex);
                _context.Response.Write(xmldata);
                return;
            }
#endif //TRY_CATCH

            
            
            
            
            
            
            
            
            
            
            /*
             * 디비를 저장하고,
             */
            string resultXML = null;
            
            //디비에 저장할 것인가? db_work
            string isDBWork = xmlCommonUtil.QueryString[FileCommonUtil.DB_WORK_GUBUN_value];
            string return_msg = string.Empty;
            
            if (!string.IsNullOrEmpty(isDBWork) && !isDBWork.Equals("pass"))
            {
                //db_work=pass 명시적으로 표시할 경우 디비 작업 없음.
            }
            else
            {
                return_msg = fileCommon.Attachment_C(attachment_filename,
                                                            attachment_fileformat,
                                                            attachment_filesize,
                                                            out resultXML);
            }
            
            
            
            
            
            
            
            
            
            
            
            /*
             * 메시지를 리턴한다.
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
            xmldata += xmlCommonUtil.returnErrorMSGXML("파일업로드 에러가 발생했습니다.", ex);
            _context.Response.Write(xmldata);
            return;
        }
#endif //TRY_CATCH
    }
}