using Newtonsoft.Json;
using System.Net.Http;

namespace LineNotifyHelper.Response
{
    public static class WrapperReponseHelper
    {
        public static LineResponse<T> WrapResponse<T>(HttpResponseMessage response) where T : LineBaseResponseBody
        {
            LineResponse<T> wrappedResponse = new LineResponse<T>();

            wrappedResponse.StatusCode = response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                var jsonString = response.Content.ReadAsStringAsync().Result;
                /// stupid checkmarx
                wrappedResponse.ErrorBody = JsonConvert.DeserializeObject<LineErrorResponseBody>(jsonString);
            }
            else
            {
                var jsonString = response.Content.ReadAsStringAsync().Result;
                /// stupid checkmarx
                wrappedResponse.Body = JsonConvert.DeserializeObject<T>(jsonString);
            }

            return wrappedResponse;
        }
    }
}
