using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IFCConverto.Services
{
    public class IOService
    {
        public string OpenFolderPicker(string title, string currentPath)
        {
            var sourcePath = string.Empty;

            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.EnsureReadOnly = true;
            dialog.Multiselect = false;           
            dialog.Title = title;

            if (string.IsNullOrEmpty(currentPath))
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                dialog.InitialDirectory = currentPath;
            }

            CommonFileDialogResult result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok)
            {
                sourcePath = dialog.FileName;
            }     
            else
            {
                sourcePath = currentPath;
            }

            return sourcePath;
        }
    }
}
