using System;

namespace SvnManager.WebUI
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
