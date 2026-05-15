using System.Configuration;
using System.Data;
using System.Windows;
using System;

namespace WPF_MVVM_SPA_Template
{
    public partial class App : Application
    {
        public static void ChangeTheme(Uri themeUri)
        {
            // Accedim als diccionaris de recursos de l'App
            var dicts = Application.Current.Resources.MergedDictionaries;

            // Creem el nou diccionari des del fitxer XAML
            ResourceDictionary newTheme = new ResourceDictionary { Source = themeUri };

            // Substituïm el tema actual (index 0) pel nou
            dicts.Clear();
            dicts.Add(newTheme);
        }
    }
}
