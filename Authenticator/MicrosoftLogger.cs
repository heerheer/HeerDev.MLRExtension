using HeerDev.MLRExtension.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Text;
using System.Linq;
using HeerDev.MLRExtension.Authenticator.Model;
using System.Diagnostics;

namespace HeerDev.MLRExtension.Authenticator
{
    public class MicrosoftLogger
    {

        private const string LogName = "";
        /// <summary>
        /// ��һ�����봰�ڣ��ȴ����벢����Code,��������˿�ֵ�����û�����ʧ�ܻ�ر��˴���
        /// </summary>
        /// <returns></returns>
        public static string OpenLoginView()
        {
            var view = new View.MicrosoftLoginView();
            try
            {
                return view.ShowDialog() is true ? view.Code : "";
            }
            catch (Exception)
            {

                return "";
            }
        }

        /// <summary>
        /// �õ�OAuth Token��Refresh Token ��Ȼ�һ���Ը��ֱ����refreshtoken
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static async Task<string[]> GetMicrosoftOAuthToken(string code)
        {
            string[] resultstrs = new string[2];
            var url = "https://login.live.com/oauth20_token.srf";
            Dictionary<string, string> dic = new()
            {
                { "client_id", "00000000402b5328" },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", "https://login.live.com/oauth20_desktop.srf" },
                { "scope", "service::user.auth.xboxlive.com::MBI_SSL" }
            };
            var response = await Http.Get(url, dic);
            var jobj = JsonDocument.Parse(response);
            resultstrs[0] = jobj.RootElement.GetProperty("access_token").GetString() ?? throw new Exception("��ȡMS OAuth Tokenʧ��");
            resultstrs[1] = jobj.RootElement.GetProperty("refresh_token").GetString() ?? throw new Exception("��ȡRefresh Tokenʧ��");
            return resultstrs;

        }

        /// <summary>
        /// �õ�MSOAuthToken���µ�RefreshToken
        /// </summary>
        /// <param name="refresh_token"></param>
        /// <returns>[0]MSOAuthToken [1]new Refresh Token</returns>
        private static async Task<string[]> RefreshMicrosoftOAuthToken(string refresh_token)
        {
            string[] resultstrs = new string[2];
            var url = "https://login.live.com/oauth20_token.srf";
            Dictionary<string, string> dic = new()
            {
                { "client_id", "00000000402b5328" },
                { "refresh_token", refresh_token },
                { "grant_type", "refresh_token" },
                { "redirect_uri", "https://login.live.com/oauth20_desktop.srf" },
                { "scope", "service::user.auth.xboxlive.com::MBI_SSL" }
            };
            var response = await Http.Get(url, dic);
            var jobj = JsonDocument.Parse(response);

            resultstrs[0] = jobj.RootElement.GetProperty("access_token").GetString() ?? throw new Exception("��ȡMS OAuth Tokenʧ��");
            resultstrs[1] = jobj.RootElement.GetProperty("refresh_token").GetString() ?? throw new Exception("��ȡRefresh Tokenʧ��");
            return resultstrs;
        }

        /// <summary>
        /// [0]XBL Token [1]Uhash
        /// </summary>
        /// <param name="msOAuthToken"></param>
        /// <returns></returns>
        private static async Task<string[]> GetXBLTokenAndUhash(string msOAuthToken)
        {
            string[] resultstrs = new string[2];

            var url = "https://user.auth.xboxlive.com/user/authenticate";
            string json = "";
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteString("RelyingParty", "http://auth.xboxlive.com");
                    writer.WriteString("TokenType", "JWT");
                    //Properties����
                    writer.WriteStartObject("Properties");
                    writer.WriteString("AuthMethod", "RPS");
                    writer.WriteString("SiteName", "user.auth.xboxlive.com");
                    writer.WriteString("RpsTicket",msOAuthToken);
                    writer.WriteEndObject();

                    writer.WriteEndObject();
                }

                json = Encoding.UTF8.GetString(stream.ToArray());
            }
            var response = await Http.Post(url, json);
            if (string.IsNullOrEmpty(response))
                throw new Exception("XBL Token��ȡʧ��");
            var jobj = JsonDocument.Parse(response);

