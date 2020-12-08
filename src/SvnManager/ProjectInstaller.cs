using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SvnManager
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            byte[] crt;
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.assets.rootCA.crt"))
            {
                using (var file = new MemoryStream())
                {
                    resource.CopyTo(file);
                    crt = file.ToArray();
                }
            }
            byte[] pfx;
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SvnManager.assets.mgr.pfx"))
            {
                using (var file = new MemoryStream())
                {
                    resource.CopyTo(file);
                    pfx = file.ToArray();
                }
            }
            var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite | OpenFlags.MaxAllowed);
            store.Add(new X509Certificate2(crt));
            store.Close();

            store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite | OpenFlags.MaxAllowed);
            store.Add(new X509Certificate2(pfx, "", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet));
            store.Close();
        }
    }
}
