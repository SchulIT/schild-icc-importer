using NLog;
using NLog.Targets;
using SchildIccImporter.Gui.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SchildIccImporter.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Target.Register<ListViewTarget>("ListView");
        }
    }
}
