using Nancy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SvnManager.WebUI
{
    public class MainModule : NancyModule
    {
        private static string RepoPath => ConfigurationManager.AppSettings["Manager.RepoPath"];
        private static string SvnLocation => ConfigurationManager.AppSettings["SvnLocation"];
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
                p.StartInfo = new ProcessStartInfo($@"{SvnLocation}\svnadmin", $@"create {name}")
                {
                    WorkingDirectory = RepoPath,
                    RedirectStandardError = true
                };
                p.Start();
                p.WaitForExit();
            }

            // This gives all users read/write access
            string svnAuthz = File.ReadAllText($@"C:\Repositories\{name}\conf\SvnAuthz.ini");
            svnAuthz += "\r\n\r\n[/]\r\n*=rw";
            File.WriteAllText($@"C:\Repositories\{name}\conf\SvnAuthz.ini", svnAuthz);

            // pre-commit hook to require commit messages
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.Api.SvnHooks.pre-commit.cmd"))
            {
                using (var file = new FileStream($@"C:\Repositories\{name}\hooks\pre-commit.cmd", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }

            // pre-revprop-change hook to allow for using svnsync
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.Api.SvnHooks.pre-revprop-change.cmd"))
            {
                using (var file = new FileStream($@"C:\Repositories\{name}\hooks\pre-revprop-change.cmd", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
    }
}
