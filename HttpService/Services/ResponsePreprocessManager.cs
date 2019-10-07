using HttpService.Models;
using System;
using System.Data;
using System.IO;

namespace HttpService.Services
{
    public class ResponsePreprocessManager : IResponsePreprocessManager
    {
        public ResponseModel ProcessDataSet(DataSet dataSet)
        {
            ResponseModel result = new ResponseModel();

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                result.Values.Namespace = dataSet.Namespace;

                result.Values.Add(dataSet.Tables[0].TableName, dataSet.Tables[0].ToDictionaryEnumerable());
            }

            return result;
        }

        public ResponseModel ProcessException(Exception ex)
        {
            if(ex is ServiceException)
            {
                return ResponseModel.ErrorMessage(ex);
            }

            // TODO 예외 메시지를 바로 출력할 것인가?
            return ResponseModel.ErrorMessage(ex);
        }

        public ResponseModel ProcessFile(string file)
        {
            if (String.IsNullOrEmpty(file) || !File.Exists(file))
            {
                var message = "해당 파일이 존재하지 않습니다.";
                return ResponseModel.ErrorMessage(new FileNotFoundException(fileName: file, message: message), message);
            }

            FileInfo fileInfo = new FileInfo(file);

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
    }
}