            resultstrs[0] = jobj.RootElement.GetProperty("Token").GetString() ?? throw new Exception("��ȡXBL Tokenʧ��");
            resultstrs[1] = jobj.RootElement.GetProperty("DisplayClaims").GetProperty("xui").EnumerateArray().ToList().First().GetProperty("uhs").GetString() ?? throw new Exception("��ȡUser Hashʧ��");
            return resultstrs;
        }

        /// <summary>
        /// [0]XSTS Token [1]Uhash
        /// </summary>
        /// <param name="xbl_token"></param>
        /// <returns></returns>
        private static async Task<string[]> GetXSTSToken(string xbl_token)
        {
            string[] resultstrs = new string[2];

            var url = "https://xsts.auth.xboxlive.com/xsts/authorize";
            string json = "";
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteString("RelyingParty", "rp://api.minecraftservices.com/");
                    writer.WriteString("TokenType", "JWT");
                    //Properties����
                    writer.WriteStartObject("Properties");
                    writer.WriteString("SandboxId", "RETAIL");
                    //write xbl_token
                    writer.WriteStartArray("UserTokens");
                    writer.WriteStringValue(xbl_token);
                    writer.WriteEndArray();

                    writer.WriteEndObject();

                    writer.WriteEndObject();
                }

                json = Encoding.UTF8.GetString(stream.ToArray());

            }
            var response = await Http.Post(url, json);
            if (string.IsNullOrEmpty(response))
                throw new Exception("XSTS Token��ȡʧ��");
            var jobj = JsonDocument.Parse(response);

            resultstrs[0] = jobj.RootElement.GetProperty("Token").GetString() ?? throw new Exception("��ȡXSTS Tokenʧ��");
            resultstrs[1] = jobj.RootElement.GetProperty("DisplayClaims").GetProperty("xui").EnumerateArray().ToList().First().GetProperty("uhs").GetString() ?? throw new Exception("��ȡUser Hashʧ��");
            return resultstrs;
        }

        /// <summary>
        /// �õ�accesstoken���Ѿ����������ˣ����UUID
        /// </summary>
        /// <param name="uhs"></param>
        /// <param name="xstsToken"></param>
        /// <returns></returns>
        private async static Task<string> GetMinecraftAccessToken(string uhs, string xstsToken)
        {
            var url = "https://api.minecraftservices.com/authentication/login_with_xbox";
            string json = "";
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteString("identityToken", $"XBL3.0 x={uhs};{xstsToken}");
                    writer.WriteEndObject();
                }

                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            var response = await Http.Post(url, json);
            var jobj = JsonDocument.Parse(response);

            return jobj.RootElement.GetProperty("access_token").GetString() ?? throw new Exception("��ȡMC Access Tokenʧ��");
        }

        /// <summary>
        /// 0 UUID 1 Name
        /// </summary>
        /// <param name="mc_accessToken"></param>
        /// <returns></returns>
        private async static Task<string[]> GetMinecraftProfile(string mc_accessToken)
        {
            string[] resultstrs = new string[2];

            var url = "https://api.minecraftservices.com/minecraft/profile";
            var response = await Http.Get(url, new KeyValuePair<string, string>("Authorization", $"Bearer {mc_accessToken}"));
            var jobj = JsonDocument.Parse(response);

            resultstrs[0] = jobj.RootElement.GetProperty("id").GetString() ?? throw new Exception("UUID��ȡʧ��");
            resultstrs[1] = jobj.RootElement.GetProperty("name").GetString() ?? throw new Exception("��ɫName��ȡʧ��");
            return resultstrs;
        }

        /// <summary>
        /// �򿪵���ҳ�棬���õ����Դ����RefreshToken
        /// </summary>
        /// <returns></returns>
        public async static Task<string> FirstLogin()
        {
            var code = OpenLoginView();
            var oauth_p = await GetMicrosoftOAuthToken(code);
            return oauth_p[1];

        }

        public static async Task<MSLoginResult> Login(string refresh_token)
        {
            MSLoginResult result = new();
            var oauth_p = await RefreshMicrosoftOAuthToken(refresh_token);
            result.RefreshAccessToken = oauth_p[1];

            var xbl_p = await GetXBLTokenAndUhash(oauth_p[0]);

            var xsts_p = await GetXSTSToken(xbl_p[0]);

            var mc_accesstoken = "";
            try
            {
                mc_accesstoken = await GetMinecraftAccessToken(xsts_p[1], xsts_p[0]);

            }
            catch (Exception)
            {

                throw new Exception("��ȡMCAccessTokenʧ��");
            }

            try
            {
                var mc_profile = await GetMinecraftProfile(mc_accesstoken);

                result.AccessToken = mc_accesstoken;
                result.UUID = mc_profile[0];
                result.Name = mc_profile[1];

                return result;

            }
            catch (Exception)
            {

                throw new Exception("�˻���û��Minecraft~");
            }
        }
    }
}
