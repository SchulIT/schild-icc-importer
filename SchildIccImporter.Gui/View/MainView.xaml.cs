using Fluent;
using KPreisser.UI;
using NLog;
using SchildIccImporter.Gui.Logger;
using SchildIccImporter.Gui.Message;
using SchildIccImporter.Gui.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace SchildIccImporter.Gui.View
{
    /// <summary>
    /// Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        public MainView()
        {
            InitializeComponent();

            var config = LogManager.Configuration;
            var target = config.AllTargets.FirstOrDefault(x => x is ListViewTarget) as ListViewTarget;

            if (target != null)
            {
                loggerListView.ItemsSource = target.Events;

                target.Events.CollectionChanged += (s, e) =>
                {
                    // Hack to always scroll to bottom
                    if(target.Events.Count > 0)
                    {
                        loggerListView.ScrollIntoView(target.Events[target.Events.Count - 1]);
                    }
                };
            }
            else
            {
                MessageBox.Show("nlog.config Fehler - es werden keine Logging-Informationen angezeigt.");
            }

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var locator = App.Current.Resources["ViewModelLocator"] as ViewModelLocator;

            var messenger = locator.Messenger;
            messenger.Register<DialogMessage>(this, OnDialogMessage);
            messenger.Register<ErrorDialogMessage>(this, OnErrorDialogMessage);
            messenger.Register<ConfirmMessage>(this, OnConfirmMessage);
        }

        private void OnCloseButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        private void OnRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void OnClearLogClick(object sender, RoutedEventArgs e)
        {
            var config = LogManager.Configuration;
            var target = config.AllTargets.FirstOrDefault(x => x is ListViewTarget) as ListViewTarget;

            if (target != null)
            {
                target.Events.Clear();
            }
        }

        private void OnConfirmMessage(ConfirmMessage msg)
        {
            var page = new TaskDialogPage
            {
                Title = msg.Title,
                Text = msg.Content,
                Instruction = msg.Title,
                Icon = TaskDialogStandardIcon.Information
            };

            var buttonYes = page.StandardButtons.Add(TaskDialogResult.Continue);
            var buttonClose = page.StandardButtons.Add(TaskDialogResult.Close);

            buttonYes.Click += (s, e) =>
            {
                msg.ConfirmAction();
            };

            buttonClose.Click += (s, e) =>
            {
                msg.CancelAction();
            };

            var dialog = new TaskDialog(page);
            dialog.Show(new WindowInteropHelper(this).Handle);
        }

        private void OnDialogMessage(DialogMessage msg)
        {
            var page = new TaskDialogPage
            {
                Title = msg.Title,
                Text = msg.Text,
                Instruction = msg.Header,
                Icon = TaskDialogStandardIcon.Information
            };

            var dialog = new TaskDialog(page);
            dialog.Show(new WindowInteropHelper(this).Handle);
        }

        private void OnErrorDialogMessage(ErrorDialogMessage msg)
        {
            var page = new TaskDialogPage
            {
                Title = msg.Title,
                Text = msg.Text,
                Instruction = msg.Header,
                Icon = TaskDialogStandardIcon.Error,
                Expander =
                {
                    Text = msg.Exception.Message,
                    ExpandFooterArea = true
                }
            };

            var dialog = new TaskDialog(page);
            dialog.Show(new WindowInteropHelper(this).Handle);
        }
    }
}
