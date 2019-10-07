using HttpService.Models;
using System.Threading.Tasks;

namespace HttpService.Services
{
    public interface IEmailManager
    {
        Task<ResponseModel> Send(RequestModel model);
    }
}
