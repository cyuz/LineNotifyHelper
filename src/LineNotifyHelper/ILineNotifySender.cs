using LineNotifyHelper.Response;
using System.Threading.Tasks;

namespace LineNotifyHelper
{
    public interface ILineNotifySender
    {
        string GenerateAuthorizeUrl(string state, bool via_post = true, string redirectUrl = null);
        Task<LineResponse<GetAccessTokenResponse>> GetAccessTokenAsync(string code, string redirectUrl = null);
        LineResponse<GetStatusResponse> GetStatus(string accessToken);
        Task<LineResponse<LineBaseResponseBody>> NotifyMessageAsync(string accessToken, string message, string stickerPackageId = null, string stickerId = null);
        Task<LineResponse<LineBaseResponseBody>> RevokeAuthorizationAsync(string accessToken);
    }
}