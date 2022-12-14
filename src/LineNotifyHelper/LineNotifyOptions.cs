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

        /// <summary>
        ///   LINE Bot api url, default value is https://notify-bot.line.me/
        /// </summary>
        public string BotBaseUrl
        {
            get;
            set;
        } = DEFAULT_BOT_URL;

        /// <summary>
        ///   LINE Notify api url, default value is https://notify-api.line.me/
        /// </summary>
        public string NotifyBaseUrl
        {
            get;
            set;
        } = DEFAULT_NOTIFY_URL;

        /// <summary>
        ///  LINE ClientId
        /// </summary>
        public string ClientId
        {
            get;
            set;
        }

        /// <summary>
        /// LINE ClinetSecret
        /// </summary>
        public string ClinetSecret
        {
            get;
            set;
        }

        /// <summary>
        /// LINE CallbackUrl, this one will be used in compose redirect url
        /// If you have multiple callback url, specify it directly in GenerateAuthorizeUri and GetAccessTokenAsync
        /// </summary>
        public string CallbackUrl
        {
            get;
            set;
        }

        /// <summary>
        /// HttpClient name, default value is ""
        /// </summary>
        public string NamedClient
        {
            get;
            set;
        } = string.Empty;
    }
}
