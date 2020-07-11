//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace SymRegressionApp.FileHelper
{
    public abstract class FileIOBase<T>
    {
        public OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
        public SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
        public string LastUsedFileName = "";

        //T tNull;

        public bool Open(ref T inData)
        {
            if (OpenFileDialog1.ShowDialog() == true)
            {
                LastUsedFileName = OpenFileDialog1.FileName;
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    inData = OpenWithKnownFilename(inData.GetType(), OpenFileDialog1.FileName);
                }
                catch
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("File Open failed.  The file might be open with another program or it is corrupt.");
                    return false;
                }
                Mouse.OverrideCursor = null;
                return true;
            }
            return false;
        }

        public void Save(ref T InApp)
        {
            Save(ref InApp, LastUsedFileName);
        }

        public void Save(ref T InApp, string path)
        {
            if (path == "")
            {
                SaveAs(ref InApp);
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    SaveWithKnownFilename(InApp, path);
                }
                catch
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("File Save failed.  The file might be open with another program.");
                }
                Mouse.OverrideCursor = null;
            }

        }

        public void SetFilters(string text, string extension)
        {
            string filter = FileIO.BuildFilter(text, extension);
            OpenFileDialog1.Filter = filter;
            SaveFileDialog1.Filter = filter;
        }

        public void SaveAs(ref T InApp)
        {
            if (SaveFileDialog1.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    SaveWithKnownFilename(InApp, SaveFileDialog1.FileName);
                }
                catch
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("File Save failed.  The file might be open with another program.");
                }
                Mouse.OverrideCursor = null;
            }
            LastUsedFileName = SaveFileDialog1.FileName;
            return;
        }

        public abstract T OpenWithKnownFilename(Type appType, string FileName);
        public abstract void SaveWithKnownFilename(T InApp, string FileName);
    }
}
