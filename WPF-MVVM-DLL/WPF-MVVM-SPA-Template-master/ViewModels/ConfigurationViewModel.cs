using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    public class ConfigurationViewModel : INotifyPropertyChanged
    {
        // 0 = LightTheme, 1 = DarkTheme
        private int _selectedThemeIndex = 0;
        public int SelectedThemeIndex
        {
            get => _selectedThemeIndex;
            set { _selectedThemeIndex = value; OnPropertyChanged(); }
        }

        public RelayCommand ApplyThemeCommand { get; set; }

        public ConfigurationViewModel()
        {
            ApplyThemeCommand = new RelayCommand(x => ExecutarCanvi());
        }

        private void ExecutarCanvi()
        {
            // Definim el fitxer segons la tria
            string themeName = SelectedThemeIndex == 0 ? "LightTheme.xaml" : "DarkTheme.xaml";

            // Creem la ruta cap a la carpeta Views/Themes/
            Uri uri = new Uri($"pack://application:,,,/Views/Themes/{themeName}", UriKind.Absolute);

            // Cridem al mètode d'App creat al Pas 1
            App.ChangeTheme(uri);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
