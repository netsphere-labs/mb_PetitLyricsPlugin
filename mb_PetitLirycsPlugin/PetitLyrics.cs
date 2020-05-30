using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MusicBeePlugin
{
    public class PetitLyrics
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        public static string FetchLyrics(string trackTitle, string artist, string album)
        {
            var client = new WebClientEx() { Encoding = Encoding };

            trackTitle = HttpUtility.UrlEncode(trackTitle);
            artist = HttpUtility.UrlEncode(artist);
            album = HttpUtility.UrlEncode(album ?? "");

            // 検索結果ページ
            string lyricId;
            {
                string searchPage = client.DownloadString($"https://petitlyrics.com/search_lyrics?title={trackTitle}&artist={artist}&album={album}");

                int startIndex = searchPage.IndexOf("id=\"lyrics_list\"");
                int length = searchPage.Substring(startIndex).IndexOf("id=\"lyrics_list_pager\"");

                var ms = Regex.Matches(searchPage.Substring(startIndex, length), @"href=""/lyrics/(?<lyricId>\d+)");
                if (ms.Count == 0)
                {
                    return null;
                }

                lyricId = ms.Cast<Match>().First().Groups["lyricId"].Value.Trim();
            }

            // 歌詞ページ（別に歌詞取得には必要ないが、向こうのサーバーを欺く為）
            string lyricPage = $"https://petitlyrics.com/lyrics/{lyricId}";
            client.DownloadData(lyricPage);

            // X-CSRF-Token
            string token;
            {
                string tokenScript = client.DownloadString("https://petitlyrics.com/lib/pl-lib.js");

                var m = Regex.Match(tokenScript, @"'X-CSRF-Token', '(?<token>[^']+)");
                if (m.Success)
                {
                    token = m.Groups["token"].Value;
                }
                else
                {
                    return null;
                }
            }

            client.Headers.Clear();
            client.Headers[HttpRequestHeader.Accept] = "*/*";
            client.Headers[HttpRequestHeader.AcceptLanguage] = "ja";
            client.Headers[HttpRequestHeader.Referer] = lyricPage;
            client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.3; Win64; x64; Trident/7.0; rv:11.0) like Gecko";
            client.Headers[HttpRequestHeader.Pragma] = "no-cache";
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
            client.Headers.Add("X-Requested-With", "XMLHttpRequest");
            client.Headers.Add("X-CSRF-Token", token);

            // 歌詞取得API
            string json = client.UploadString("https://petitlyrics.com/com/get_lyrics.ajax", $"lyrics_id={lyricId}");

            // パース
            string[] lines = Regex.Matches(json, @"{""lyrics"":""(?<base64>[^""]*)""}")
                .Cast<Match>()
                .Select(x => x.Groups["base64"].Value.Replace(@"\/", "/"))
                .Select(Convert.FromBase64String)
                .Select(line => Encoding.GetString(line).TrimEnd(new[] { '\r', '\n' }))
                .ToArray();
            return string.Join("\r\n", lines);
        }
    }
}