using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using WPF_MVVM_SPA_Template.Models;
using WPF_MVVM_SPA_Template.Views;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ── ViewModels fills ──────────────────────────────────────────────
        public HomeViewModel HomeVM { get; set; }
        public CustomerViewModel CustomerVM { get; set; }

        // ── Vista actual ──────────────────────────────────────────────────
        private object? _currentView;
        public object? CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        // ── Índex del sidebar ─────────────────────────────────────────────
        private int? _selectedIndex;
        public int? SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                ChangeView();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  POPUP ── Millor Client
        // ══════════════════════════════════════════════════════════════════

        // Controla si el overlay és visible
        private bool _isPopupVisible;
        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set { _isPopupVisible = value; OnPropertyChanged(); }
        }

        // Títol gran del popup (nom del client)
        private string _popupTitle = string.Empty;
        public string PopupTitle
        {
            get => _popupTitle;
            set { _popupTitle = value; OnPropertyChanged(); }
        }

        // Missatge de detall (facturació total, etc.)
        private string _popupMessage = string.Empty;
        public string PopupMessage
        {
            get => _popupMessage;
            set { _popupMessage = value; OnPropertyChanged(); }
        }

        // Comanda del botó de tancar (X)
        public RelayCommand ClosePopupCommand { get; set; }

        // Mètode públic que crida CustomerViewModel per mostrar el popup
        public void MostrarPopup(string titol, string missatge)
        {
            PopupTitle   = titol;
            PopupMessage = missatge;
            IsPopupVisible = true;
        }

        // ══════════════════════════════════════════════════════════════════

        public MainViewModel()
        {
            HomeVM     = new HomeViewModel();
            CustomerVM = new CustomerViewModel(this);

            ClosePopupCommand = new RelayCommand(_ => IsPopupVisible = false);

            SelectedIndex = 0;
        }

        private void ChangeView()
        {
            switch (SelectedIndex)
            {
                case 0:
                    var viewHome = new HomeView();
                    viewHome.DataContext = HomeVM;
                    CurrentView = viewHome;
                    break;
                case 1:
                    var viewList = new CustomerView();
                    viewList.DataContext = CustomerVM;
                    CurrentView = viewList;
                    break;
                case 2:
                    var configVM = new ConfigurationViewModel();
                    CurrentView = new ConfigurationView { DataContext = configVM };
                    break;
                default:
                    CurrentView = null;
                    break;
            }
        }

        public void NavegarALlista()
        {
            var view = new CustomerView();
            view.DataContext = CustomerVM;
            CurrentView = view;
        }

        public void NavegarAFormulari()
        {
            CurrentView = new CustomerFormView { DataContext = CustomerVM };
        }

        public void NavegarAGrafica(Customer client)
        {
            var vmGrafica = new StatisticsViewModel(this, client);
            CurrentView = new StatisticsView { DataContext = vmGrafica };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
