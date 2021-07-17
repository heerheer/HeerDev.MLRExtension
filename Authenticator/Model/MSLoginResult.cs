using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeerDev.MLRExtension.Authenticator.Model
{
    public class MSLoginResult
    {
        public MSLoginResult()
        {
            Name = "";
            UUID = "";
            AccessToken = "";
            RefreshAccessToken = "";
        }

        public MSLoginResult(string name, string uUID, string accessToken, string refreshAccessToken)
        {
            Name = name;
            UUID = uUID;
            AccessToken = accessToken;
            RefreshAccessToken = refreshAccessToken;
        }

        public string Name { get; set; }
        public string UUID  { get; set; }
        public string AccessToken { get; set; }
        public string RefreshAccessToken { get; set; }
    }
}
