using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Extensions.Logging;
using SchildIccImporter.Gui.Message;
using SchulIT.IccImport;
using SchulIT.SchildExport;
using SchulIT.SchildIccImporter.Core;
using SchulIT.SchildIccImporter.Settings;
using System;

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
                exporter.Configure(settings.Schild.ConnectionString, false);

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

        private async void DoImport()
        {
            try {
                IsImportRunning = true;
                var settings = await settingsManager.LoadSettingsAsync();

                foreach (var mapping in settings.TeacherTagMapping)
                {
                    schildIccImporter.TeacherTagMapping.Add(mapping.Key, mapping.Value);
                }

                schildIccImporter.OnlyVisibleEntities = settings.Schild.OnlyVisibleEntities;
                iccImporter.BaseUrl = settings.Icc.Url;
                iccImporter.Token = settings.Icc.Token;

                var students = await exporter.GetStudentsAsync(settings.Schild.StudentFilter, DateTime.Today);

                if (ImportGrades)
                {
                    logger.LogInformation("Importiere Klassen/Jgst.");
                    await schildIccImporter.ImportGradesAsync();
                    logger.LogInformation("Klassen/Jgst. erfolgreich importiert.");
                }

                if(ImportSubjects)
                {
                    logger.LogInformation("Importiere Fächer...");
                    await schildIccImporter.ImportSubjectsAsync();
                    logger.LogInformation("Fächer erfolgreich importiert.");
                }

                if(ImportTeachers)
                {
                    logger.LogInformation("Importiere Lehrkräfte...");
                    await schildIccImporter.ImportTeachersAsync(Year, Section);
                    logger.LogInformation("Lehrkräfte erfolgreich importiert.");
                }

                if(ImportGradeTeachers)
                {
                    logger.LogInformation("Importiere Klassenleitungen...");
                    await schildIccImporter.ImportGradeTeachersAsync();
                    logger.LogInformation("Klassenleitungen erfolgreich importiert.");
                }

                if(ImportPrivacy)
                {
                    logger.LogInformation("Importiere Datenschutz-Kategorien...");
                    await schildIccImporter.ImportPrivacyCategoriesAsync();
                    logger.LogInformation("Datenschutz-Kategorien erfolgreich importiert.");
                }

                if(ImportStudents)
                {
                    logger.LogInformation("Importere Lernende...");
                    await schildIccImporter.ImportStudentsAsync(settings.Schild.StudentFilter, DateTime.Today);
                    logger.LogInformation("Lernende erfolgreich importiert.");
                }

                if (ImportStudyGroups)
                {
                    logger.LogInformation("Importiere Lerngruppen...");
                    await schildIccImporter.ImportStudyGroupsAsync(students, Year, Section);
                    logger.LogInformation("Lerngruppen erfolgreich importiert.");
                }

                if(ImportStudyGroupMemberships)
                {
                    logger.LogInformation("Importiere Lerngruppen-Mitgliedschaften...");
                    await schildIccImporter.ImportStudyGroupMembershipsAsync(students, Year, Section);
                    logger.LogInformation("Lerngruppen-Mitgliedschaften erfolgreich importiert.");
                }

                if(ImportTuitions)
                {
                    logger.LogInformation("Importiere Unterrichte...");
                    await schildIccImporter.ImportTuitionsAsync(students, Year, Section);
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
            ImportPrivacy = true;
            ImportStudents = true;
            ImportStudyGroupMemberships = true;
            ImportStudyGroups = true;
            ImportSubjects = true;
            ImportTeachers = true;
            ImportTuitions = true;
        }
    }
}
