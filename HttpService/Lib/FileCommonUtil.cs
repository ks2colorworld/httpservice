#define USING_TRANS //트랜젝션을 사용할 것인가? -> 사용시 TRY_CATCH같이 사용할 것.
#define TRY_CATCH //오류를 캐치 처리할 것인가?

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Xml;
using System.IO;
using Microsoft.Extensions.Options;
using HttpService.Options;
using Microsoft.AspNetCore.Http;
using HttpService.Models;

namespace HttpService.Lib
{
    public class FileCommonUtil
    {
        private HttpContext _httpContext;
        private XMLCommonUtil xmlCommonUtil;

        public const string FILE_DOWNLOAD_GUBUN_value = "file_download";
        public const string FILE_DELETE_GUBUN_value = "file_delete";

        public const string FILE_INFO_GUBUN_value = "file_info";

        public const string FILE_RENAME_GUBUN_value = "file_rename";
        public const string FILE_LIST_GUBUN_value = "file_list";

        public const string DB_WORK_GUBUN_value = "db_work";

        public const string ATTACHMENT_KEY_key = "attachment_key";
        private const string ATTACHMENT_GUBUN_key = "attachment_gubun";
        private const string ATTACHMENT_DETAIL_CODE_key = "attachment_detail_code";

        public const string ATTACHMENT_FILENAME_key = "file_name";

        private const string ATTACHMENT_FILESIZE_key = "file_size";
        private const string ATTACHMENT_FILEFORMAT_key = "file_format";

        private const string ATTACHMENT_FILETARGET_key = "target_file";

        //아직 구현안됨.
        //private const string ATTACHMENT_THUMBNAIL_PATH_string = "thumbnail_path";

        private const string ATTACHMENT_CUD_value = "attachment_CUD";
        private const string ATTACHMENT_R_value = "attachment_R";

        public FileCommonUtil(
            IHttpContextAccessor httpContextAccessor,
            IOptions<AppOptions> appOptionsAccessor,
            XMLCommonUtil xmlCommonUtil)
        {
            this.appOptions = appOptionsAccessor.Value;
            this._httpContext = httpContextAccessor.HttpContext;
            this.xmlCommonUtil = xmlCommonUtil;

            //this._httpContext = HttpContext.Current;
            //this.xmlCommonUtil = new XMLCommonUtil();
        }

        #region //파일 존재여부 확인
        /// <summary>
        /// 파일 존재여부 확인
        /// </summary> 
        #endregion
        public ResponseModel GetFileInfo()
        {
            // TODO 사용되지 않습니다.
            /*
             * gubun = file_info
             * attachment_key = string
             */

            //_httpContext.Response.ClearHeaders();
            //_httpContext.Response.ClearContent();
            _httpContext.Response.Clear();

            string xmldata = XMLCommonUtil.XMLHeader;
            ResponseModel responseModel = null;
            try
            {
                string fileFullPath = this.ReturnFileFullPath(ATTACHMENT_KEY);

                //가져온 파일정보에 맞는 파일을 리턴한다.
                if (File.Exists(fileFullPath))
                {
                    //xmldata += xmlCommonUtil.returnMSGXML("1", "서버상에 해당 파일이 존재합니다.");
                    responseModel = ResponseModel.Message("1", "서버상에 해당 파일이 존재합니다.");
                }
                else
                {
                    //xmldata += xmlCommonUtil.returnErrorMSGXML("서버상에 해당 파일이 존재하지 않습니다.");
                    throw new ServiceException("서버상에 해당 파일이 존재하지 않습니다.");
                }
            }
            catch(ServiceException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                //xmldata += xmlCommonUtil.returnErrorMSGXML("httpservice(GetFileInfo).Error check", ex);
                throw new ServiceException("httpservice(GetFileInfo).Error check", ex);
            }
            finally
            {
                //_httpContext.Response.Write(xmldata);
                //_httpContext.Response.End();
            }

            return responseModel;
        }

