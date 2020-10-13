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
        /// <summary>
        /// Common method to open the Folder Selector Dialog. 
        /// </summary>
        /// <param name="title">Title for the dialog. In case of Destination folder selection it will be different from source folder selection</param>
        /// <param name="currentPath">If the Source Path or Destination Path have been previously selected by user, then open the dialog from same location</param>
        /// <returns>Source or Destination path string, which will be consumed elsewhere</returns>
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
