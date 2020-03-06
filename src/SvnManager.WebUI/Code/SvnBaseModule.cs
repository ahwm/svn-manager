using Nancy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.WebUI.Code
{
    /// <summary>
    /// Base module class that inherits from NancyModule but includes basic common properties
    /// </summary>
    public class SvnBaseModule : NancyModule
    {
        protected static string RepoPath => ConfigurationManager.AppSettings["Manager.RepoPath"];
        protected static string SvnLocation => ConfigurationManager.AppSettings["SvnLocation"];

        public SvnBaseModule(string modulePath) : base(modulePath)
        { }

        public SvnBaseModule() : base()
        { }
    }
}
