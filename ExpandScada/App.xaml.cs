using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExpandScada.GUI;


namespace ExpandScada
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Info("Starting up...");
            try
            {
                Logger.Info("Loading of screens");
                GuiLoader.FindAndLoadScreens(@"C:\Users\admin\Desktop\SCADA\Sources\Tests\WpfUiLibTest1\WpfShower1\TestElements\ChangedManually");
            }
            catch (Exception ex)
            {
                Logger.Error($"Critical error, will be shut down: {ex.Message}");
                Application.Current.Shutdown(); // doesn't wock, be more redical
            }
        }
    }
}
