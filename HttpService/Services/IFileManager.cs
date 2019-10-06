using HttpService.Models;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IFileManager
    {
        Task<ResponseModel> GetFileInfo(RequestModel model);

        Task<string> ReturnFileFullPath(RequestModel model);

        Task<ResponseModel> DownloadFile(RequestModel model, bool isDirectDownloadFile = false);

        Task<ResponseModel> DownloadCsv(RequestModel model);

        Task<ResponseModel> DeleteFile(RequestModel model);

        Task<ResponseModel> GetFileNameList(RequestModel model);

        Task<ResponseModel> FileRename(RequestModel model);

        Task<ResponseModel> UploadFile(RequestModel model);

        Task SaveRemoteFile(string source, string destination);
    }
}
