using LineNotifyHelper.Response;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace LineNotifyHelper
{
    public class LineNotifySender : ILineNotifySender
    {
        private const string AuthorizeAPI = "oauth/authorize";
        private const string OauthAPI = "oauth/token";
        private const string NotifyAPI = "api/notify";
        private const string RevokeAPI = "api/revoke";
        private const string StatusAPI = "api/status";

        private readonly LineNotifyOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;

        public LineNotifySender(IOptions<LineNotifyOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        ///   Generate the authorize url for redirect
        /// </summary>
        /// <param name="state">random generated state</param>
        /// <param name="via_post">return from POST or GET</param>
        /// <param name="callbackUrl">override callbackUrl in options</param>
        /// <returns></returns>
        public string GenerateAuthorizeUrl(string state, bool via_post = true, string callbackUrl = null)
        {
            Uri baseUri = new Uri(_options.BotBaseUrl);
            Uri uri = new Uri(baseUri, AuthorizeAPI);

            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["response_type"] = "code";
            query["client_id"] = _options.ClientId;
            query["redirect_uri"] = callbackUrl ?? _options.CallbackUrl;
            query["scope"] = "notify";
            query["state"] = state;
            if (via_post)
            {
                query["response_mode"] = "form_post";
            }
            builder.Query = query.ToString();
            string url = builder.ToString();

            return url;
        }

        /// <summary>
        ///  after redirect from line, get access token from authorization Code
        ///  one authorization Code only can generate access token once
        ///  just save the access token for future use
        /// </summary>
        /// <param name="code">the authorization code of this user</param>
        /// <param name="callbackUrl">override callbackUrl in options</param>
        /// <returns></returns>
        public async Task<LineResponse<GetAccessTokenResponse>> GetAccessTokenAsync(string code, string callbackUrl = null)
        {
            using HttpClient client = _httpClientFactory.CreateClient(_options.NamedClient);
            client.BaseAddress = new Uri(_options.BotBaseUrl);

            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", "authorization_code");
            dict.Add("code", code);
            dict.Add("redirect_uri", callbackUrl ?? _options.CallbackUrl);
            dict.Add("client_id", _options.ClientId);
            dict.Add("client_secret", _options.ClinetSecret);
            var content = new FormUrlEncodedContent(dict);

            using var response = await client.PostAsync(OauthAPI, content);

            LineResponse<GetAccessTokenResponse> wrappedResponse = WrapperReponseHelper.WrapResponse<GetAccessTokenResponse>(response);

            return wrappedResponse;
        }


        /// <summary>
        ///   Send message to user associate with access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="message"></param>
        /// <param name="stickerPackageId">optional</param>
        /// <param name="stickerId">optional</param>
        /// <returns></returns>
        public async Task<LineResponse<LineBaseResponseBody>> NotifyMessageAsync(string accessToken, string message, string stickerPackageId = null, string stickerId = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message", "message is required");
            }

            using HttpClient client = _httpClientFactory.CreateClient(_options.NamedClient);
            /// stupid checkmarx
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", accessToken);            
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            client.BaseAddress = new Uri(_options.NotifyBaseUrl);

            var dict = new Dictionary<string, string>();
            dict.Add("message", message);
            if (!string.IsNullOrEmpty(stickerPackageId))
            {
                dict.Add("stickerPackageId", stickerPackageId);
            }
            if (!string.IsNullOrEmpty(stickerId))
            {
                dict.Add("stickerId", stickerId);
            }
            var content = new FormUrlEncodedContent(dict);

            using var response = await client.PostAsync(NotifyAPI, content);

            LineResponse<LineBaseResponseBody> wrappedResponse = WrapperReponseHelper.WrapResponse<LineBaseResponseBody>(response);

            return wrappedResponse;
        }


        /// <summary>
        ///   Revoke a registered accessToken
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<LineResponse<LineBaseResponseBody>> RevokeAuthorizationAsync(string accessToken)
        {
            using HttpClient client = _httpClientFactory.CreateClient(_options.NamedClient);
            client.BaseAddress = new Uri(_options.NotifyBaseUrl);

            /// stupid checkmarx
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", accessToken);            
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var dict = new Dictionary<string, string>();
            var content = new FormUrlEncodedContent(dict);

            using var response = await client.PostAsync(RevokeAPI, content);

            LineResponse<LineBaseResponseBody> wrappedResponse = WrapperReponseHelper.WrapResponse<LineBaseResponseBody>(response);

            return wrappedResponse;
        }

        public LineResponse<GetStatusResponse> GetStatus(string accessToken)
        {
            using HttpClient client = _httpClientFactory.CreateClient(_options.NamedClient);
            client.BaseAddress = new Uri(_options.NotifyBaseUrl);

            /// stupid checkmarx
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", accessToken);            
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            using var response = client.GetAsync(StatusAPI).Result;

            LineResponse<GetStatusResponse> wrappedResponse = WrapperReponseHelper.WrapResponse<GetStatusResponse>(response);

            return wrappedResponse;
        }
    }
}
