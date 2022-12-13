namespace LineNotifyHelper.Response
{
    public class GetStatusResponse : LineBaseResponseBody
    {
        public string targetType
        {
            get;
            set;
        }

        public string target
        {
            get;
            set;
        }
    }
}
