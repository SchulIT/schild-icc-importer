using Autofac;
using GalaSoft.MvvmLight.Messaging;
using LinqToDB.Tools;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using SchulIT.IccImport;
using SchulIT.SchildExport;
using SchulIT.SchildIccImporter.Core;
using SchulIT.SchildIccImporter.Settings;

namespace SchildIccImporter.Gui.ViewModel
{
    public class ViewModelLocator
    {
        private static IContainer container;

        static ViewModelLocator()
        {
            RegisterServices();
        }

        public static void RegisterServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<JsonSettingsManager>().As<ISettingsManager>().SingleInstance();

            builder.RegisterType<Exporter>().As<IExporter>().SingleInstance();
            builder.RegisterType<IccImporter>().As<IIccImporter>().SingleInstance();
            builder.RegisterType<SchulIT.SchildIccImporter.Core.SchildIccImporter>().As<ISchildIccImporter>().SingleInstance();

            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterType<NLogLoggerFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();

            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance().OnActivated(x => x.Instance.LoadCurrentSectionAsync());
            builder.RegisterType<SettingsViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<AboutViewModel>().AsSelf().SingleInstance();

            container = builder.Build();
        }

        public IMessenger Messenger { get { return container.Resolve<IMessenger>(); } }

        public MainViewModel Main { get { return container.Resolve<MainViewModel>(); } }

        public SettingsViewModel Settings { get { return container.Resolve<SettingsViewModel>(); } }

        public AboutViewModel About { get { return container.Resolve<AboutViewModel>(); } }
    }
}
