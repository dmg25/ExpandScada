using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExpandScada.GUI;
using Common.Gateway;
using ExpandScada.SignalsGateway;
using System.Collections.Concurrent;


namespace ExpandScada
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       // public ConcurrentDictionary<string, Signal> AllNamedSignals = SignalStorage.allNamedSignals;

        public MainWindow()
        {
            InitializeComponent();

           // DataContext = this;

            // Add loaded screens here (only one for tests)
            rootOfRoots.Children.Add(GuiLoader.screens.ElementAt(0).Value);
            //layoutGrid.SetColumn(rootElement, COLUMN);
            //layoutGrid.SetRow(rootElement, ROW);
        }
    }
}
