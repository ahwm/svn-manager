using Nancy;
using SvnManager.WebUI.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SvnManager.WebUI
{
    public class MainModule : SvnBaseModule
    {
        public MainModule()
        {
            Get["/"] = x =>
            {
                var repos = GetRepos();
                return View["AddRepo", new { IsError = false, ErrorMessage = "", IsSuccess = false, RepoName = "", Repositories = repos }];
            };
            Post["/"] = x =>
            {
                var name = Request.Form.repoName.Value;
                name = name.ToLower();
                name = Regex.Replace(name, "[^a-zA-Z0-9- ]", "-");
                while (name.Contains("--"))
                    name = name.Replace("--", "-");

                bool isError = false, isSuccess = false;
                string errorMessage = "";
                try
                {
                    CreateRepo(name);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                        ex = ex.InnerException;

                    isError = true;
                    errorMessage = ex.Message;
                }

                var repos = GetRepos();
                return View["AddRepo", new { IsError = isError, ErrorMessage = errorMessage, IsSuccess = isSuccess, RepoName = name, Repositories = repos }];
            };
        }
        private List<string> GetRepos()
        {
            List<string> repos = new DirectoryInfo(RepoPath).GetDirectories().Select(r => r.Name).ToList();

            return repos;
        }
        private void CreateRepo(string name)
        {
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo($@"{SvnLocation}\svnadmin", $@"create {Path.Combine(RepoPath, name)}")
                {
                    UseShellExecute = false,
                    WorkingDirectory = SvnLocation,
                    RedirectStandardError = true
                };
                p.Start();
                p.WaitForExit();
            }

            // This gives all users read/write access
            string svnAuthz = File.ReadAllText($@"{RepoPath}\{name}\conf\authz");
            svnAuthz += "\r\n\r\n[/]\r\n*=rw";
            File.WriteAllText($@"{RepoPath}\{name}\conf\authz", svnAuthz);

            // pre-commit hook to require commit messages
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.WebUI.SvnHooks.pre-commit.cmd"))
            {
                using (var file = new FileStream($@"{RepoPath}\{name}\hooks\pre-commit.cmd", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }

            // pre-revprop-change hook to allow for using svnsync
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.WebUI.SvnHooks.pre-revprop-change.cmd"))
            {
                using (var file = new FileStream($@"{RepoPath}\{name}\hooks\pre-revprop-change.cmd", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
    }
}
