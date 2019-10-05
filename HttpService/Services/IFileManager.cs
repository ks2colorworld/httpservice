using HttpService.Models;
using HttpService.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IFileManager
    {
        Task<ResponseModel> GetFileInfo(RequestModel model);

        Task<ResponseModel> DownloadFile(bool isDirectDownloadFile = false);

        Task<ResponseModel> DeleteFile(RequestModel model);
    }

    public class FileManager
    {
        public FileManager(
            IDatabaseManager databaseManager,
            IResponsePreprocessManager responsePreprocessManager,
            IOptions<AppOptions> appOptionsAccessor)
        {
            this.databaseManager = databaseManager;
            appOptions = appOptionsAccessor.Value;
        }

        /// <summary>
        /// 파일 존재여부 확인
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ResponseModel> GetFileInfo(RequestModel model)
        {
            ResponseModel result = null;
            try
            {
                string fileFullPath = await this.ReturnFileFullPath(model);

                if (!System.IO.File.Exists(fileFullPath))
                {
                    throw new ServiceException("서버상에 해당 파일이 존재하지 않습니다.");
                }

                result = ResponseModel.Message("1", "서버상에 해당 파일이 존재합니다.");
            }
            catch (Exception ex)
            {
                result = responsePreprocessManager.ProcessException(ex);
            }

            return result;
        }

        /// <summary>
        /// 파일 다운로드
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isDirectDownloadFile"></param>
        /// <returns></returns>
        public async Task<ResponseModel> DownloadFile(RequestModel model, bool isDirectDownloadFile = false)
        {
            ResponseModel result = null;

            if (isDirectDownloadFile)
            {
                result = DownloadFile_Direct(model);
            }
            else
            {
                result = await DownloadFile_Response(model);
            }

            return result;
        }

        public async Task<ResponseModel> Deletefile(RequestModel model)
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

            if (model.HasKey(Constants.ATTACHMENT_KEY_key))
            {
                fileFullPath = await this.ReturnFileFullPath(model);
            }
            else
            {
                string namedFileName = model.GetValue(Constants.ATTACHMENT_FILENAME_key);
                if (!string.IsNullOrEmpty(namedFileName))
                {
                    var uploadPath = GetATTACHMENT_UPLOAD_PATH(model);

                    fileFullPath = System.IO.Path.Join(uploadPath, namedFileName);
                }
                else
                {
                    fileFullPath = string.Empty;
                }
            }


            try
            {
                if (String.IsNullOrEmpty(fileFullPath))
                {
                    throw new ServiceException("삭제할 파일 정보가 올바르지 않습니다.(파일 삭제 방식을 체크하세요.)");
                }

                //가져온 파일정보의 실제 파일을 삭제한다.
                if (File.Exists(fileFullPath))
                {
                    FileInfo fi = new FileInfo(fileFullPath);

                    string fileName = fi.Name;
                    string dirName = fi.DirectoryName;

                    if (!fileName.StartsWith("_del_"))
                    {
                        //string fileFullPath2 = string.Format(@"{0}\{1}", dirName, "_del_" + fileName);
                        string fileFullPath2 = Path.Join(dirName, "_del_", fileName);

                        fileFullPath2 = this.MakeUniqueFileName(fileFullPath2);
                        fi.MoveTo(fileFullPath2);
                    }

                    //File.Delete(fileFullPath);//실제 삭제하지 않음.
                }

                //디비에서 해당 파일항목을 삭제한다.
                string isDBWork = model.GetValue(Constants.DB_WORK_GUBUN_value);

                if (!string.IsNullOrEmpty(isDBWork) && !isDBWork.Equals("pass"))
                {
                    //db_work=pass 명시적으로 표시할 경우 디비 작업 없음.
                }
                else
                {
                    //this.Attachment_D(ATTACHMENT_KEY);

                    await databaseManager.Attachment_CRDAsync(Lib.CRUD.D, model, FileModel.Empty);
                }

                //responseModel = ResponseModel.Message("1", "파일 삭제완료.\r\n" + msg);
                responseModel = ResponseModel.Message("1", "파일 삭제완료.");
            }
            catch (ServiceException ex)
            {
                responseModel = responsePreprocessManager.ProcessException(ex);
            }
            catch (Exception ex)
            {
                //throw new ServiceException("httpservice(DeleteFile).Error check", ex);
                responseModel = ResponseModel.ErrorMessage(ex,"httpservice(DeleteFile).Error check");
            }       

            return responseModel;
        }

        private async Task<ResponseModel> DownloadFile_Response(RequestModel model)
        {
            string file = await this.ReturnFileFullPath(model);

            var result = responsePreprocessManager.ProcessFile(file);

            return result;
        }

        private ResponseModel DownloadFile_Direct(RequestModel model)
        {
            throw new NotImplementedException();
        }

        private async Task<string> ReturnFileFullPath(RequestModel model)
        {
            var attachment_key = model.GetValue(Constants.ATTACHMENT_KEY_key);

            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;

            string attachment_filename = "";
            string returnfilefullpath = "";
            try
            {
                //파일정보를 가져온다.
                DataSet dataSet = await databaseManager.Attachment_CRDAsync(Lib.CRUD.R, model, new FileModel());

                if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                {
                    throw new ServiceException("파일이 존재하지 않습니다.");
                }

                var dr = dataSet.Tables[0].Select().FirstOrDefault();

                string organization_key = include_organization_key ? dr[Constants.ORGANIZATION_KEY_GUBUN].ToString() : null;

                string attachment_gubun = dr[Constants.ATTACHMENT_GUBUN_key].ToString();
                string attachment_detail_code = dr[Constants.ATTACHMENT_DETAIL_CODE_key].ToString();
                attachment_filename = dr[Constants.ATTACHMENT_FILENAME_key].ToString();

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

            var filePath = System.IO.Path.Join(returnfilefullpath, attachment_filename);

            return filePath;
        }

        private string GetATTACHMENT_UPLOAD_PATH(RequestModel model)
        {
            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;
            var header = include_organization_key ? model.GetValue(Constants.ORGANIZATION_KEY_GUBUN) : null;
           
            string returnPath = ReturnDirectoryPath(
                  header,
                  model.GetValue(Constants.ATTACHMENT_GUBUN_key, "etc"),
                  model.GetValue(Constants.ATTACHMENT_DETAIL_CODE_key));

            return returnPath;
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

                bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;

                if (include_organization_key || !string.IsNullOrEmpty(header))
                {
                    // linux 디렉터리 구분자 대응안됨
                    //returnPath = string.Format(
                    //    @"{0}{1}\{2}\{3}\{4}\",
                    //    returnPath, header, year, month, detail_code);
                    //첨부파일 종류별 저장위치\단체(회사,모임)키\년도\월\첨부파일 디테일 코드

                    returnPath = System.IO.Path.Join(returnPath, header, year, month, detail_code);
                }
                else
                {
                    //returnPath = string.Format(
                    //    @"{0}{1}\{2}\{3}\",
                    //    returnPath, year, month, detail_code);
                    //첨부파일 종류별 저장위치\년도\월\첨부파일 디테일 코드
                    returnPath = System.IO.Path.Join(returnPath, year, month, detail_code);
                }
            }
            catch
            {

                //returnPath = string.Format(@"{0}{1}\{2}\", returnPath, header, attachment_detail_code);
                //AttachmentType.etc.ToString());

                returnPath = System.IO.Path.Join(returnPath, header, attachment_detail_code);
            }
            //*/

            /*단순하게 첨부파일 관리!!
            returnPath = string.Format(@"{0}{1}\", returnPath, header);
            //*/

            return returnPath;
        }

        private string MakeUniqueFileName(string fileFullPath)
        {
            string fileFullPath2 = fileFullPath;
            string returnFileFullPath = fileFullPath;

            FileInfo fileInfo = new FileInfo(fileFullPath);
            int newFileNumber = 1;

            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            while (File.Exists(fileFullPath2))
            {
                string directoryName = fileInfo.DirectoryName;

                //*수정
                string fileName = string.Format("{0}_{1}", fileInfo.Name.Split('.')[0],
                    (newFileNumber > 1 ? newFileNumber.ToString() + "_" : string.Empty) + DateTime.Now.ToString("yyyyMMddhhmmss"));

                string fileExtension = fileInfo.Extension;
                //fileFullPath2 = string.Format(@"{0}\{1}{2}", directoryName, fileName, fileExtension);
                fileFullPath2 = Path.Join(directoryName, $"{fileName}{fileExtension}");

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

        private readonly IDatabaseManager databaseManager;
        private readonly IResponsePreprocessManager responsePreprocessManager;
        private readonly AppOptions appOptions;
    }
}
