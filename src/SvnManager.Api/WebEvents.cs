using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.Api
{
    public class WebEvents
    {
        public delegate void CreateRepositoryEventHandler(object sender, CreateRepositoryEventArgs e);
        public event CreateRepositoryEventHandler CreateRepository;
        
        protected virtual void OnSiteCreated(CreateRepositoryEventArgs e)
        {
            CreateRepository?.Invoke(this, e);
        }
    }
    public class CreateRepositoryEventArgs : EventArgs
    {

    }
}