        #region //파일 다운로드
        /// <summary>
        /// 파일 다운로드 
        /// </summary> 
        #endregion
        public ResponseModel DownloadFile()
        {
            // TODO 확인 사용되지 않습니다.
            /*
             * gubun = file_download
             * attachment_key = string
             */

            return this.DownloadFile(false);
        }

        public ResponseModel DownloadFile(bool isDirectDownloadFile)
        {
            //직접 다운로드 방식으로도 코딩해 둘 것.
            //gubun=file_download&attachment_key={0}

            if (isDirectDownloadFile)
            {
               return this.DownloadFile_Direct();
            }
            else
            {
                return this.DownloadFile_Response();
            }
        }

        /*구현안됨*/
        private ResponseModel DownloadFile_Direct()
        {
            //직접 다운로드 방식으로도 코딩해 둘 것.
            return ResponseModel.Empty;
        }

        private ResponseModel DownloadFile_Response()
        {
            //_httpContext.Response.ClearHeaders();
            //_httpContext.Response.ClearContent();
            //_httpContext.Response.Clear();

            //string xmldata = XMLCommonUtil.XMLHeader;

            string fileFullPath = this.ReturnFileFullPath(ATTACHMENT_KEY);
            try
            {
                //가져온 파일정보에 맞는 파일을 리턴한다.
                if (File.Exists(fileFullPath))
                {
                    FileInfo fileInfo = new FileInfo(fileFullPath);
                    //헤더에 파일이름 지정하기
                    //_httpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + _httpContext.Server.UrlPathEncode(fileInfo.Name));
                    //_httpContext.Response.ContentType = "multipart/form-data";

                    //_httpContext.Response.WriteFile(fileFullPath);

                    byte[] buffer = null;
                    using (var stream = fileInfo.OpenRead())
                    {
                        using (var reader = new BinaryReader(stream))
                        {
                            buffer = reader.ReadBytes((int)fileInfo.Length);

                            reader.Close();
                        }

                        stream.Close();
                    }

                    return new FileResponseModel
                    {
                        // TODO Uri encode가 필요한가?
                        FileName = fileInfo.Name,
                        // https://developer.mozilla.org/ko/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Complete_list_of_MIME_types
                        ContentType = "application/octet-stream",
                        Content = buffer,
                    };
                }

                return xmlCommonUtil.returnMSGXML("100", "해당 파일이 존재하지 않습니다.");

                /*
                else
                {
                    xmldata += xmlCommonUtil.returnMSGXML("100", "해당 파일이 존재하지 않습니다.");

                    _httpContext.Response.Write(xmldata);
                }
                //*/
            }
            catch (Exception ex)
            {
                //xmldata += xmlCommonUtil.returnErrorMSGXML("httpservice(DownloadFile).Error check", ex);

                //_httpContext.Response.Write(xmldata);

                var data = xmlCommonUtil.returnErrorMSGXML("httpservice(DownloadFile).Error check", ex);
                return data;

            }
            finally
            {
                //_httpContext.Response.End();
            }
        }

