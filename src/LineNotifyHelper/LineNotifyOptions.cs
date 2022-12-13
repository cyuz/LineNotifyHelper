using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineNotifyHelper
{
    public class LineNotifyOptions
    {
        public const string DEFAULT_BOT_URL = "https://notify-bot.line.me/";
        public const string DEFAULT_NOTIFY_URL = "https://notify-api.line.me/";

        public string BotBaseUrl
        {
            get;
            set;
        } = DEFAULT_BOT_URL;

        public string NotifyBaseUrl
        {
            get;
            set;
        } = DEFAULT_NOTIFY_URL;

        public string ClientId
        {
            get;
            set;
        }

        public string ClinetSecret
        {
            get;
            set;
        }

        public string CallbackUrl
        {
            get;
            set;
        }
    }
}
