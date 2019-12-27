using CryptSharp;
using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.Api
{
    public class UserApiModule : NancyModule
    {
        public UserApiModule() : base("/api/user")
        {
            Get["/"] = x =>
            {
                return HttpStatusCode.OK;
            };
            Post["/{user}"] = x =>
            {
                return HttpStatusCode.Created;
            };
            Patch["/{user}"] = x =>
            {
                string strPassword = "";
                using (var req = Request.Body)
                {
                    using (var reader = new StreamReader(req))
                    {
                        strPassword = reader.ReadToEnd();
                    }
                }
                string userPasswordHash = Crypter.MD5.Crypt(strPassword, new CrypterOptions { { CrypterOption.Variant, MD5CrypterVariant.Apache } });

                return HttpStatusCode.OK;
            };
        }
    }
}