        #region //파일 삭제
        /// <summary>
        /// 파일 삭제
        /// </summary> 
        #endregion
        public ResponseModel DeleteFile()
        {
            /*
             * gubun = file_delete
             * (옵션1)************************************
             * attachment_key = string
             * (옵션2)************************************
             * attachment_gubun = string
             * file_name = string
             * db_work = pass
             */
            string fileFullPath = string.Empty;

            ResponseModel responseModel = null;

            if (!string.IsNullOrEmpty(ATTACHMENT_KEY))
            {
                fileFullPath = this.ReturnFileFullPath(ATTACHMENT_KEY);
            }
            else
            {
                //string namedFileName = xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_FILENAME_key];
                string namedFileName = xmlCommonUtil.RequestData.GetValue(FileCommonUtil.ATTACHMENT_FILENAME_key);
                if (!string.IsNullOrEmpty(namedFileName))
                {
                    fileFullPath = this.ATTACHMENT_UPLOAD_PATH + namedFileName;
                }
                else
                {
                    fileFullPath = string.Empty;
                }
            }

            string xmldata = XMLCommonUtil.XMLHeader;
            if (string.IsNullOrEmpty(fileFullPath))
            {
                //xmldata += xmlCommonUtil.returnErrorMSGXML("삭제할 파일 정보가 올바르지 않습니다.(파일 삭제 방식을 체크하세요.)");
                //_httpContext.Response.Clear();
                //_httpContext.Response.Write(xmldata);
                //_httpContext.Response.End();
                //return;

                throw new ServiceException("삭제할 파일 정보가 올바르지 않습니다.(파일 삭제 방식을 체크하세요.)");
            }

            try
            {
                string msg = string.Empty;
                //가져온 파일정보의 실제 파일을 삭제한다.
                if (File.Exists(fileFullPath))
                {
                    FileInfo fi = new FileInfo(fileFullPath);
                    string fileName = fi.Name;
                    string dirName = fi.DirectoryName;
                    if (!fileName.StartsWith("_del_"))
                    {
                        string fileFullPath2 = string.Format(@"{0}\{1}", dirName, "_del_" + fileName);
                        fileFullPath2 = this.MakeUniqueFileName(fileFullPath2);
                        fi.MoveTo(fileFullPath2);
                    }
                    //File.Delete(fileFullPath);//실제 삭제하지 않음.
                }

                if (string.IsNullOrEmpty(fileFullPath))
                {
                    msg += "파일정보가 존재하지 않습니다.";
                }

                //디비에서 해당 파일항목을 삭제한다.
                //string isDBWork = xmlCommonUtil.QueryString[FileCommonUtil.DB_WORK_GUBUN_value];
                string isDBWork = xmlCommonUtil.RequestData.GetValue(FileCommonUtil.DB_WORK_GUBUN_value);

                if (!string.IsNullOrEmpty(isDBWork) && !isDBWork.Equals("pass"))
                {
                    //db_work=pass 명시적으로 표시할 경우 디비 작업 없음.
                }
                else
                {
                    this.Attachment_D(ATTACHMENT_KEY);
                }

                //xmldata += xmlCommonUtil.returnMSGXML("1", "파일 삭제완료.\r\n" + msg);
                responseModel = ResponseModel.Message("1", "파일 삭제완료.\r\n" + msg);
            }
            catch(ServiceException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                //xmldata += xmlCommonUtil.returnErrorMSGXML("httpservice(DeleteFile).Error check", ex);
                throw new ServiceException("httpservice(DeleteFile).Error check", ex);
            }
            //_httpContext.Response.Clear();
            //_httpContext.Response.Write(xmldata);
            //_httpContext.Response.End();

            return responseModel;
        }

