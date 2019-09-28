using HttpService.Models;

namespace HttpService.Serializer
{
    public interface ISerializer
    {
        string Serialize(ResponseModel model);
    }
}
