using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SchulIT.SchildExport.Entities;
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

        public List<SchuelerStatus> SchuelerStatusList { get; private set; } = new List<SchuelerStatus>();

        public ObservableCollection<SchuelerStatus> EnabledSchuelerStatusList { get; private set; } = new ObservableCollection<SchuelerStatus>();

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

        #endregion


        public SettingsViewModel(ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            LoadSchuelerStatus();
            SaveCommand = new RelayCommand(SaveSettings, CanSaveSettings);

            LoadSettings();
        }

        private async void LoadSettings()
        {
            var settings = await settingsManager.LoadSettingsAsync();

            OnlyVisible = settings.Schild.OnlyVisibleEntities;
            ConnectionString = settings.Schild.ConnectionString;

            foreach(SchuelerStatus status in settings.Schild.StudentFilter)
            {
                EnabledSchuelerStatusList.Add(status);
            }

            IccEndpoint = settings.Icc.Url;
            IccToken = settings.Icc.Token;
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
                schildSettings.ConnectionString = ConnectionString;
                schildSettings.OnlyVisibleEntities = OnlyVisible;
                schildSettings.StudentFilter = EnabledSchuelerStatusList.Cast<int>().ToArray();

                await settingsManager.SaveSettingsAsync(settings);
            }
            finally
            {
                CanSaveSettings = true;
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
