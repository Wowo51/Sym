//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Win32;

namespace SymRegressionApp.FileHelper
{
    public class FileIO
    {
        public static string SaveText(string inText)
        {
            string path = GetSavePath("text file", "txt");
            if (path != null)
            {
                System.IO.File.WriteAllText(path, inText);
            }
            return path;
        }

        public static string OpenText()
        {
            string path = GetOpenPath("text file", "txt");
            if (path != null)
            {
                return System.IO.File.ReadAllText(path);
            }
            return null;
        }

        public static string OpenCsv()
        {
            string path = GetOpenPath("csv file", "csv");
            if (path != null)
            {
                return System.IO.File.ReadAllText(path);
            }
            return null;
        }

        public static string SaveCsv(string inText)
        {
            string path = GetSavePath("csv file", "csv");
            if (path != null)
            {
                System.IO.File.WriteAllText(path, inText);
            }
            return path;
        }

        public static string GetSavePath(string filter)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = filter;
            if (saveFileDialog1.ShowDialog() == true)
            {
                return saveFileDialog1.FileName;
            }
            return null;
        }

        public static string GetSavePath(string text, string extension)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = BuildFilter(text, extension);
            if (saveFileDialog1.ShowDialog() == true)
            {
                return saveFileDialog1.FileName;
            }
            return null;
        }

        public static string GetOpenPath(string filter)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = filter;
            if (openFileDialog1.ShowDialog() == true)
            {
                return openFileDialog1.FileName;
            }
            return null;
        }

        public static string GetOpenPath(string text, string extension)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = BuildFilter(text, extension);
            if (openFileDialog1.ShowDialog() == true)
            {
                return openFileDialog1.FileName;
            }
            return null;
        }

        public static string[] GetOpenPaths(string text, string extension)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = BuildFilter(text, extension);
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == true)
            {
                return openFileDialog1.FileNames;
            }
            return null;
        }

        //public static string[] GetTexts(string text, string extension)
        //{
        //    string[] paths = GetOpenPaths(text, extension);
        //    if (paths==null)
        //    {
        //        return null;
        //    }
        //    return Maps.Map(paths, x => System.IO.File.ReadAllText(x));
        //}

        //public static string GetOpenPath(string[] textStrings, string[] extensions)
        //{
        //    OpenFileDialog openFileDialog1 = new OpenFileDialog();
        //    openFileDialog1.Filter = BuildFilter(textStrings, extensions);
        //    if (openFileDialog1.ShowDialog() == true)
        //    {
        //        return openFileDialog1.FileName;
        //    }
        //    return null;
        //}

        public static string BuildFilter(string text, string extension)
        {
            return text + " (*." + extension + ")|*." + extension + ";";
        }

        //public static string BuildFilter(string[] textStrings, string[] extensions)
        //{
        //    string[] filters = Functional.Maps.Map(textStrings, extensions, BuildFilter);
        //    return string.Join("|", filters);
        //}


    }
}
