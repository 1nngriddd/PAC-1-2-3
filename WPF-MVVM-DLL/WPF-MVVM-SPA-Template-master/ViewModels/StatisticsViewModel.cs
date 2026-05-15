using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPF_MVVM_SPA_Template.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private Customer _selectedCustomer;
        private ChartValues<double> _valorsFacturacio;

        public string CustomerName => $"{_selectedCustomer?.FirstName} {_selectedCustomer?.LastName}";

        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set { _seriesCollection = value; OnPropertyChanged(); }
        }

        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private int _selectedChartTypeIndex;
        public int SelectedChartTypeIndex
        {
            get => _selectedChartTypeIndex;
            set
            {
                if (_selectedChartTypeIndex != value)
                {
                    _selectedChartTypeIndex = value;
                    OnPropertyChanged();
                    ActualitzarTipusGrafic();
                }
            }
        }

        public ICommand BackCommand { get; }

        // ══════════════════════════════════════════════════════════════════
        //  COLORS DE LA PALETA  (es defineixen aquí perquè LiveCharts
        //  no pot llegir DynamicResource directament des de XAML)
        // ══════════════════════════════════════════════════════════════════
        // Or ambre  — "ColorCremaFosc" del tema
        private static readonly SolidColorBrush BrushAccent =
            new SolidColorBrush(Color.FromRgb(0xB9, 0x96, 0x68));

        // Verd olivaci mig — "ColorVerd" del tema
        private static readonly SolidColorBrush BrushVerd =
            new SolidColorBrush(Color.FromRgb(0x69, 0x78, 0x75));

        // Àrea de la gràfica de línies: crema semitransparent (#EDE2CC al 45%)
        private static readonly SolidColorBrush BrushAccentFill =
            new SolidColorBrush(Color.FromArgb(0x73, 0xED, 0xE2, 0xCC));

        // Àrea de barres (no s'usa com a fill, però es manté per coherència)
        private static readonly SolidColorBrush BrushVerdFill =
            new SolidColorBrush(Color.FromArgb(0x33, 0x69, 0x78, 0x75));

        public StatisticsViewModel(MainViewModel mainViewModel, Customer customer)
        {
            _mainViewModel   = mainViewModel;
            _selectedCustomer = customer;

            BackCommand = new RelayCommand(_ => _mainViewModel.NavegarALlista());

            GenerarDadesFicticies();
            ActualitzarTipusGrafic();
        }

        private void GenerarDadesFicticies()
        {
            _valorsFacturacio = new ChartValues<double>();

            if (_selectedCustomer.RendimentMensual == null || _selectedCustomer.RendimentMensual.Count == 0)
            {
                var rnd = new Random();
                var novesDades = new List<double>();
                for (int i = 0; i < 12; i++)
                    novesDades.Add(rnd.Next(2000, 20000));

                _selectedCustomer.RendimentMensual = novesDades;
                WPF_MVVM_SPA_Template.Services.DataService.SaveCustomers(
                    _mainViewModel.CustomerVM.Customers);
            }

            foreach (var valor in _selectedCustomer.RendimentMensual)
                _valorsFacturacio.Add(valor);

            Labels    = new[] { "Gen", "Feb", "Mar", "Abr", "Mai", "Jun",
                                 "Jul", "Ago", "Set", "Oct", "Nov", "Des" };
            Formatter = value => value.ToString("C");
        }

        private void ActualitzarTipusGrafic()
        {
            if (SelectedChartTypeIndex == 0)
            {
                // ── LÍNIES: traç ambre, farciment ambre transparent ──
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title       = "Facturació",
                        Values      = _valorsFacturacio,
                        Stroke      = BrushAccent,          // línia ambre
                        Fill        = BrushAccentFill,      // àrea semitransparent
                        PointGeometrySize = 8,
                        PointForeground   = BrushAccent,
                        LineSmoothness    = 0.4             // lleugera corba
                    }
                };
            }
            else
            {
                // ── BARRES: farciment verd olivaci, contorn ambre ──
                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title  = "Facturació",
                        Values = _valorsFacturacio,
                        Fill   = BrushVerd,                 // barres en verd olivaci
                        Stroke = BrushAccent,               // vora ambre
                        StrokeThickness = 1
                    }
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
