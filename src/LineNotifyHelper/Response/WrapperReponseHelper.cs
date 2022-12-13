using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                wrappedResponse.ErrorBody = JsonSerializer.Deserialize<LineErrorResponseBody>(jsonString);
            }
            else
            {
                var jsonString = response.Content.ReadAsStringAsync().Result;
                wrappedResponse.Body = JsonSerializer.Deserialize<T>(jsonString);
            }

            return wrappedResponse;
        }
    }
}
