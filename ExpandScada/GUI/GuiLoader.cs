using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Reflection;
using ExpandScada.SignalsGateway;

namespace ExpandScada.GUI
{
    public class GuiLoader
    {
        // Each file = one screen. Canvas in the root 
        // Find all files in Screen's folder, load them here and add to the List/Dictionary of UIElements
        // Read bindings block (special section in the file) and create each needed connection

        //static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static Dictionary<string, UIElement> screens = new Dictionary<string, UIElement>();

        public static void FindAndLoadScreens(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, "*.xaml");

            if (files == null || files.Length == 0)
            {
                //Logger.Error("No screens were found");
                throw new FileNotFoundException($"In folder '{folderPath}' no screens were found");
            }

            foreach (var file in files)
            {
                var foundScreen = LoadRootUiElement(file);
                screens.Add(Path.GetFileNameWithoutExtension(file), foundScreen);
            }
        }

        static UIElement LoadRootUiElement(string filePath)
        {
            UIElement rootElement;

            // Clean method, but file must be without x:Class definition and without any trash inside
            //---------------------------------------------------------------------------------------
            //using FileStream s = new FileStream(filePath, FileMode.Open);
            //rootElement = (UIElement)XamlReader.Load(s);
            //s.Close();
            //---------------------------------------------------------------------------------------

            // We can not use XAML reader, because there will be problem with x:Class and it is not worling with any additional blocks inside
            // We can not use XML also, because there is more then one root element, and it is at least...

            // So let's work with file as string and remove x:Class manually and find special block
            string wholeFile = File.ReadAllText(filePath);
            string[] lines = wholeFile.Split("\n");

            // Find UserControl block
            // We know, that before UserControl there is nothing. Special block is ONLY after this block
            List<string> userControlLines = new List<string>();
            int endOfUserElementIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("</UserControl>"))
                {
                    endOfUserElementIndex = i;
                    break;
                }
            }

            if (endOfUserElementIndex == -1)
            {
                throw new Exception($"Error in screen {Path.GetFileNameWithoutExtension(filePath)}: End of UserControl not found. Wrong file structure.");
            }

            // Remove x:Class property
            // TODO now it is can be only in first line, maybe will be problems if it will be gone to another line
            if (lines[0].Contains("x:Class"))
            {
                lines[0] = "<UserControl \r";
            }

            for (int i = 0; i < endOfUserElementIndex + 1; i++)
            {
                userControlLines.Add($"{lines[i]}\n");
            }

            string userControlString = string.Join(String.Empty, userControlLines);
            rootElement = (UIElement)XamlReader.Parse(userControlString);

            // Now we can get our additional block for parsing
            List<string> specialBlockLines = new List<string>();
            for (int i = endOfUserElementIndex + 1; i < lines.Length; i++)
            {
                specialBlockLines.Add($"{lines[i]}\n");
            }

            try
            {
                LoadAllBindingsForScreen(specialBlockLines, rootElement);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in screen {Path.GetFileNameWithoutExtension(filePath)}: {ex.Message}");
            }

            return rootElement;
        }

        // TODO add normal log messages when exceptions
        static void LoadAllBindingsForScreen(List<string> specialBlockLines, UIElement rootElement)
        {
            // - Parse text like XML document
            // - Run in the cycle all Binding nodes
            //      - check if there is such a UI element with this name
            //      - Somehow check if this element has property with this name 
            //      - create binding and attach to the property via reflection(?) check this

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(string.Join(string.Empty, specialBlockLines));
            XmlNodeList bindingNodes = doc.GetElementsByTagName("Binding");

            string uiName = string.Empty;
            string propertyName = string.Empty;
            string signalName = string.Empty;
            Type uiType = null;
            foreach (XmlNode node in bindingNodes)
            {
                uiName = node.Attributes["UiName"].Value;
                propertyName = node.Attributes["PropertyName"].Value;
                signalName = node.Attributes["SignalName"].Value;

                if (uiName.Length == 0 || propertyName.Length == 0 || signalName.Length == 0 )
                {
                    throw new ArgumentException($"Error in Binding node: {node.OuterXml}");
                }

                UIElement bindedElement = (UIElement)LogicalTreeHelper.FindLogicalNode(rootElement, uiName);

                if (bindedElement == null)
                {
                    // TODO add screen's name somehow
                    throw new Exception($"UI element with name \"{uiName}\" isn't exist in the screen");
                }

                // Searching for dependency property
                uiType = bindedElement.GetType();
                var field = FindFieldInClassOrBases(uiType, propertyName);

                if (field == null)
                {
                    throw new Exception($"Property \"{propertyName}\" isn't exist in the element \"{uiName}\"");
                }

                if (!SignalStorage.allNamedSignals.ContainsKey(signalName))
                {
                    throw new Exception($"Signal \"{signalName}\" isn't exist in the signal storage");
                }

                // Install the binding
                Binding myBinding = new Binding("TypedValue");
                myBinding.Source = SignalStorage.allNamedSignals[signalName];
                DependencyProperty dp = (DependencyProperty)field.GetValue(bindedElement);
                BindingOperations.SetBinding(bindedElement, dp, myBinding);


                //Type abstr = SignalStorage.allNamedSignals[signalName].GetType();


            }




        }

        static FieldInfo FindFieldInClassOrBases(Type classType, string fieldName)
        {
            bool found = false;
            Type currentType = classType;
            FieldInfo field = null;
            do
            {
                field = currentType.GetField(fieldName);
                if (field != null)
                {
                    break;
                }
                else
                {
                    currentType = currentType.BaseType;
                }
            } while (currentType != null);

            return field;
        }











      
    }
}
