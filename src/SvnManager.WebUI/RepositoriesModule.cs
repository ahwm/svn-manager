using Nancy;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SvnManager.WebUI
{
    public class RepositoriesModule : NancyModule
    {
        private static string RepoPath => ConfigurationManager.AppSettings["Manager.RepoPath"];
        private static string SvnLocation => ConfigurationManager.AppSettings["SvnLocation"];

        public RepositoriesModule() : base("/api/repositories")
        {
            Get["/"] = x =>
            {
                List<string> repos = new DirectoryInfo(RepoPath).GetDirectories().Select(r => r.Name).ToList();

                return Response.AsJson(repos).WithStatusCode(HttpStatusCode.OK);
            };
            Post["/{name}"] = x =>
            {
                using (Process p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo($@"{SvnLocation}\svnadmin", $@"create {x.name}")
                    {
                        WorkingDirectory = RepoPath,
                        RedirectStandardError = true
                    };
                    p.Start();
                    p.WaitForExit();
                }

                // This gives all users read/write access
                string svnAuthz = File.ReadAllText($@"C:\Repositories\{x.name}\conf\SvnAuthz.ini");
                svnAuthz += "\r\n\r\n[/]\r\n*=rw";
                File.WriteAllText($@"C:\Repositories\{x.name}\conf\SvnAuthz.ini", svnAuthz);

                // pre-commit hook to require commit messages
                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.Api.SvnHooks.pre-commit.cmd"))
                {
                    using (var file = new FileStream($@"C:\Repositories\{x.name}\hooks\pre-commit.cmd", FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }

                // pre-revprop-change hook to allow for using svnsync
                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.Api.SvnHooks.pre-revprop-change.cmd"))
                {
                    using (var file = new FileStream($@"C:\Repositories\{x.name}\hooks\pre-revprop-change.cmd", FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }

                return HttpStatusCode.Created;
            };
            //Delete["/{name}"] = x =>
            //{
            //    using (PowerShell ps = PowerShell.Create())
            //    {
            //        ps.AddCommand("Remove-SvnRepository");
            //        ps.AddArgument(x.name);
            //        ps.Invoke();
            //    }

            //    return HttpStatusCode.OK;
            //};
        }
    }
}
