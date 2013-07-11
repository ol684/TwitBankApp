using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.Devices.Geolocation;

namespace TwitBankApp.Utility
{
    public class TwittingUtility
    {
        public static string GeneralTwit = "ihatetowait";

        public static string GenerateMessage(string TwitText, string Bank, string Inline, string Hours, string Minutes, string Open, string Closed, bool Angry, bool Chairs, bool USSR)
        {
            StringBuilder Twit = new StringBuilder(string.Format("#{0}", GeneralTwit));

            if (!String.IsNullOrWhiteSpace(Bank))
                Twit.AppendFormat(" {0}", Bank);

            int inlineCount = 0;
            if (!String.IsNullOrWhiteSpace(Inline) && int.TryParse(Inline, out inlineCount) && inlineCount > 0)
                Twit.AppendFormat(" #{0}inline", inlineCount);

            int hCount = 0;
            if (!String.IsNullOrWhiteSpace(Hours) && int.TryParse(Hours, out hCount) && hCount > 0)
                Twit.AppendFormat(" #{0}htowait", Hours);

            int mCount = 0;
            if (!String.IsNullOrWhiteSpace(Minutes) && int.TryParse(Minutes, out mCount) && mCount > 0)
                Twit.AppendFormat(" #{0}mtowait", Minutes);

            int availCount = 0;
            if (!String.IsNullOrWhiteSpace(Open) && int.TryParse(Open, out availCount) && availCount > 0)
                Twit.AppendFormat(" #{0}avail", availCount);

            int closedCount = 0;
            if (!String.IsNullOrWhiteSpace(Closed) && int.TryParse(Closed, out closedCount) && closedCount > 0)
                Twit.AppendFormat(" #{0}closed", closedCount);




            if (Angry)
                Twit.Append("  #angryoldlady");
            if (Chairs)
                Twit.Append("  #nochairs");
            if (USSR)
                Twit.Append("  #ussrstyle");

            Twit.Append(" " + TwitText);

            return Twit.ToString();
        }

        public static string GetBankInfo(string twit)
        {
            return twit.Replace("#", "")
                .Replace(TwittingUtility.GeneralTwit, "")
                .Replace("inline", " в очереди| ")
                .Replace("avail", " открыто |")
                .Replace("closed", " закрыто |")
                .Replace("htowait", " ч. ")
                .Replace("mtowait", "м. ");
        }

        public static bool SendMessage(TwitterContext twitterCtx, string Twit, Geoposition geo, Action<TwitterAsyncResponse<Status>> callback)
        {
            try
            {
                twitterCtx.UpdateStatus(Twit,
                       (decimal)geo.Coordinate.Latitude,
                       (decimal)geo.Coordinate.Longitude,
                       true,
                       callback);
                return true;
            }
            catch (Exception ee)
            {
                return false;
            }
        }

        public static bool SendMessage(TwitterContext twitterCtx, string Twit, Action<TwitterAsyncResponse<Status>> callback)
        {
            try
            {
                twitterCtx.UpdateStatus(Twit, callback);
                return true;
            }
            catch (Exception ee)
            {
                return false;
            }
        }


    }
}
