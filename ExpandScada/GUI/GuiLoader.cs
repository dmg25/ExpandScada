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
using ExpandScada.Commands;
using System.Xml.Linq;
using Common.Communication;

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

        //static UIElement LoadRootUiElement(string filePath)
        //{
        //    UIElement rootElement;

        //    // Clean method, but file must be without x:Class definition and without any trash inside
        //    //---------------------------------------------------------------------------------------
        //    //using FileStream s = new FileStream(filePath, FileMode.Open);
        //    //rootElement = (UIElement)XamlReader.Load(s);
        //    //s.Close();
        //    //---------------------------------------------------------------------------------------

        //    // We can not use XAML reader, because there will be problem with x:Class and it is not worling with any additional blocks inside
        //    // We can not use XML also, because there is more then one root element, and it is at least...

        //    // So let's work with file as string and remove x:Class manually and find special block
        //    string wholeFile = File.ReadAllText(filePath);
        //    string[] lines = wholeFile.Split("\n");

        //    // Find UserControl block
        //    // We know, that before UserControl there is nothing. Special block is ONLY after this block
        //    List<string> userControlLines = new List<string>();
        //    int endOfUserElementIndex = -1;
        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        if (lines[i].Contains("</UserControl>"))
        //        {
        //            endOfUserElementIndex = i;
        //            break;
        //        }
        //    }

        //    if (endOfUserElementIndex == -1)
        //    {
        //        throw new Exception($"Error in screen {Path.GetFileNameWithoutExtension(filePath)}: End of UserControl not found. Wrong file structure.");
        //    }

        //    // Remove x:Class property
        //    // TODO now it is can be only in first line, maybe will be problems if it will be gone to another line
        //    if (lines[0].Contains("x:Class"))
        //    {
        //        lines[0] = "<UserControl \r";
        //    }

        //    for (int i = 0; i < endOfUserElementIndex + 1; i++)
        //    {
        //        userControlLines.Add($"{lines[i]}\n");
        //    }

        //    string userControlString = string.Join(String.Empty, userControlLines);
        //    rootElement = (UIElement)XamlReader.Parse(userControlString);

        //    // Now we can get our additional block for parsing
        //    List<string> specialBlockLines = new List<string>();
        //    for (int i = endOfUserElementIndex + 1; i < lines.Length; i++)
        //    {
        //        specialBlockLines.Add($"{lines[i]}\n");
        //    }

        //    try
        //    {
        //        LoadAllBindingsForScreen(specialBlockLines, rootElement);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error in screen {Path.GetFileNameWithoutExtension(filePath)}: {ex.Message}");
        //    }

        //    return rootElement;
        //}

        static UIElement LoadRootUiElement(string filePath)
        {
            UIElement rootElement;

            // Clean method, but file must be without x:Class definition and without any trash inside
            //---------------------------------------------------------------------------------------
            //using FileStream s = new FileStream(filePath, FileMode.Open);
            //rootElement = (UIElement)XamlReader.Load(s);
            //s.Close();
            //---------------------------------------------------------------------------------------


            // take special section and cut it out

            // go from the bottom to top and take all lines until <EditorBlockForConnections> not found
            // since found - take it too and stop - cut all bottom lines

            string wholeFile = File.ReadAllText(filePath);
            string[] lines = wholeFile.Split("\n");

            // remove all empty strings
            List<string> allLinesCleaned = new List<string>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    allLinesCleaned.Add(line.TrimEnd('\r'));
                }
            }
                                                                               
            if (allLinesCleaned[allLinesCleaned.Count - 1] != "</EditorBlockForConnections>")
            {
                throw new InvalidOperationException($"Special section in screen file {filePath} not found");
            }

            bool startOfSpecialSectionFound = false;
            int lineOfStartOfSpecialSection = 0;
            List<string> specialSectionLines = new List<string>();
            for (int i = allLinesCleaned.Count - 1; i > 0; i--)
            {
                if (allLinesCleaned[i] != "<EditorBlockForConnections>")
                {
                    specialSectionLines.Insert(0, $"{allLinesCleaned[i]}\n");
                }
                else
                {
                    startOfSpecialSectionFound = true;
                    specialSectionLines.Insert(0, $"{allLinesCleaned[i]}\n");
                    lineOfStartOfSpecialSection = i;
                    break;
                }
            }

            if (!startOfSpecialSectionFound)
            {
                throw new InvalidOperationException($"Beginning of special section in screen file {filePath} not found");
            }

            List<string> rootElementLines = new List<string>();
            for (int i = 0; i < lineOfStartOfSpecialSection; i++)
            {
                rootElementLines.Add($"{allLinesCleaned[i]}\n");
            }

            // get root element 
            string rootElementString = string.Join(String.Empty, rootElementLines);
            rootElement = (UIElement)XamlReader.Parse(rootElementString);


            try
            {
                LoadAllBindingsForScreen(specialSectionLines, rootElement);
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

            // Load simple bindings
            XmlNodeList bindingNodes = doc.GetElementsByTagName("Binding");
            ApplyAllBindingsForScreen(bindingNodes, rootElement);

            // Load commands
            XmlNodeList commandNodes = doc.GetElementsByTagName("Command");

            foreach (XmlNode commandNode in commandNodes)
            {
                // check a button with this name, select it
                var buttonName = commandNode.Attributes["UiName"].Value;
                if (buttonName is null || buttonName.Length == 0)
                {
                    throw new ArgumentException($"Error in Binding button command node {commandNode.OuterXml}");
                }

                Button bindedElement = (Button)LogicalTreeHelper.FindLogicalNode(rootElement, buttonName);
                if (bindedElement == null)
                {
                    throw new Exception($"Button with name \"{buttonName}\" doesn't exist in the screen");
                }

                // load and create all actions
                XmlNodeList actionNodes = doc.GetElementsByTagName("ButtonAction");
                List<ButtonAction> executeList = new List<ButtonAction>();
                foreach (XmlNode actionNode in actionNodes)
                {
                    var actionName = actionNode.Attributes["ActionName"].Value;
                    if (actionName is null || actionName.Length == 0)
                    {
                        throw new ArgumentException($"Error in Binding button action node {actionNode.OuterXml}");
                    }

                    if (!ButtonActions.Actions.ContainsKey(actionName))
                    {
                        throw new ArgumentException($"Unknown action with name {actionName}");
                    }

                    XmlNodeList propertiesNodes = doc.GetElementsByTagName("Property");
                    Dictionary<string, string> propertiesWithValues = new Dictionary<string, string>();
                    foreach (XmlNode property in propertiesNodes)
                    {
                        var propertyName = property.Attributes["Name"].Value;
                        var propertyValue = property.InnerText;
                        if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyValue))
                        {
                            throw new ArgumentException($"Error in Binding action property node {property.OuterXml}");
                        }

                        if (propertiesWithValues.ContainsKey(propertyName))
                        {
                            throw new ArgumentException($"Property {propertyName} already exists in action");
                        }

                        propertiesWithValues.Add(propertyName, propertyValue);
                    }

                    var newAction = (ButtonAction)ButtonActions.Actions[actionName].Clone();
                    newAction.Initialize(propertiesWithValues);
                    executeList.Add(newAction);
                }

                if (executeList.Count == 0)
                {
                    throw new InvalidOperationException($"Command has no actions inside in the command {commandNode.OuterXml}");
                }

                // create a command
                MultiActionCommand newCommand = new MultiActionCommand(executeList);

                // bind a commant to the button
                bindedElement.Command = newCommand;
            }

            // XAML on screen1 is done
            // Need to read parameters depended on action
            // Create better class for action creation:
            //      - static, contains list of parameters with names, method to create action with given params
            //      - here just check properties and call creation of action
            // Contain these classes in Dictionary





        }





        static void ApplyAllBindingsForScreen(XmlNodeList bindingNodes, UIElement rootElement)
        {
            string uiName = string.Empty;
            string propertyName = string.Empty;
            string signalName = string.Empty;
            Type uiType = null;
            foreach (XmlNode node in bindingNodes)
            {
                uiName = node.Attributes["UiName"].Value;
                propertyName = node.Attributes["PropertyName"].Value;
                signalName = node.Attributes["SignalName"].Value;

                if (uiName.Length == 0 || propertyName.Length == 0 || signalName.Length == 0)
                {
                    throw new ArgumentException($"Error in Binding node: {node.OuterXml}");
                }

                UIElement bindedElement = (UIElement)LogicalTreeHelper.FindLogicalNode(rootElement, uiName);

                if (bindedElement == null)
                {
                    // TODO add screen's name somehow
                    throw new Exception($"UI element with name \"{uiName}\" doesn't exist in the screen");
                }

                // Searching for dependency property
                // Since we have some special properties which cannot be found in element - create special cases for them
                DependencyProperty dp = null;
                switch (propertyName)
                {
                    case "Canvas.Top":
                        dp = Canvas.TopProperty;
                        break;
                    case "Canvas.Left":
                        dp = Canvas.LeftProperty;
                        break;
                    default:
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

                        dp = (DependencyProperty)field.GetValue(bindedElement);
                        break;
                }

                // Install the binding
                Binding myBinding = new Binding("TypedValue");
                myBinding.Source = SignalStorage.allNamedSignals[signalName];
                BindingOperations.SetBinding(bindedElement, dp, myBinding);
            }
        }

        static void ApplyAllBottonCommandsForScreen(XmlNodeList bindingNodes, UIElement rootElement)
        {
            /*  - new nodetype "Command"
             *  - there is a UiName
             *      - check this name belongs to button, if not - exception
             *  - PropertyName not necessary, we know this is Command
             *  - contains child nodes "Actions" - one command can do a lot of actions
             *  - Add new property "ActionType"
             *      - take an action from commands dictionary where they are stored
             *  
             *  - depending on command type try to find all required properties in this line
             *  - create Command object and add there this action with found parameters for execution
             *  - add this Command to the button
             *  
             *  
             *  ---------------------------------------
             *  Command example:
             *      
             *      Write value to device 
             *          - property Source local signal
             *          - property Target device signal
             *          
             *          - On execution write value from source to target
             *              find device in communication channel and call function "write one value" or smth
             *  
             *  
             * */








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
