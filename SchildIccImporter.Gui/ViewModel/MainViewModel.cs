using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LinqToDB.Common;
using Microsoft.Extensions.Logging;
using SchildIccImporter.Gui.Message;
using SchildIccImporter.Settings;
using SchulIT.IccImport;
using SchulIT.IccImport.Response;
using SchulIT.SchildExport;
using SchulIT.SchildIccImporter.Core;
using SchulIT.SchildIccImporter.Settings;
using System;
using System.DirectoryServices.ActiveDirectory;

namespace SchildIccImporter.Gui.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        private bool importTeachers;

        public bool ImportTeachers
        {
            get => importTeachers;
            set
            {
                Set(() => ImportTeachers, ref importTeachers, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importStudents;

        public bool ImportStudents
        {
            get => importStudents;
            set
            {
                Set(() => ImportStudents, ref importStudents, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importGrades;

        public bool ImportGrades
        {
            get { return importGrades; }
            set
            {
                Set(() => ImportGrades, ref importGrades, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importGradeTeachers;

        public bool ImportGradeTeachers
        {
            get { return importGradeTeachers; }
            set
            {
                Set(() => ImportGradeTeachers, ref importGradeTeachers, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importGradeMemberships;

        public bool ImportGradeMemberships
        {
            get { return importGradeMemberships; }
            set
            {
                Set(() => ImportGradeMemberships, ref importGradeMemberships, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importSubjects;

        public bool ImportSubjects
        {
            get { return importSubjects; }
            set
            {
                Set(() => ImportSubjects, ref importSubjects, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importStudyGroups;

        public bool ImportStudyGroups
        {
            get { return importStudyGroups; }
            set
            {
                Set(() => ImportStudyGroups, ref importStudyGroups, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importStudyGroupMemberships;

        public bool ImportStudyGroupMemberships
        {
            get { return importStudyGroupMemberships; }
            set
            {
                Set(() => ImportStudyGroupMemberships, ref importStudyGroupMemberships, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importTuitions;

        public bool ImportTuitions
        {
            get { return importTuitions; }
            set
            {
                Set(() => ImportTuitions, ref importTuitions, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool importPrivacy;

        public bool ImportPrivacy
        {
            get { return importPrivacy; }
            set
            {
                Set(() => ImportPrivacy, ref importPrivacy, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private short year;

        public short Year
        {
            get { return year; }
            set
            {
                Set(() => Year, ref year, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private short section;

        public short Section
        {
            get { return section; }
            set
            {
                Set(() => Section, ref section, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool isImportRunning;

        public bool IsImportRunning
        {
            get { return isImportRunning; }
            set
            {
                Set(() => IsImportRunning, ref isImportRunning, value);
                ImportCommand?.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands

        public RelayCommand ImportCommand { get; private set; }

        public RelayCommand SelectAllCommand { get; private set; }

        public RelayCommand UnselectAllCommand { get; private set; }

        public RelayCommand LoadSchoolInfoCommand { get; private set; }

        #endregion

        #region Services

        public IMessenger Messenger { get { return base.MessengerInstance; } }

        private readonly IExporter exporter;

        private readonly IIccImporter iccImporter;
        private readonly ISchildIccImporter schildIccImporter;

        private readonly ISettingsManager settingsManager;

        private readonly ILogger<MainViewModel> logger;

        #endregion

        public MainViewModel(IExporter exporter, IIccImporter iccImporter, ISchildIccImporter schildIccImporter, ISettingsManager settingsManager, ILogger<MainViewModel> logger, IMessenger messenger)
            : base(messenger)
        {
            ImportCommand = new RelayCommand(Import, CanImport);
            SelectAllCommand = new RelayCommand(SelectAll);
            UnselectAllCommand = new RelayCommand(UnselectAll);
            LoadSchoolInfoCommand = new RelayCommand(LoadCurrentSectionAsync);

            this.exporter = exporter;
            this.iccImporter = iccImporter;
            this.schildIccImporter = schildIccImporter;
            this.settingsManager = settingsManager;
            this.logger = logger;
        }

        public async void LoadCurrentSectionAsync()
        {
            try
            {
                var settings = await settingsManager.LoadSettingsAsync();
                exporter.Configure(
                    GetConnectionProviderFromSettings(settings.Schild.ConnectionType),
                    settings.Schild.ConnectionString,
                    false
                );
                

                var schoolInfo = await exporter.GetSchoolInfoAsync();

                if (schoolInfo.CurrentYear.HasValue)
                {
                    Year = schoolInfo.CurrentYear.Value;
                }

                if (schoolInfo.CurrentSection.HasValue)
                {
                    Section = schoolInfo.CurrentSection.Value;
                }
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage
                {
                    Exception = e,
                    Title = "Fehler",
                    Header = "Fehler beim Laden der Schulinfo",
                    Text = "Entweder ist der Datenbankserver nicht erreichbar oder wurde noch nicht konfiguriert."
                });
            }
        }

        private void Import()
        {
            Messenger.Send(new ConfirmMessage
            {
                Title = "Hochladen bestätigen",
                Content = "Bitte die hochzuladenden Einträge sowie die Schuljahresabschnitte überprüfen.",
                ConfirmAction = DoImport,
                CancelAction = () =>
                {
                    Messenger.Send(new DialogMessage { Header = "Aktion abgebrochen", Title = "Import abgebrochen", Text = "Es wurde kein Importvorgang gestartet." });
                }
            });
        }

        private void HandleResponse(IResponse response)
        {
            var importResponse = response as ImportResponse;

            if(importResponse != null && importResponse.IgnoredEntities.Count > 0)
            {
                Messenger.Send(new ResponseMessage { Type = ResponseMessageType.Information, ResponseCode = importResponse.ResponseCode, ResponseBody = importResponse.ResponseBody });
            }

            var errorResponse = response as ErrorResponse;

            if(errorResponse != null)
            {
                Messenger.Send(new ResponseMessage { Type = ResponseMessageType.Error, ResponseCode = errorResponse.ResponseCode, ResponseBody = errorResponse.ResponseBody });
            }
        }

        private async void DoImport()
        {
            try {
                IsImportRunning = true;
                var settings = await settingsManager.LoadSettingsAsync();

                schildIccImporter.TeacherTagMapping.Clear();

                foreach (var mapping in settings.TeacherTagMapping)
                {
                    schildIccImporter.TeacherTagMapping.Add(mapping.Key, mapping.Value);
                }

                schildIccImporter.OnlyVisibleEntities = settings.Schild.OnlyVisibleEntities;
                schildIccImporter.GradesWithoutSubstituteTeachers.Clear();
                schildIccImporter.GradesWithoutSubstituteTeachers.AddRange(settings.GradesWithoutSubstituteTeachers);
                iccImporter.BaseUrl = settings.Icc.Url;
                iccImporter.Token = settings.Icc.Token;

                Configuration.Linq.GuardGrouping = false;

                var students = await exporter.GetStudentsAsync(settings.Schild.StudentFilter, DateTime.Today);

                if (ImportGrades)
                {
                    logger.LogInformation("Importiere Klassen/Jgst.");
                    HandleResponse(await schildIccImporter.ImportGradesAsync(Year, Section));
                    logger.LogInformation("Klassen/Jgst. erfolgreich importiert.");
                }

                if(ImportSubjects)
                {
                    logger.LogInformation("Importiere Fächer...");
                    HandleResponse(await schildIccImporter.ImportSubjectsAsync());
                    logger.LogInformation("Fächer erfolgreich importiert.");
                }

                if(ImportTeachers)
                {
                    logger.LogInformation("Importiere Lehrkräfte...");
                    HandleResponse(await schildIccImporter.ImportTeachersAsync(Year, Section));
                    logger.LogInformation("Lehrkräfte erfolgreich importiert.");
                }

                if(ImportGradeTeachers)
                {
                    logger.LogInformation("Importiere Klassenleitungen...");
                    HandleResponse(await schildIccImporter.ImportGradeTeachersAsync(Year, Section));
                    logger.LogInformation("Klassenleitungen erfolgreich importiert.");
                }

                if(ImportPrivacy)
                {
                    logger.LogInformation("Importiere Datenschutz-Kategorien...");
                    HandleResponse(await schildIccImporter.ImportPrivacyCategoriesAsync());
                    logger.LogInformation("Datenschutz-Kategorien erfolgreich importiert.");
                }

                if(ImportStudents)
                {
                    logger.LogInformation("Importere Lernende...");
                    HandleResponse(await schildIccImporter.ImportStudentsAsync(Year, Section, settings.Schild.StudentFilter));
                    logger.LogInformation("Lernende erfolgreich importiert.");

                }

                if (ImportGradeMemberships)
                {
                    logger.LogInformation("Importiere Klassenmitgliedschaften...");
                    HandleResponse(await schildIccImporter.ImportGradeMembershipsAsync(Year, Section, settings.Schild.StudentFilter));
                    logger.LogInformation("Klassenmitgliedschaften erfolgreich importiert.");
                }

                if (ImportStudyGroups)
                {
                    logger.LogInformation("Importiere Lerngruppen...");
                    HandleResponse(await schildIccImporter.ImportStudyGroupsAsync(students, Year, Section));
                    logger.LogInformation("Lerngruppen erfolgreich importiert.");
                }

                if(ImportStudyGroupMemberships)
                {
                    logger.LogInformation("Importiere Lerngruppen-Mitgliedschaften...");
                    HandleResponse(await schildIccImporter.ImportStudyGroupMembershipsAsync(students, Year, Section));
                    logger.LogInformation("Lerngruppen-Mitgliedschaften erfolgreich importiert.");
                }

                if(ImportTuitions)
                {
                    logger.LogInformation("Importiere Unterrichte...");
                    HandleResponse(await schildIccImporter.ImportTuitionsAsync(students, Year, Section));
                    logger.LogInformation("Unterrichte erfolgreich importiert.");
                }

                logger.LogInformation("Import abgeschlossen.");
            }
            catch (Exception e)
            {
                Messenger.Send(new ErrorDialogMessage
                {
                    Exception = e,
                    Title = "Fehler",
                    Header = "Fehler beim Importvorgang",
                    Text = "Beim Importvorgang ist etwas schief gelaufen."
                });
            }
            finally
            {
                IsImportRunning = false;
            }
        }

        private bool CanImport()
        {
            return (ImportGrades == true
                || ImportGradeTeachers == true
                || ImportGradeMemberships == true
                || ImportPrivacy == true
                || ImportStudents == true
                || ImportStudyGroupMemberships == true
                || ImportStudyGroups == true
                || ImportSubjects == true
                || ImportTeachers == true
                || ImportTuitions == true)
                && Year >= 0 && Section >= 0
                && !IsImportRunning;
        }

        private void SelectAll()
        {
            ImportGrades = true;
            ImportGradeTeachers = true;
            ImportGradeMemberships = true;
            ImportPrivacy = true;
            ImportStudents = true;
            ImportStudyGroupMemberships = true;
            ImportStudyGroups = true;
            ImportSubjects = true;
            ImportTeachers = true;
            ImportTuitions = true;
        }

        private void UnselectAll()
        {
            ImportGrades = false;
            ImportGradeTeachers = false;
            ImportGradeMemberships = false;
            ImportPrivacy = false;
            ImportStudents = false;
            ImportStudyGroupMemberships = false;
            ImportStudyGroups = false;
            ImportSubjects = false;
            ImportTeachers = false;
            ImportTuitions = false;
        }

        private ConnectionProvider GetConnectionProviderFromSettings(ConnectionType type)
        {
            switch(type)
            {
                case ConnectionType.Access:
                    return ConnectionProvider.Access;
                case ConnectionType.MySQL:
                    return ConnectionProvider.MySqlConnector;
                case ConnectionType.MSSQL:
                default:
                    return ConnectionProvider.SqlServer;
            }
        }
    }
}
