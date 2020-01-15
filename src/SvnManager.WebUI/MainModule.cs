using Nancy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SvnManager.WebUI
{
    public class MainModule : NancyModule
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
            List<string> repos = new List<string>();
            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddCommand("Get-SvnRepository");
                    Collection<PSObject> output = ps.Invoke();
                    foreach (var o in output)
                    {
                        repos.Add((string)o.Members["Name"].Value);
                    }
                }
            }
            catch { }

            return repos;
        }
        private void CreateRepo(string name)
        {
            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddCommand("New-SvnRepository");
                ps.AddArgument(name);
                ps.Invoke();
            }

            // This gives all users read/write access
            string svnAuthz = File.ReadAllText($@"C:\Repositories\{name}\conf\VisualSVN-SvnAuthz.ini");
            svnAuthz += "\r\n\r\n[/]\r\n*=rw";
            File.WriteAllText($@"C:\Repositories\{name}\conf\VisualSVN-SvnAuthz.ini", svnAuthz);

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