        #region //파일 목록 조회
        /// <summary>
        /// 파일 목록 조회
        /// </summary> 
        #endregion
        public ResponseModel GetFileNameList()
        {
            /*
             * gubun = file_list
             * attachment_gubun = string
             * (옵션-파일명 필터링)file_name = string
             * (옵션-확장자 필터링)file_format = string
             * ******************************************************
             * (옵션-데이터베이스기준 리스트, 구현안됨)db_work = yes
             */
            //_httpContext.Response.ClearHeaders();
            //_httpContext.Response.ClearContent();
            _httpContext.Response.Clear();

            string xmldata = XMLCommonUtil.XMLHeader;
            ResponseModel responseModel = null;

            try
            {
                string fileBasicURL = ATTACHMENT_FILE_BASIC_URL;
                string fileDir = ATTACHMENT_UPLOAD_PATH;

                if (!Directory.Exists(fileDir))
                {
                    //xmldata += xmlCommonUtil.returnErrorMSGXML("서버상에 해당 폴더가 존재하지 않습니다.");
                    //_httpContext.Response.Write(xmldata);
                    //_httpContext.Response.End();
                    //return;

                    throw new ServiceException("서버상에 해당 폴더가 존재하지 않습니다.");
                }
                DirectoryInfo di = new DirectoryInfo(fileDir);

                string filterString = null;

                //파일이름을 filtering할 것인가? file_name
                //string filterFileName = xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_FILENAME_key];
                string filterFileName = xmlCommonUtil.RequestData.GetValue( FileCommonUtil.ATTACHMENT_FILENAME_key);
                bool includeFilterFileName = !string.IsNullOrEmpty(filterFileName) && (filterFileName.Trim() != string.Empty);

                //파일확장자를 filtering할 것인가? file_extension
                //string filterFileExtension = xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_FILEFORMAT_key];
                string filterFileExtension = xmlCommonUtil.RequestData.GetValue(FileCommonUtil.ATTACHMENT_FILEFORMAT_key);
                bool includeFilterFileExtension = !string.IsNullOrEmpty(filterFileExtension) && (filterFileExtension.Trim() != string.Empty);

                if (includeFilterFileName && includeFilterFileExtension)
                {
                    filterString = //"*" + 
                        filterFileName.Trim() + "*." + filterFileExtension.Trim();
                }
                else if (includeFilterFileName)
                {
                    filterString = //"*" + 
                        filterFileName.Trim() + "*.*";
                }
                else if (includeFilterFileExtension)
                {
                    filterString = "*." + filterFileExtension.Trim();
                }

                FileInfo[] fis = string.IsNullOrEmpty(filterString) ? di.GetFiles() : di.GetFiles(filterString);

                xmldata += string.Format("<values>{0}", Environment.NewLine);

                responseModel = new ResponseModel();
                //responseModel.Values = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();
                responseModel.Values = new ValuesModel();
                var filesList = new List<Dictionary<string, object>>();
                for (int i = 0; i < fis.Length; i++)
                {
                    //xmldata += string.Format("<item>{0}", Environment.NewLine);

                    //xmldata += string.Format("<{0}>{1}</{0}>{2}",
                    //    "file_name", fis[i].Name, Environment.NewLine);
                    //xmldata += string.Format("<{0}>{1}</{0}>{2}",
                    //    "file_creationtime", fis[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"), Environment.NewLine);
                    //xmldata += string.Format("<{0}>{1}</{0}>{2}",
                    //    "file_url", fileBasicURL + fis[i].Name, Environment.NewLine);

                    //xmldata += string.Format("</item>{0}", Environment.NewLine);

                    filesList.Add(new Dictionary<string, object>()
                    {
                        ["file_name"] = fis[i].Name,
                        ["file_creationtime"] = fis[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["file_url"] = fileBasicURL + fis[i].Name
                    });                    
                }

                responseModel.Values.Add("item", filesList);

                //xmldata += "</values>";
            }
            catch(ServiceException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                //xmldata += xmlCommonUtil.returnErrorMSGXML("httpservice(GetFileNameList).Error check", ex);
                throw new ServiceException("httpservice(GetFileNameList).Error check", ex);
            }
            finally
            {
                //_httpContext.Response.Write(xmldata);
                //_httpContext.Response.End();
            }

            return responseModel;
        }

        #region //파일 이름 변경
        /// <summary>
        /// 파일 이름 변경
        /// </summary> 
        #endregion
        public ResponseModel FileRename()
        {
            // TODO 사용되지 않습니다.
            /*
             * gubun = file_rename
             * attachment_gubun = string
             * target_file_name = string
             * file_name = string
             * *******************************************
             * (옵션-DB 업데이트, 구현안됨)db_work = yes
             * (옵션-디비수정,구현안됨)attachment_key = string
             */
            //_httpContext.Response.ClearHeaders();
            //_httpContext.Response.ClearContent();
            _httpContext.Response.Clear();

            string xmldata = XMLCommonUtil.XMLHeader;

            ResponseModel responseModel = null;

            try
            {
                //string newFileName = xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_FILENAME_key];
                string newFileName = xmlCommonUtil.RequestData.GetValue( FileCommonUtil.ATTACHMENT_FILENAME_key);

                string fileDir = ATTACHMENT_UPLOAD_PATH;
                //string targetFileName = xmlCommonUtil.QueryString[FileCommonUtil.ATTACHMENT_FILETARGET_key];
                string targetFileName = xmlCommonUtil.RequestData.GetValue(FileCommonUtil.ATTACHMENT_FILETARGET_key);
                string fileFullPath = fileDir + targetFileName;


                string msg = string.Empty;
                if (File.Exists(fileFullPath))
                {
                    string fileName = newFileName;
                    string dirName = fileDir;

                    string fileFullPath2 = string.Format(@"{0}\{1}", dirName, fileName);
                    fileFullPath2 = this.MakeUniqueFileName(fileFullPath2);
                    File.Move(fileFullPath, fileFullPath2);

                    FileInfo fi = new FileInfo(fileFullPath2);
                    string renamedFileName = fi.Name;

                    msg += string.Format("파일이름이 수정되었습니다.\n{0}->{1}", targetFileName, renamedFileName);
                }
                else
                {
                    msg += "이름을 변경할 파일정보가 존재하지 않습니다.";
                }

                //xmldata += xmlCommonUtil.returnMSGXML("1", "파일 이름 변경.\n" + msg);
                responseModel = ResponseModel.Message("1", "파일 이름 변경.\n" + msg);
            }
            catch(ServiceException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                xmldata += xmlCommonUtil.returnErrorMSGXML("httpservice(FileRename).Error check", ex);
                throw new ServiceException("httpservice(FileRename).Error check", ex);
            }
            finally
            {
                //_httpContext.Response.Write(xmldata);
                //_httpContext.Response.End();
            }

            return responseModel;
        }

        public string MakeUniqueFileName(string fileFullPath)
        {
            string fileFullPath2 = fileFullPath;
            string returnFileFullPath = fileFullPath;

            FileInfo fileInfo = new FileInfo(fileFullPath);
            int newFileNumber = 1;

            while (File.Exists(fileFullPath2))
            {
                string directoryName = fileInfo.DirectoryName;

                //*수정
                string fileName = string.Format("{0}_{1}", fileInfo.Name.Split('.')[0],
                    (newFileNumber > 1 ? newFileNumber.ToString() + "_" : string.Empty) + DateTime.Now.ToString("yyyyMMddhhmmss"));
                string fileExtension = fileInfo.Extension;
                fileFullPath2 = string.Format(@"{0}\{1}{2}", directoryName, fileName, fileExtension);
                //*/

                /*이전
                string fileName = string.Format("{0}({1})", fileInfo.Name.Split('.')[0], newFileNumber);
                string fileExtension = fileInfo.Extension;
                fileFullPath2 = string.Format(@"{0}\{1}{2}", directoryName, fileName, fileExtension);
                //*/
                newFileNumber++;
            }
            returnFileFullPath = fileFullPath2;

            return returnFileFullPath;
        }

        public ResponseModel Attachment_C(string filename, string fileformat, long filesize, out string resultXML)
        {
            var responseModel = new ResponseModel();

            string return_resultXML = null;
            string return_string = string.Empty;

            resultXML = String.Empty;

            try
            {
                /*throw new Exception("test_exception");*/
                DataSet ds = this.Attachment_CRD(CRUD.C, null, filename, fileformat, filesize, out return_string);
                if (ds != null)
                {
                    ds.DataSetName = XMLCommonUtil.DATASET_NAME;
                    ds.Tables[0].TableName = XMLCommonUtil.TABLE_NAME;
                    return_resultXML = ds.GetXml();

                    if(ds.Tables.Count > 0)
                    {
                        ds.Tables[0].TableName = "item";

                        responseModel.Values.Add("item", ds.Tables[0].ToDictionaryEnumerable());
                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                //return_string = xmlCommonUtil.returnErrorMSGXML("Attachment_C", ex);
                throw new ServiceException("Attachment_C", ex);
            }

            //resultXML = return_resultXML;
            //return return_string;
            return responseModel;
        }

        public void Attachment_D(string attachment_key)
        {
            string temp = string.Empty;
            this.Attachment_CRD(CRUD.D, attachment_key, null, null, 0, out temp);

            if (!string.IsNullOrEmpty(temp))
            {
                //_httpContext.Response.Clear();
                //_httpContext.Response.Write(temp);
                //_httpContext.Response.End();
                //return;

                throw new ServiceException(temp);
            }
        }


        public DataRow Attachment_R(string attachment_key)
        {
            string temp = string.Empty;
            DataSet ds = this.Attachment_CRD(CRUD.R, attachment_key, null, null, 0, out temp);

            DataRow dr = null;

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                dr = ds.Tables[0].Rows[0];
            }


            if (!string.IsNullOrEmpty(temp))
            {
                //_httpContext.Response.Clear();
                //_httpContext.Response.Write(temp);
                //_httpContext.Response.End();

                throw new Exception(temp);
            }

            return dr;
        }
        private DataSet Attachment_CRD(
            CRUD crd,
            string attachment_key,
            string filename,
            string fileformat,
            long filesize,
            out string msg)
        {
            string out_msg = string.Empty;

            //
            string proc_name = this.ATTACHMENT_R_PROC;
            switch (crd)
            {
                case CRUD.C:
                case CRUD.U:
                case CRUD.D:
                    proc_name = this.ATTACHMENT_CUD_PROC;
                    break;
                case CRUD.R:
                    proc_name = this.ATTACHMENT_R_PROC;
                    break;
            }

            //
            List<SqlParameter> temp_param = new List<SqlParameter>();
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.GUBUN_KEY_STRING, crd.ToString()));
            //temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_IP_GUBUN, _httpContext.Request.ServerVariables["REMOTE_ADDR"]));
            temp_param.Add(new SqlParameter("@" + XMLCommonUtil.OPERATOR_IP_GUBUN, _httpContext.Connection.RemoteIpAddress.ToString()));

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

            switch (crd)
            {
                case CRUD.R:
                case CRUD.D:
                    temp_param.Add(new SqlParameter("@" + ATTACHMENT_KEY_key, attachment_key));
                    break;
                case CRUD.C:
                    temp_param.Add(new SqlParameter("@" + ATTACHMENT_GUBUN_key, this.ATTACHMENT_TYPE.ToString()));
                    temp_param.Add(new SqlParameter("@" + ATTACHMENT_DETAIL_CODE_key, this.ATTACHMENT_DETAIL_CODE));
                    temp_param.Add(new SqlParameter("@" + ATTACHMENT_FILENAME_key, filename));
                    temp_param.Add(new SqlParameter("@" + ATTACHMENT_FILEFORMAT_key, fileformat));
                    temp_param.Add(new SqlParameter("@" + ATTACHMENT_FILESIZE_key, filesize));
                    break;
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
                //throw ex;
                //msg = xmlCommonUtil.returnErrorMSGXML("ReturnDataSet_Common", ex);
                msg = "ReturnDataSet_Common";

                return null;
            }

            msg = out_msg;
            /*
            if (ds == null || ds.Tables.Count ==0 || ds.Tables[0].Rows.Count == 0)
            {
                return null;
            }

            DataTable dt = ds.Tables[0];
            //*/
            return ds;//dt.Rows[0];
        }

