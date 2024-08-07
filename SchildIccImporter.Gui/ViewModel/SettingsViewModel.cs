using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SchildIccImporter.Settings;
using SchulIT.SchildExport;
using SchulIT.SchildExport.Entities;
using SchulIT.SchildExport.Linq;
using SchulIT.SchildExport.Models;
using SchulIT.SchildIccImporter.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SchildIccImporter.Gui.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Properties

        private ConnectionType connectionType;

        public ConnectionType ConnectionType
        {
            get => connectionType;
            set => Set(() => ConnectionType, ref connectionType, value);
        }

        private string connectionString;

        public string ConnectionString
        {
            get => connectionString;
            set => Set(() => ConnectionString, ref connectionString, value);
        }

        private bool onlyVisible;

        public bool OnlyVisible
        {
            get => onlyVisible;
            set => Set(() => OnlyVisible, ref onlyVisible, value);
        }

        private string iccEndpoint;

        public string IccEndpoint
        {
            get => iccEndpoint;
            set => Set(() => IccEndpoint, ref iccEndpoint, value);
        }

        private string iccToken;

        public string IccToken
        {
            get { return iccToken; }
            set { Set(() => IccToken, ref iccToken, value); }
        }

        public List<ConnectionType> ConnectionTypes { get; private set; } = new List<ConnectionType>();

        public List<SchuelerStatus> SchuelerStatusList { get; private set; } = new List<SchuelerStatus>();

        public ObservableCollection<SchuelerStatus> EnabledSchuelerStatusList { get; private set; } = new ObservableCollection<SchuelerStatus>();

        public ObservableCollection<string> Grades { get; private set; } = new ObservableCollection<string>();

        public ObservableCollection<string> GradesWithoutSubstituteTeachers { get; private set; } = new ObservableCollection<string>();

        private bool canSaveSettings;

        public bool CanSaveSettings
        {
            get => canSaveSettings;
            set
            {
                Set(() => CanSaveSettings, ref canSaveSettings, value);
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands

        public RelayCommand SaveCommand { get; private set; }

        #endregion

        #region Services

        private readonly ISettingsManager settingsManager;
        private readonly IExporter schildExporter;

        #endregion


        public SettingsViewModel(ISettingsManager settingsManager, IExporter exporter)
        {
            this.settingsManager = settingsManager;
            this.schildExporter = exporter;

            LoadConnectionTypes();
            LoadSchuelerStatus();
            SaveCommand = new RelayCommand(SaveSettings, CanSaveSettings);

            LoadSettings();
        }

        private async void LoadSettings()
        {
            var settings = await settingsManager.LoadSettingsAsync();

            OnlyVisible = settings.Schild.OnlyVisibleEntities;
            ConnectionType = settings.Schild.ConnectionType;
            ConnectionString = settings.Schild.ConnectionString;

            foreach(SchuelerStatus status in settings.Schild.StudentFilter)
            {
                EnabledSchuelerStatusList.Add(status);
            }

            IccEndpoint = settings.Icc.Url;
            IccToken = settings.Icc.Token;

            if(!string.IsNullOrEmpty(ConnectionString))
            {
                Grades.Clear();
                GradesWithoutSubstituteTeachers.Clear();

                try
                {
                    var grades = (await schildExporter.GetGradesAsync()) as IEnumerable<Grade>;
                    if (OnlyVisible)
                    {
                        grades = grades.WhereIsVisible();
                    }

                    foreach (var grade in grades)
                    {
                        Grades.Add(grade.Name);
                        if (settings.GradesWithoutSubstituteTeachers.Contains(grade.Name))
                        {
                            GradesWithoutSubstituteTeachers.Add(grade.Name);
                        }
                    }
                }
                catch
                {
                    // nothing to do here - list boxes were already cleared
                }
            }
        }

        private async void SaveSettings()
        {
            try
            {
                CanSaveSettings = false;
                var settings = new JsonSettings();

                var iccSettings = settings.Icc as JsonIccSettings;
                iccSettings.Url = IccEndpoint;
                iccSettings.Token = IccToken;

                var schildSettings = settings.Schild as JsonSchildSettings;
                schildSettings.ConnectionType = ConnectionType;
                schildSettings.ConnectionString = ConnectionString;
                schildSettings.OnlyVisibleEntities = OnlyVisible;
                schildSettings.StudentFilter = EnabledSchuelerStatusList.Cast<int>().Distinct().ToArray();

                settings.GradesWithoutSubstituteTeachers.Clear();
                settings.GradesWithoutSubstituteTeachers.AddRange(GradesWithoutSubstituteTeachers.Distinct());

                await settingsManager.SaveSettingsAsync(settings);
            }
            finally
            {
                CanSaveSettings = true;
            }
        }

        private void LoadConnectionTypes()
        {
            ConnectionTypes.Clear();

            foreach(ConnectionType type in Enum.GetValues(typeof(ConnectionType)))
            {
                ConnectionTypes.Add(type);
            }
        }

        private void LoadSchuelerStatus()
        {
            SchuelerStatusList.Clear();

            foreach (SchuelerStatus status in Enum.GetValues(typeof(SchuelerStatus)))
            {
                SchuelerStatusList.Add(status);
            }
        }

    }
}
