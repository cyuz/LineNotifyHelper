using System.Net;

namespace LineNotifyHelper.Response
{
    public class LineResponse<T> where T : LineBaseResponseBody
    {
        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        public T Body
        {
            get;
            set;
        }

        public LineErrorResponseBody ErrorBody
        {
            get;
            set;
        }

        public bool Success
        {
            get
            {
                return this.StatusCode == HttpStatusCode.OK;
            }
        }
    }
}