        public string ReturnFileFullPath(string attachment_key)
        {
            bool include_organization_key = XMLCommonUtil.INCLUDE_ORGANIZATION_KEY;

            string attachment_filename = "";
            string returnfilefullpath = "";
            try
            {
                //파일정보를 가져온다.
                DataRow dr = this.Attachment_R(attachment_key);

                if (dr == null)
                {
                    return string.Empty;
                }

                string organization_key = include_organization_key ? dr[XMLCommonUtil.ORGANIZATION_KEY_GUBUN].ToString() : null;
                string attachment_gubun = dr[ATTACHMENT_GUBUN_key].ToString();
                string attachment_detail_code = dr[ATTACHMENT_DETAIL_CODE_key].ToString();
                attachment_filename = dr[ATTACHMENT_FILENAME_key].ToString();

                returnfilefullpath = ReturnDirectoryPath(organization_key, attachment_gubun, attachment_detail_code);
            }
            catch (Exception ex)
            {
                //string xmldata = XMLCommonUtil.XMLHeader;

                //xmldata += xmlCommonUtil.returnErrorMSGXML("httpservice(ReturnFileFullPath).Error check", ex);


                //_httpContext.Response.Write(xmldata);
                //_httpContext.Response.End();

                throw new Exception("httpservice(ReturnFileFullPath).Error check", ex);
            }

            return returnfilefullpath + "\\" + attachment_filename;
        }

