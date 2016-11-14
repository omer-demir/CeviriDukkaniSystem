using System.Business.Services;
using System.Configuration;
using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin.Hosting;
using MongoDB.Driver;
using RabbitMQ.Client;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Mappers;
using Tangent.CeviriDukkani.Logging;
using Tangent.CeviriDukkani.Messaging;
using Tangent.CeviriDukkani.Messaging.Producer;

namespace System.Api {
    class Program {
        static void Main(string[] args) {
            string baseAddress = "http://localhost:8001/";

            Bootstrapper();
            Console.WriteLine("Bootstrapper finished");

            var webApp = WebApp.Start<Startup>(url: baseAddress);

            Console.WriteLine($"SS is ready in {baseAddress}");

            Console.ReadLine();

            Console.WriteLine("Starting to close SS...");
        }

        public static void Bootstrapper() {
            var builder = new ContainerBuilder();
            var settings = builder.RegisterSettings();

            builder.RegisterCommons(settings);
            builder.RegisterBusiness();

            
            builder.RegisterEvents(settings);
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            Container = builder.Build();
            //CustomLogger.Logger.Info($"SS service is up and ready with projections {DateTime.Today}");
        }

        public static IContainer Container { get; set; }
    }

    public static class AutofacExtensions {
        public static void RegisterCommons(this ContainerBuilder builder, Settings settings) {

            builder.RegisterType<CeviriDukkaniModel>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<CustomMapperConfiguration>().AsSelf().SingleInstance();
            builder.RegisterInstance(CustomLogger.Logger).As<ILog>().SingleInstance();
            var instance = new MongoClient(settings.MongoEventStore);

            builder.RegisterInstance(instance).As(typeof(MongoClient));
        }

        public static void RegisterBusiness(this ContainerBuilder builder) {
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            builder.RegisterType<CommonService>().As<ICommonService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
        }

        public static void RegisterEvents(this ContainerBuilder builder, Settings settings) {
            var connection =
                new RabbitMqConnectionFactory(settings.RabbitHost, settings.RabbitPort, settings.RabbitUserName,
                    settings.RabbitPassword).CreateConnection();
            var dispatcher =
                new RabbitMqDispatcherFactory(connection, settings.RabbitExchangeName, CustomLogger.Logger)
                    .CreateDispatcher();

            builder.RegisterInstance<IConnection>(connection);
            builder.RegisterInstance<IDispatchCommits>(dispatcher);
        }

        public static Settings RegisterSettings(this ContainerBuilder builder) {
            var settings = new Settings {
                RabbitExchangeName = ConfigurationManager.AppSettings["RabbitExchangeName"],
                RabbitHost = ConfigurationManager.AppSettings["RabbitHost"],
                RabbitPassword = ConfigurationManager.AppSettings["RabbitPassword"],
                RabbitPort = int.Parse(ConfigurationManager.AppSettings["RabbitPort"]),
                RabbitUserName = ConfigurationManager.AppSettings["RabbitUserName"],
                DocumentServiceEndpoint = ConfigurationManager.AppSettings["DocumentServiceEndpoint"],
                MongoEventStore = ConfigurationManager.AppSettings["MongoEventStore"]
            };

            builder.RegisterInstance(settings);
            return settings;
        }
    }
}
