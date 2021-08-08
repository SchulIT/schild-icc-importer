using Autofac;
using CommandLine;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SchulIT.IccImport;
using SchulIT.SchildExport;
using SchulIT.SchildIccImporter.Core;
using SchulIT.SchildIccImporter.Settings;
using System;
using System.Threading.Tasks;

namespace SchulIT.SchildIccImporter.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new Parser(config => config.HelpWriter = Console.Out);
            var result = parser.ParseArguments<Options>(args);

            await result.MapResult(async options => await Run(options), _ => Task.FromResult(1));
        }

        static async Task Run(Options options)
        {
            var container = BuildContainer();
            var logger = container.Resolve<ILogger<Program>>();

            logger.LogDebug("Loading settings...");
            var settings = await container.Resolve<ISettingsManager>().LoadSettingsAsync();
            logger.LogDebug("Settings loaded.");

            if (string.IsNullOrEmpty(settings.Schild.ConnectionString))
            {
                logger.LogError("You must specify a ConnectionString in the settings.json file.");
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(settings.Icc.Url))
            {
                logger.LogError("You must specify the URL of the ICC.");
                Environment.Exit(1);
            }

            logger.LogDebug("Configure services...");

            // Configure
            var iccImporter = container.Resolve<IIccImporter>();
            iccImporter.BaseUrl = settings.Icc.Url;
            iccImporter.Token = settings.Icc.Token;

            var exporter = container.Resolve<IExporter>();
            exporter.Configure(settings.Schild.ConnectionString, false);

            var schildIccImporter = container.Resolve<ISchildIccImporter>();

            foreach (var mapping in settings.TeacherTagMapping)
            {
                schildIccImporter.TeacherTagMapping.Add(mapping.Key, mapping.Value);
            }

            schildIccImporter.OnlyVisibleEntities = settings.Schild.OnlyVisibleEntities;
            schildIccImporter.GradesWithoutSubstituteTeachers.Clear();
            schildIccImporter.GradesWithoutSubstituteTeachers.AddRange(settings.GradesWithoutSubstituteTeachers);

            logger.LogDebug("Services configured.");
            logger.LogInformation("Retrieving students...");
            var currentStudents = await exporter.GetStudentsAsync(settings.Schild.StudentFilter, DateTime.Today);

            if (options.Grades)
            {
                logger.LogInformation("Uploading grades...");
                await schildIccImporter.ImportGradesAsync(options.Year, options.Section);
            }

            if (options.Subjects)
            {
                logger.LogInformation("Uploading subjects...");
                await schildIccImporter.ImportSubjectsAsync();
            }

            if (options.Teachers)
            {
                logger.LogInformation("Uploading teachers...");
                await schildIccImporter.ImportTeachersAsync(options.Year, options.Section);
            }

            if (options.TeacherGrades)
            {
                logger.LogInformation("Uploading teacher grades...");
                await schildIccImporter.ImportGradeTeachersAsync(options.Year, options.Section);
            }

            if (options.PrivacyCategories)
            {
                logger.LogInformation("Uploading privacy categories...");
                await schildIccImporter.ImportPrivacyCategoriesAsync();
            }

            if (options.Students)
            {
                logger.LogInformation("Uploading students...");
                await schildIccImporter.ImportStudentsAsync(options.Year, options.Section, settings.Schild.StudentFilter, DateTime.Today);
            }

            if (options.StudyGroups)
            {
                logger.LogInformation("Uploading study groups...");
                await schildIccImporter.ImportStudyGroupsAsync(currentStudents, options.Year, options.Section);
            }

            if(options.StudyGroupMemberships)
            {
                logger.LogInformation("Uploading study group memberships...");
                await schildIccImporter.ImportStudyGroupMembershipsAsync(currentStudents, options.Year, options.Section);
            }

            if (options.Tuitions)
            {
                logger.LogInformation("Uploading tuitions...");
                await schildIccImporter.ImportTuitionsAsync(currentStudents, options.Year, options.Section);
            }

            Console.WriteLine("Upload finished. Press any key to close this window.");
            Console.ReadLine();
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<JsonSettingsManager>().As<ISettingsManager>().SingleInstance();

            builder.RegisterType<Exporter>().As<IExporter>().SingleInstance();
            builder.RegisterType<IccImporter>().As<IIccImporter>().SingleInstance();
            builder.RegisterType<Core.SchildIccImporter>().As<ISchildIccImporter>().SingleInstance();

            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterType<NLogLoggerFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}