        private string ReturnDirectoryPath(string header, string attachment_gubun, string attachment_detail_code)
        {
            //string returnPath = ConfigurationManager.AppSettings[attachment_gubun];
            string returnPath = appOptions[attachment_gubun];
            //*각 문서키별로 첨부파일 관리!!
            try
            {
                string detail_code = attachment_detail_code;
                string year = detail_code.Substring(2, 4);
                string month = detail_code.Substring(6, 2);

                bool include_organization_key = XMLCommonUtil.INCLUDE_ORGANIZATION_KEY;

                if (include_organization_key || !string.IsNullOrEmpty(header))
                {
                    returnPath = string.Format(
                        @"{0}{1}\{2}\{3}\{4}\",
                        returnPath, header, year, month, detail_code);
                    //첨부파일 종류별 저장위치\단체(회사,모임)키\년도\월\첨부파일 디테일 코드
                }
                else
                {
                    returnPath = string.Format(
                        @"{0}{1}\{2}\{3}\",
                        returnPath, year, month, detail_code);
                    //첨부파일 종류별 저장위치\년도\월\첨부파일 디테일 코드
                }
            }
            catch
            {

                returnPath = string.Format(@"{0}{1}\{2}\", returnPath, header, attachment_detail_code);
                //AttachmentType.etc.ToString());
            }
            //*/

            /*단순하게 첨부파일 관리!!
            returnPath = string.Format(@"{0}{1}\", returnPath, header);
            //*/

            return returnPath;
        }

        public string ATTACHMENT_UPLOAD_PATH
        {
            get
            {
                bool include_organization_key = XMLCommonUtil.INCLUDE_ORGANIZATION_KEY;

                //string returnPath = ReturnDirectoryPath(
                //    include_organization_key ? xmlCommonUtil.QueryString[XMLCommonUtil.ORGANIZATION_KEY_GUBUN].ToString() : null,
                //    this.ATTACHMENT_TYPE.ToString(),
                //    this.ATTACHMENT_DETAIL_CODE);

                string returnPath = ReturnDirectoryPath(
                    include_organization_key ? xmlCommonUtil.RequestData.GetValue(XMLCommonUtil.ORGANIZATION_KEY_GUBUN) : null,
                    this.ATTACHMENT_TYPE.ToString(),
                    this.ATTACHMENT_DETAIL_CODE);

                return returnPath;
            }
        }

        public string ATTACHMENT_FILE_BASIC_URL
        {
            get
            {
                //return ConfigurationManager.AppSettings[this.ATTACHMENT_TYPE + "_path"] == null ?
                //    string.Empty :
                //    ConfigurationManager.AppSettings[this.ATTACHMENT_TYPE + "_path"].ToString(); ;
                string attachmentFileBaicUrl = String.Empty;
                if (!appOptions.Properties.TryGetValue(this.ATTACHMENT_TYPE + "_path", out attachmentFileBaicUrl))
                {
                    attachmentFileBaicUrl = String.Empty;
                }

                return attachmentFileBaicUrl;
            }
        }

        #region //AttachmentType 사용안함.
        //enumdatatype AttachmentType 사용안함.
        //private AttachmentType ATTACHMENT_TYPE
        //{
        //    get
        //    {
        //        AttachmentType _attachmentType = AttachmentType.etc;

        //        try
        //        {
        //            string gubun = xmlCommonUtil.QueryString[ATTACHMENT_GUBUN_string] == null ?
        //                AttachmentType.etc.ToString() :
        //                xmlCommonUtil.QueryString[ATTACHMENT_GUBUN_string].ToString();

        //            _attachmentType = (AttachmentType)Enum.Parse(typeof(AttachmentType), gubun);
        //        }
        //        catch //(Exception)
        //        {
        //            _attachmentType = AttachmentType.etc;
        //        }

        //        return _attachmentType;
        //    }
        //}
        //
        #endregion

        private string ATTACHMENT_TYPE
        {
            get
            {
                //return xmlCommonUtil.QueryString[ATTACHMENT_GUBUN_key] == null ?
                //    "etc" :
                //    xmlCommonUtil.QueryString[ATTACHMENT_GUBUN_key].ToString();

                if (xmlCommonUtil.RequestData.Parameters.ContainsKey(ATTACHMENT_GUBUN_key))
                {
                    return xmlCommonUtil.RequestData.Parameters[ATTACHMENT_GUBUN_key];
                }

                return "etc";
            }
        }

        private string ATTACHMENT_DETAIL_CODE
        {
            get
            {
                if (xmlCommonUtil.RequestData.Parameters.ContainsKey(ATTACHMENT_DETAIL_CODE_key))
                {
                    return xmlCommonUtil.RequestData.Parameters[ATTACHMENT_DETAIL_CODE_key];
                }

                return String.Empty;

                //return xmlCommonUtil.QueryString[ATTACHMENT_DETAIL_CODE_key] == null ?
                //    string.Empty :
                //    xmlCommonUtil.QueryString[ATTACHMENT_DETAIL_CODE_key].ToString();
            }
        }
        public string ATTACHMENT_KEY
        {
            get
            {
                if (xmlCommonUtil.RequestData.Parameters.ContainsKey(ATTACHMENT_KEY_key))
                {
                    return xmlCommonUtil.RequestData.Parameters[ATTACHMENT_KEY_key];
                }

                return String.Empty;

                //return xmlCommonUtil.QueryString[ATTACHMENT_KEY_key] == null ?
                //    string.Empty :
                //    xmlCommonUtil.QueryString[ATTACHMENT_KEY_key].ToString();
            }
        }
        private string ATTACHMENT_CUD_PROC
        {
            get
            {
                //return XMLCommonUtil.DB_SCHEMA + "." + ATTACHMENT_CUD_value;
                return $"{appOptions.DbSchema}.{ATTACHMENT_CUD_value}";
            }
        }
        private string ATTACHMENT_R_PROC
        {
            get
            {
                //return XMLCommonUtil.DB_SCHEMA + "." + ATTACHMENT_R_value;
                return $"{appOptions.DbSchema}.{ATTACHMENT_R_value}";
            }
        }


        public string CheckAttachmentKeyAndReturnFullFilePath()
        {
            string fileFullPath = null;

            //FileCommonUtil fcu = new FileCommonUtil();
            if (!string.IsNullOrEmpty(this.ATTACHMENT_KEY))//attachment_key
            {
                fileFullPath = this.ReturnFileFullPath(this.ATTACHMENT_KEY);
            }

            return fileFullPath;
        }

        private readonly AppOptions appOptions;
    }
}
