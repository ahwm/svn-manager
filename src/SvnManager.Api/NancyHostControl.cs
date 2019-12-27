using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.Api
{
    public class NancyHostControl : IDisposable
    {
        NancyHost _host { get; set; }
        public NancyHostControl(string host, bool https = false)
        {
            string scheme = https ? "https" : "http";
            _host = new NancyHost(new Uri($"{scheme}://{host}:9664"));
        }

        public void Start()
        {
            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();
        }

        public void Dispose()
        {
            _host.Stop();
        }
    }
}
