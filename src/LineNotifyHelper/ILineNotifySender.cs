using LineNotifyHelper.Response;
using System.Threading.Tasks;

namespace LineNotifyHelper
{
    public interface ILineNotifySender
    {
        string GenerateAuthorizeUri(string state, bool via_post = true, string redirectUri = null);
        Task<LineResponse<GetAccessTokenResponse>> GetAccessTokenAsync(string authorizationCode, string redirectUri = null);
        LineResponse<GetStatusResponse> GetStatus(string accessToken);
        Task<LineResponse<LineBaseResponseBody>> NotifyMessageAsync(string accessToken, string message, string stickerPackageId = null, string stickerId = null);
        Task<LineResponse<LineBaseResponseBody>> RevokeAuthorizationAsync(string accessToken);
    }
}