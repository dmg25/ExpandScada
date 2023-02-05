using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

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
                var foundScreen = LoadUiElement(file);
                screens.Add(Path.GetFileNameWithoutExtension(file), foundScreen);
            }


        }

        static UIElement LoadUiElement(string filePath)
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
                // TODO exception ?
                return null;
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
                specialBlockLines.Add(lines[i]);
            }



            return rootElement;
        }



    }
}
