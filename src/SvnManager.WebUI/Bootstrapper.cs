using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using Nancy.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.WebUI
{
    public class Bootstrapper: DefaultNancyBootstrapper
    {
        public static WebEvents webEvents;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            pipelines.OnError += (ctx, ex) =>
            {
                Notification.SendError(ex);
                return null;
            };
            //CookieBasedSessions.Enable(pipelines);

            //var cryptographyConfiguration = new CryptographyConfiguration(new RijndaelEncryptionProvider(new PassphraseKeyGenerator("k60S3wal**7I2IVD$n1V", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })), new DefaultHmacProvider(new PassphraseKeyGenerator("51t!1w2DH^Ge$iR8G^4w", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })));
            //var formsAuthConfiguration = new FormsAuthenticationConfiguration()
            //{
            //    RedirectUrl = "~/login",
            //    UserMapper = container.Resolve<IUserMapper>(),
            //    CryptographyConfiguration = cryptographyConfiguration,
            //    //RequiresSSL = true
            //};
            //FormsAuthentication.Enable(pipelines, formsAuthConfiguration);

            //Csrf.Enable(pipelines);

            //// https://stackoverflow.com/q/37445013/1892993
            //container.Register(typeof(IFluentAdapterFactory), typeof(DefaultFluentAdapterFactory));
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(OnConfigurationBuilder);
            }
        }

        void OnConfigurationBuilder(NancyInternalConfiguration x)
        {
            x.ViewLocationProvider = typeof(ResourceViewLocationProvider);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            ResourceViewLocationProvider.RootNamespaces.Add(Assembly.GetAssembly(typeof(RepositoriesModule)), "SvnManager.WebUI.Views");
        }
    }
}
