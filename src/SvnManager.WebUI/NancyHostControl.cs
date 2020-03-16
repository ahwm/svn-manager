using Nancy.Hosting.Self;
using System;

namespace SvnManager.WebUI
{
    public class NancyHostControl : IDisposable
    {
        NancyHost _host { get; set; }
        public NancyHostControl(string host, int port = 9664, bool https = false)
        {
            string scheme = https ? "https" : "http";
            _host = new NancyHost(new Uri($"{scheme}://{host}:{port}"));
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
            _host.Dispose();
        }
    }
}
