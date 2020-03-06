using CryptSharp;
using Nancy;
using SvnManager.WebUI.Code;
using System;
using System.Collections.Generic;
using System.IO;

namespace SvnManager.WebUI
{
    public class UserModule : SvnBaseModule
    {
        public UserModule() : base("/user")
        {
            Get["/add"] = x =>
            {
                var users = GetUsers();

                return View["AddUser", new { IsError = false, ErrorMessage = "", UserName = "", UserPassword = "", IsSuccess = false, Users = users }];
            };
            Post["/add"] = x =>
            {
                var name = Request.Form.userName.Value;
                var password = Request.Form.userPass.Value;

                var users = GetUsers();

                try
                {
                    string strPassword = password;
                    string userPasswordHash = Crypter.MD5.Crypt(strPassword, new CrypterOptions { { CrypterOption.Variant, MD5CrypterVariant.Apache } });

                    var pwds = File.ReadAllText($@"{RepoPath.TrimEnd('\\')}\htpasswd");
                    pwds += $"{name}:{userPasswordHash}{Environment.NewLine}";
                    File.WriteAllText($@"{RepoPath.TrimEnd('\\')}\htpasswd", pwds);
                }
                catch (Exception ex)
                {
                    return View["AddUser", new { IsError = true, ErrorMessage = ex.Message, UserName = name, UserPassword = password, IsSuccess = false, Users = users }];
                }

                users = GetUsers();

                return View["AddUser", new { IsError = false, ErrorMessage = "", UserName = "", UserPassword = "", IsSuccess = true, Users = users }];
            };
        }

        private List<dynamic> GetUsers()
        {
            var users = new List<dynamic>();

            if (!File.Exists($@"{RepoPath.TrimEnd('\\')}\htpasswd"))
            {
                File.CreateText($@"{RepoPath.TrimEnd('\\')}\htpasswd");
                return users;
            }

            string line;
            using (StreamReader fs = new StreamReader($@"{RepoPath.TrimEnd('\\')}\htpasswd"))
            {
                while ((line = fs.ReadLine()) != null)
                {
                    users.Add(new { User = line.Split(':')[0] });
                }
            }

            return users;
        }
    }
}
