using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExpandScada.GUI;
using ExpandScada.SignalsGateway;
using ExpandScada.Communication;

namespace ExpandScada
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //const string FOLDER_WITH_SCREENS = @"C:\Users\admin\Desktop\SCADA\Sources\Tests\WpfUiLibTest1\WpfShower1\TestElements\ChangedManually";
        const string FOLDER_WITH_SCREENS = @"C:\Users\admin\Desktop\SCADA\Sources\Project\Screens";
        const string RESOURCES_FILE_PATH = @"C:\Users\admin\Desktop\SCADA\Sources\Tests\WpfUiLibTest1\WpfShower1\TestElements\ResourceStyle\CommonStyle.xaml";
        //const string PROJECT_DB_PATH = "..\\..\\Project\\test1.db"; // TODO doesn't work, why????
        const string PROJECT_DB_PATH = @"C:\Users\admin\Desktop\SCADA\Sources\Project\test1.db";

        const string PROTOCOLS_PATH = @"C:\Users\admin\Desktop\SCADA\Sources\Protocols\Debug";

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load all signals
            SignalLoader.LoadAllSignals(PROJECT_DB_PATH);


            //!!! TEST ONLY!!
            //SignalStorage.allNamedSignals["MaxValueCounter"].Value = 150;


            // Start communication
            //!!! TESTS YET!!!
            
            //CommunicationLoader.LoadAllProtocols(PROTOCOLS_PATH, PROJECT_DB_PATH);
            //var modbusTcp = CommunicationManager.communicationProtocols[1];
            //modbusTcp.StartCommunication();



            // Load common style for screens
            // Relative URI
            //Uri relativeUri = new Uri("/File.xaml",  UriKind.Relative); //AFTER CREATION OF SPECIAL FOLDER USE THIS
            Uri relativeUri = new Uri(RESOURCES_FILE_PATH);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = relativeUri });
            //---------------------------------------------------------------------------------------------------------
            // We can use more flexible resources loading, but define all cases to make right loader first
            //---------------------------------------------------------------------------------------------------------

            // Load screens
            Logger.Info("Starting up...");
            try
            {
                Logger.Info("Loading of screens");
                GuiLoader.FindAndLoadScreens(FOLDER_WITH_SCREENS);
            }
            catch (Exception ex)
            {
                Logger.Error($"Critical error, will be shut down: {ex.Message}");
                Application.Current.Shutdown(); // TODO doesn't wock, be more redical
                return;
            }
        }
    }
}
