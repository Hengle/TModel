using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TModel
{
    public static class Preferences
    {
        // Folder for storing user settings, Fortnite mappings and other data for the software.
        public static string StorageFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TModel");

        public static string GameDirectory { get; set; } = @"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks";


        // TODO: add reading and writing functionality preferably in binary.
        public static void Save()
        {

        }

        public static void Read()
        {

        }
    }
}
