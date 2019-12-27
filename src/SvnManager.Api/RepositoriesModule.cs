using Nancy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.Api
{
    public class RepositoriesModule : NancyModule
    {
        public RepositoriesModule() : base("/api/repositories")
        {
            Get["/"] = x =>
            {
                List<string> repos = new List<string>();
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddCommand("Get-SvnRepository");
                    Collection<PSObject> output = ps.Invoke();
                    foreach (var o in output)
                    {
                        repos.Add((string)o.Members["Name"].Value);
                    }
                }

                return Response.AsJson(repos).WithStatusCode(HttpStatusCode.OK);
            };
            Post["/{name}"] = x =>
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddCommand("New-SvnRepository");
                    ps.AddArgument(x.name);
                    ps.Invoke();
                }

                // This gives all users read/write access
                string svnAuthz = File.ReadAllText($@"C:\Repositories\{x.name}\conf\VisualSVN-SvnAuthz.ini");
                svnAuthz += "\r\n\r\n[/]\r\n*=rw";
                File.WriteAllText($@"C:\Repositories\{x.name}\conf\VisualSVN-SvnAuthz.ini", svnAuthz);

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
