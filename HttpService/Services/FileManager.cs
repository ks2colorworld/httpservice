using HttpService.Models;
using HttpService.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public class FileManager : IFileManager
    {
        public FileManager(
            IDatabaseManager databaseManager,
            IHttpContextManager httpContextManager,
            IResponsePreprocessManager responsePreprocessManager,
            IOptions<AppOptions> appOptionsAccessor)
        {
            this.databaseManager = databaseManager;
            this.httpContextManager = httpContextManager;
            this.responsePreprocessManager = responsePreprocessManager;

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

        public async Task<string> ReturnFileFullPath(RequestModel model)
        {
            //var attachment_key = model.GetValue(Constants.ATTACHMENT_KEY_key);

            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;

            string attachment_filename = "";
            string returnfilefullpath = "";
            try
            {
                //파일정보를 가져온다.
                DataSet dataSet = await Attachment_CRDAsync(CRUD.R, model, FileModel.Empty);

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
                throw new Exception("httpservice(ReturnFileFullPath).Error check", ex);
            }

            var filePath = System.IO.Path.Join(returnfilefullpath, attachment_filename);

            return filePath;
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

        public async Task<ResponseModel> DeleteFile(RequestModel model)
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
                    await Attachment_CRDAsync(CRUD.D, model,  FileModel.Empty);
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

        public Task<ResponseModel> GetFileNameList(RequestModel model)
        {
            /*
            * gubun = file_list
            * attachment_gubun = string
            * (옵션-파일명 필터링)file_name = string
            * (옵션-확장자 필터링)file_format = string
            * ******************************************************
            * (옵션-데이터베이스기준 리스트, 구현안됨)db_work = yes
            */

            ResponseModel responseModel = null;

            try
            {
                string fileBasicURL = GetATTACHMENT_FILE_BASIC_URL(model);
                string fileDir = GetATTACHMENT_UPLOAD_PATH(model);

                if (!Directory.Exists(fileDir))
                {
                    throw new ServiceException("서버상에 해당 폴더가 존재하지 않습니다.");
                }

                DirectoryInfo di = new DirectoryInfo(fileDir);

                string filterString = null;

                //파일이름을 filtering할 것인가? file_name
                string filterFileName = model.GetValue(Constants.ATTACHMENT_FILENAME_key);
                bool includeFilterFileName = !string.IsNullOrEmpty(filterFileName) && (filterFileName.Trim() != string.Empty);

                //파일확장자를 filtering할 것인가? file_extension
                string filterFileExtension = model.GetValue(Constants.ATTACHMENT_FILEFORMAT_key);
                bool includeFilterFileExtension = !string.IsNullOrEmpty(filterFileExtension) && (filterFileExtension.Trim() != string.Empty);

                if (includeFilterFileName && includeFilterFileExtension)
                {
                    filterString = $"{filterFileName.Trim()}*.{filterFileExtension.Trim()}";
                }
                else if (includeFilterFileName)
                {
                    filterString = $"{filterFileName.Trim()}*.*";
                }
                else if (includeFilterFileExtension)
                {
                    filterString = $"*.{filterFileExtension.Trim()}";
                }

                FileInfo[] fis = string.IsNullOrEmpty(filterString) ? di.GetFiles() : di.GetFiles(filterString);

                responseModel = new ResponseModel();
                //responseModel.Values = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();
                responseModel.Values = new ValuesModel();
                var filesList = new List<Dictionary<string, object>>();
                for (int i = 0; i < fis.Length; i++)
                {
                    var fileName = fis[i].Name;
                    var filePath = Path.Join(fileBasicURL, fis[i].Name);

                    filesList.Add(new Dictionary<string, object>()
                    {
                        ["file_name"] = fileName,
                        ["file_creationtime"] = fis[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["file_url"] = filePath,
                    });
                }

                responseModel.Values.Add("item", filesList);
            }
            catch (ServiceException ex)
            {
                responseModel = responsePreprocessManager.ProcessException(ex);
            }
            catch (Exception ex)
            {
                responseModel = responsePreprocessManager.ProcessException(new ServiceException("httpservice(GetFileNameList).Error check", ex));
                //throw new ServiceException("httpservice(GetFileNameList).Error check", ex);
            }
            finally
            {

            }

            return Task.FromResult(responseModel);
        }

        public Task<ResponseModel> FileRename(RequestModel model)
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

            ResponseModel responseModel = null;
            string message = String.Empty;
            try
            {
                string newFileName = model.GetValue(Constants.ATTACHMENT_FILENAME_key);
                string fileDir = GetATTACHMENT_UPLOAD_PATH(model);

                string targetFileName = model.GetValue(Constants.ATTACHMENT_FILETARGET_key);
                string fileFullPath = Path.Join(fileDir, targetFileName);


                if (File.Exists(fileFullPath))
                {
                    string fileName = newFileName;
                    string dirName = fileDir;

                    //string fileFullPath2 = string.Format(@"{0}\{1}", dirName, fileName);
                    string fileFullPath2 = Path.Join(dirName, fileName);
                    fileFullPath2 = this.MakeUniqueFileName(fileFullPath2);
                    File.Move(fileFullPath, fileFullPath2);

                    FileInfo fi = new FileInfo(fileFullPath2);
                    string renamedFileName = fi.Name;

                    message += $"파일이름이 수정되었습니다.{Environment.NewLine}{targetFileName}->{renamedFileName}";
                }
                else
                {
                    message += "이름을 변경할 파일정보가 존재하지 않습니다.";
                }

                responseModel = ResponseModel.Message("1", $"파일 이름 변경.{Environment.NewLine}{message}");
            }
            catch (ServiceException ex)
            {
                responseModel = responsePreprocessManager.ProcessException(ex);
            }
            catch (Exception ex)
            {
                responseModel = responsePreprocessManager.ProcessException(new ServiceException("httpservice(FileRename).Error check", ex));
            }
            finally
            {

            }

            return Task.FromResult(responseModel);
        }

        public async Task<ResponseModel> UploadFile(RequestModel model)
        {
            ResponseModel responseModel = null;

            try
            {
                /*웹서버 세션 정보 체크*/
                string sessionID = httpContextManager.GetSessionId();
                string client_SessionID = model.GetValue(Constants.SESSIONID_KEY_STRING);

                if (Constants.SESSION_CHECK && String.IsNullOrWhiteSpace(client_SessionID))
                {
                    throw new ServiceException("세션정보가 빈값이거나, 세션 정보를 넘기지 않습니다. 로그인 후 사용바랍니다.");
                }
                else if (Constants.SESSION_CHECK && httpContextManager.CheckSessionId(client_SessionID))
                {
                    throw new ServiceException("세션 정보가 올바르지 않습니다. 다시 로그인한 후 사용바랍니다.");
                }

                var formFiles = httpContextManager.GetFormFiles();

                /*파일 업로드 갯수 제한*/
                if (formFiles.Count == 0)
                {
                    throw new ServiceException("업로드 파일이 없습니다.");
                }
                else if (formFiles.Count > 1)
                {
                    throw new ServiceException("파일은 하나씩만 업로드 가능합니다.");
                }

                //저장위치를 지정
                //string uploadDir = ConfigurationManager.AppSettings["etc"];
                var uploadDir = appOptions.Etc;

                uploadDir = GetATTACHMENT_UPLOAD_PATH(model);

                //디렉토리 체크
                if (!Directory.Exists(uploadDir))
                {
                    try
                    {
                        Directory.CreateDirectory(uploadDir);
                    }
                    catch (Exception ex)
                    {
                        throw new ServiceException("파일경로를 만들 수 없습니다.", ex);
                    }
                }

                // 파일을 저장하고,(+이미지 파일일 경우 섬네일이미지를 생성하고 - 구현안됨)

                foreach (var file in formFiles)
                {
                    //HttpPostedFile file = _context.Request.Files[fileKey];

                    //파일이름을 지정하여 저장할 것인가? file_name
                    string namedFileName = model.GetValue(Constants.ATTACHMENT_FILENAME_key);
                    string fileFullPath = String.Empty;

                    if (!String.IsNullOrEmpty(namedFileName))
                    {
                        fileFullPath = Path.Join(uploadDir, namedFileName);
                    }
                    else
                    {
                        fileFullPath = Path.Join(uploadDir, file.FileName);
                    }

                    fileFullPath = MakeUniqueFileName(fileFullPath);

                    using (var stream = file.OpenReadStream())
                    {
                        using (var destinationStream = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(destinationStream);
                            await destinationStream.FlushAsync();
                            destinationStream.Close();
                        }
                        stream.Close();
                    }

                    FileInfo fileInfo = new FileInfo(fileFullPath);
                    var attachment_filename = fileInfo.Name;
                    var attachment_fileformat = fileInfo.Extension;
                    var attachment_filesize = fileInfo.Length;

                    // 디비를 저장하고
                    // 디비에 저장할 것인가? db_work
                    string isDBWork = model.GetValue(Constants.DB_WORK_GUBUN_value);
                    string return_msg = string.Empty;
                    string resultXML = String.Empty;

                    if (!String.IsNullOrEmpty(isDBWork) && !isDBWork.Equals("pass"))
                    {
                        //db_work=pass 명시적으로 표시할 경우 디비 작업 없음.
                        // TODO 메시지가 없어서 추가
                        responseModel = ResponseModel.Message("1", "요청하신 파일이 업로드되었습니다.");
                    }
                    else
                    {
                        var dataSet = await Attachment_CRDAsync(CRUD.C, model, new FileModel
                        {
                            Name = attachment_filename,
                            Format = attachment_fileformat,
                            Size = attachment_filesize,
                        });

                        responseModel = responsePreprocessManager.ProcessDataSet(dataSet);
                    }
                }
            }
            catch (ServiceException ex)
            {
                responseModel = ResponseModel.ErrorMessage(ex);
            }
            catch (Exception ex)
            {
                responseModel = ResponseModel.Message("100", "파일업로드 에러가 발생했습니다.");
                //responseModel = ResponseModel.ErrorMessage(ex);
            }

            return responseModel;
        }

        public async Task SaveRemoteFile(string source, string destination)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                var response= await client.GetAsync(source);
                if (!response.IsSuccessStatusCode || response.Content == null)
                {
                    throw new ServiceException($"첨부파일을 원격서버에서 로컬로 복사해 오던 중 오류발생하였습니다. (원격 : {source}, 로컬 : {destination})");
                }

                using (var sourceStream = await response.Content.ReadAsStreamAsync())
                {
                    using(var destinationStream = new FileStream(destination,  FileMode.OpenOrCreate,  FileAccess.Write, FileShare.Read))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                        await destinationStream.FlushAsync();
                        destinationStream.Close();
                    }

                    sourceStream.Close();
                }
            }
        }

        public async Task<ResponseModel> DownloadCsv(RequestModel model)
        {
            string out_msg = string.Empty;

            string proc_name = databaseManager.GetProcedureName(model);

            var sqlparams = model.Data.ToDictionary(item => item.Key, item => (object)item.Value);
            DataSet ds = null;

            try
            {
                ds = await databaseManager.ExecuteQueryAsync(proc_name, sqlparams);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"{nameof(DownloadCsv)}", ex);
            }

            if (ds == null || ds.Tables.Count == 0)
            {
                throw new ServiceException($"{nameof(DownloadCsv)} 리턴 데이터에 오류가 있습니다.");
            }

            return this.MakeCSVFile(ds);
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

        private FileResponseModel MakeCSVFile(DataSet ds)
        {
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

            byte[] bom = { 0xEF, 0xBB, 0xBF };//EF BB BF; utf-8 with BOM

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

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] combine = new byte[bom.Length + buffer.Length];

            System.Buffer.BlockCopy(bom, 0, combine, 0, bom.Length);
            System.Buffer.BlockCopy(buffer, 0, combine, bom.Length, buffer.Length);

            return new FileResponseModel
            {
                FileName = Uri.EscapeUriString(fileName),
                ContentType = "text/csv; charset=utf-8",
                ContentEncoding = Encoding.UTF8,
                Content = combine,
            };
        }

        private static void AddComma(string value, StringBuilder stringBuilder)
        {
            stringBuilder.Append(value.Replace(',', ' '));
            stringBuilder.Append(", ");
        }

        /// <summary>
        /// 파일 정보를 입출력합니다.
        /// </summary>
        /// <param name="crd">입출력</param>
        /// <param name="requestModel"></param>
        /// <param name="fileModel"></param>
        /// <returns></returns>
        private async Task<DataSet> Attachment_CRDAsync(CRUD crd, RequestModel requestModel, FileModel fileModel)
        {
            string proc_name = Constants.ATTACHMENT_R_value;
            switch (crd)
            {
                case CRUD.C:
                case CRUD.U:
                case CRUD.D:
                    proc_name = Constants.ATTACHMENT_CUD_value;
                    break;
                case CRUD.R:
                    proc_name = Constants.ATTACHMENT_R_value;
                    break;
            }

            if (!String.IsNullOrEmpty(appOptions.DbSchema))
            {
                proc_name = $"{appOptions.DbSchema}.{proc_name}";
            }

            var parameters = new Dictionary<string, object>();
            parameters.Add(Constants.GUBUN_KEY_STRING, crd.ToString());
            parameters.Add(Constants.OPERATOR_IP_GUBUN, httpContextManager.GetRemoteIpAddress());
            parameters.Add(Constants.OPERATOR_KEY_GUBUN, requestModel.GetValue(Constants.OPERATOR_KEY_GUBUN));

            bool include_organization_key = Constants.INCLUDE_ORGANIZATION_KEY;
            if (include_organization_key)
            {
                parameters.Add(Constants.ORGANIZATION_KEY_GUBUN, requestModel.GetValue(Constants.ORGANIZATION_KEY_GUBUN));
            }

            switch (crd)
            {
                case CRUD.R:
                case CRUD.D:
                    parameters.Add(Constants.ATTACHMENT_KEY_key, requestModel.GetValue(Constants.ATTACHMENT_KEY_key));
                    break;
                case CRUD.C:
                    parameters.Add(Constants.ATTACHMENT_GUBUN_key, requestModel.GetValue(Constants.ATTACHMENT_GUBUN_key, "etc"));
                    parameters.Add(Constants.ATTACHMENT_DETAIL_CODE_key, requestModel.GetValue(Constants.ATTACHMENT_DETAIL_CODE_key));
                    parameters.Add(Constants.ATTACHMENT_FILENAME_key, fileModel.Name);
                    parameters.Add(Constants.ATTACHMENT_FILEFORMAT_key, fileModel.Format);
                    parameters.Add(Constants.ATTACHMENT_FILESIZE_key, fileModel.Size);
                    break;
            }

            DataSet dataSet = await databaseManager.ExecuteQueryAsync(proc_name, parameters);

            return dataSet;
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

        private string GetATTACHMENT_FILE_BASIC_URL(RequestModel model)
        {
            string attachmentFileBaicUrl = String.Empty;

            var attachmentPathKey = $"{GetATTACHMENT_TYPE(model)}_path";

            if (!appOptions.Properties.TryGetValue($"App:{attachmentPathKey}", out attachmentFileBaicUrl))
            {
                attachmentFileBaicUrl = String.Empty;
            }

            return attachmentFileBaicUrl;
        }

        private string GetATTACHMENT_TYPE(RequestModel model)
        {
            return model.GetValue(Constants.ATTACHMENT_GUBUN_key, "etc");
        }

        private string ReturnDirectoryPath(string header, string attachment_gubun, string attachment_detail_code)
        {
            //string returnPath = ConfigurationManager.AppSettings[attachment_gubun];
            string returnPath = appOptions[$"App:{attachment_gubun}"];
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
        private readonly IHttpContextManager httpContextManager;
        private readonly IResponsePreprocessManager responsePreprocessManager;
        private readonly AppOptions appOptions;
    }
}
