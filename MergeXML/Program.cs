using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Xsl;


// Created By: Ghiath Al-Qaisi
// Date: 2016-08-27
// Description: Merge multiple XML files into one file

namespace MergeXML
{
    class Program
    {
        // Variables to store arguments input values.
        public static string XmlFilePath;
        public static string DestinationFileName;
        public static string XmlNodePath;
        public static string XsltFileName;

        static int Main(string[] args)
        {
            // Parameters have to be provided.
            if (!args.Any())
            {
                Console.WriteLine();
                Console.WriteLine(@"Usage: MergeXML.exe [Mandatory: files Path] [Mandatory: Destination File Name] [Mandatory: XML node path] [Optional: xslt file path]");
                Console.WriteLine("Example: MergeXML.exe \"C:\\FilesToMerge\" \"C:\\FilesToMerge\\ResultFile.xml\" \"OrderList\" \"XSLT/XSLTIndent.xslt\"");
                Console.WriteLine();
                return 1;
            }

            // Show help
            if (args[0] == "-h" || args[0] == "-help" || args[0] == "help")
            {
                Console.WriteLine();
                Console.WriteLine(@"Usage: MergeXML.exe [Mandatory: files Path] [Mandatory: Destination File Name] [Mandatory: XML node path] [Optional: xslt file path]");
                Console.WriteLine("Example: MergeXML.exe \"C:\\FilesToMerge\" \"C:\\FilesToMerge\\ResultFile.xml\" \"OrderList\" \"XSLT/XSLTIndent.xslt\"");
                Console.WriteLine();
                return 0;
            }

            // Test if input arguments were supplied:
            if (args.Length < 3)
            {
                Console.WriteLine();
                Console.WriteLine(@"Please enter the required parameters:");
                Console.WriteLine(@"Usage: MergeXML.exe [Mandatory: files Path] [Mandatory: Destination File Name] [Mandatory: XML node path] [Optional: xslt file path]");
                Console.WriteLine("Example: MergeXML.exe \"C:\\FilesToMerge\" \"C:\\FilesToMerge\\ResultFile.xml\" \"OrderList\" \"XSLT/XSLTIndent.xslt\"");
                Console.WriteLine();
                return 1;
            }

            // Assign input parameter values
            XmlFilePath = args[0];
            if (!Directory.Exists(XmlFilePath))
            {
                Print("The provided file path does not exists..!", 'e');
                return 1;
            }

            DestinationFileName = args[1];
            XmlNodePath = args[2];

            if (args.Length > 3)
            {
                XsltFileName = args[3];

                if (!File.Exists(XsltFileName))
                {
                    Print("The provided XSLT file path does not exists..!", 'e');
                    return 1;
                }
            }

            // Remove the slash at the end of the path string if there where any
            var fullPath = XmlFilePath.TrimEnd(Path.DirectorySeparatorChar);

            try
            {
                // Get the file name list
                var xmlFiles = Directory.EnumerateFiles(fullPath, "*.xml");

                // Check if there were files to merege in the provided path
                var files = xmlFiles as string[] ?? xmlFiles.ToArray();
                if (!files.Any())
                {
                    Print("No files to be merged found..!", 'e');
                    return 1;
                }

                Print(files.Count() + " files to merge found..!", 's');
                
                // Set the first file as main file to merge other files into it
                var mainFileName = files.FirstOrDefault();
                Print(mainFileName + " Sat as Main file..!", 'i');
                
                // Clean up the main file.
                CleanupXmlFile(mainFileName);
                Print(mainFileName + " File Cleaned...", 'o');

                // Transform the main file if XSLT file has been provided.
                if (!string.IsNullOrEmpty(XsltFileName))
                {
                    XsltTransformXml(mainFileName);
                    Print(mainFileName + " File Transformed...", 'o');
                }
                
                // Load the main file.
                var mainXml = XDocument.Load(mainFileName);
                Print("File " + mainFileName + " Loaded...", 'o');

                // Check if the provided node path exists in the main file
                if (!mainXml.Descendants(XmlNodePath).Nodes().Any())
                {
                    Print("The \"" + XmlNodePath + "\" Has not been found in the loaded file..!", 'e');
                    return 1;
                }

                var mergeFile = new XDocument();
                
                // Loop through the other files in folder to merge
                foreach (var currentFile in files.Where(f => f != files.FirstOrDefault()))
                {
                    Print("Merging " + currentFile + " File...", 'i');
                    
                    // Cleanup the file before merging it.
                    CleanupXmlFile(currentFile);
                    Print(currentFile + " File Cleaned...", 'o');

                    // Transform the current file if XSLT file has been provided
                    if (!string.IsNullOrEmpty(XsltFileName))
                    {
                        XsltTransformXml(currentFile);
                        Print(currentFile + " File Transformed...", 'o');
                    }

                    // Load the current file
                    mergeFile = XDocument.Load(currentFile);
                    Print(currentFile + " File Loaded...", 'o');

                    // add nodes if they exists in the current file
                    mainXml.Descendants(XmlNodePath).LastOrDefault()?.Add(mergeFile.Descendants(XmlNodePath).Nodes());
                    Print(currentFile + " File Merged...", 'o');

                    // Save the resulted file after merge
                    mainXml.Save(DestinationFileName);
                    Print("Changes Saved...", 'o');
                }
            }
            catch (Exception e)
            {
                Print(e.Message, 'e');
                return 1;
            }

            Print("Done!", 's');
            return 0;
        }

        //re-create the file making sure that xml declaration is the first char in file.
        public static void CleanupXmlFile(string filePath)
        {
            Console.WriteLine("Cleaning " + filePath + " File...");
            var lines = new List<string>();

            // Read the file line by line into list
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (TextReader reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }

            // Find the line containing the XML Definition
            var i = lines.FindIndex(s => s.StartsWith("<?xml"));

            // Move the xml definition line to top of the file
            var xmlLine = lines[i].Trim();
            lines.RemoveAt(i);
            lines.Insert(0, xmlLine);

            // Write the file back to disk
            using (var fileStream = File.Open(filePath, FileMode.Truncate, FileAccess.Write))
            using (TextWriter writer = new StreamWriter(fileStream))
            {
                foreach (var line in lines)

                    writer.Write(line.Trim());

                writer.Flush();
            }
        }

        // Transform XML file using the provided XSLT file.
        public static void XsltTransformXml(string filePath)
        {
            Console.WriteLine("Transforming " + filePath + " File...");
            var myXslTransform = new XslTransform();

            // Load the XSLT File
            myXslTransform.Load(XsltFileName);

            // Transform the XML file
            myXslTransform.Transform(filePath, filePath);
        }

        // Custom console text printer
        public static void Print(string text, char type)
        {
            // i: Information: Magenta
            // e: Error: Red
            // s: Success: Green

            switch (type)
            {
                case 'i':
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case 'e':
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case 's':
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                default:
                    Console.WriteLine(text);
                    break;
            }
        }
    }
}
