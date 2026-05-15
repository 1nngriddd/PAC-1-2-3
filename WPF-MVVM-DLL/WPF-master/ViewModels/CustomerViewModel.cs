using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WPF_MVVM_SPA_Template.Models;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    public class CustomerViewModel : INotifyPropertyChanged
    {
        private Customer? _originalCustomer;
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>();

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private Customer _currentCustomer;
        public Customer CurrentCustomer
        {
            get { return _currentCustomer; }
            set { _currentCustomer = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCustomerCommand { get; set; }
        public RelayCommand DelCustomerCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand EditCustomerCommand { get; set; }
        public RelayCommand ViewStatisticsCommand { get; set; }
        public RelayCommand ExportPdfCommand { get; set; }
        public RelayCommand ShowBestCustomerCommand { get; set; }

        public CustomerViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            CurrentCustomer = new Customer();
            Customers = Services.DataService.LoadCustomers();

            AddCustomerCommand = new RelayCommand(_ => PrepararAlta());
            EditCustomerCommand = new RelayCommand(x => EditarClient(x as Customer));
            DelCustomerCommand = new RelayCommand(x => DelCustomer(x as Customer));
            SaveCommand = new RelayCommand(_ => GuardarClient());
            CancelCommand = new RelayCommand(_ => CancelarEdicio());
            ViewStatisticsCommand = new RelayCommand(x => VeureGrafica(x as Customer));
            ShowBestCustomerCommand = new RelayCommand(_ => MostrarMillorClient());
            ExportPdfCommand = new RelayCommand(_ => ExportarPDF());
        }

        private void MostrarMillorClient()
        {
            if (!Customers.Any()) { MessageBox.Show("No hi ha clients registrats."); return; }

            var millor = Customers.OrderByDescending(c => c.RendimentMensual?.Sum() ?? 0).First();
            double total = millor.RendimentMensual?.Sum() ?? 0;

            string titol = $"🏆  {millor.FirstName} {millor.LastName}";
            string missatge = total > 0
                ? $"Facturació total acumulada: {total:C}\n(mitjana mensual: {total / 12:C})"
                : "Aquest client encara no té dades de facturació registrades.";

            _mainViewModel.MostrarPopup(titol, missatge);
        }

        private void PrepararAlta()
        {
            _originalCustomer = null;
            CurrentCustomer = new Customer { DataAlta = DateTime.Now };
            _mainViewModel.NavegarAFormulari();
        }

        private void EditarClient(Customer? client)
        {
            if (client != null)
            {
                _originalCustomer = client;
                CurrentCustomer = client.Clone();
                _mainViewModel.NavegarAFormulari();
            }
        }

        private void GuardarClient()
        {
            if (CurrentCustomer == null) CurrentCustomer = new Customer();

            if (string.IsNullOrWhiteSpace(CurrentCustomer.FirstName) || CurrentCustomer.FirstName.Length < 3)
            { MessageBox.Show("El nom ha de tenir almenys 3 caràcters."); return; }
            if (string.IsNullOrWhiteSpace(CurrentCustomer.LastName) || CurrentCustomer.LastName.Length < 3)
            { MessageBox.Show("Els cognoms han de tenir almenys 3 caràcters."); return; }
            if (string.IsNullOrWhiteSpace(CurrentCustomer.Dni))
            { MessageBox.Show("El DNI és obligatori."); return; }
            if (string.IsNullOrEmpty(CurrentCustomer.Phone) || !Regex.IsMatch(CurrentCustomer.Phone, @"^\d{9}$"))
            { MessageBox.Show("El telèfon ha de tenir 9 dígits numèrics."); return; }
            if (string.IsNullOrEmpty(CurrentCustomer.Email) || !Regex.IsMatch(CurrentCustomer.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { MessageBox.Show("El format del correu electrònic no és vàlid."); return; }

            if (_originalCustomer == null)
            {
                int nouId = 1;
                while (Customers.Any(c => c.Id == nouId)) nouId++;
                CurrentCustomer.Id = nouId;
                Customers.Add(CurrentCustomer);
                MessageBox.Show("Nou client afegit amb èxit.");
            }
            else
            {
                _originalCustomer.CopyFrom(CurrentCustomer);
                MessageBox.Show("S'han guardat els canvis del client.");
            }

            WPF_MVVM_SPA_Template.Services.DataService.SaveCustomers(Customers);
            _mainViewModel.NavegarALlista();
        }

        private void CancelarEdicio() => _mainViewModel.NavegarALlista();

        private void DelCustomer(Customer? client)
        {
            var target = client ?? SelectedCustomer;
            if (target != null)
            {
                Customers.Remove(target);
                WPF_MVVM_SPA_Template.Services.DataService.SaveCustomers(Customers);
            }
        }

        private void VeureGrafica(Customer? client)
        {
            if (client != null) _mainViewModel.NavegarAGrafica(client);
            else MessageBox.Show("Si us plau, selecciona un client de la llista.");
        }

        // ══════════════════════════════════════════════════════════════════
        //  EXPORTAR PDF
        // ══════════════════════════════════════════════════════════════════
        private void ExportarPDF()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string rutaInforme = System.IO.Path.Combine(baseDir, "CustomerReport.frx");
                if (!System.IO.File.Exists(rutaInforme))
                {
                    MessageBox.Show("No s'ha trobat CustomerReport.frx.\n\nRuta:\n" + rutaInforme,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Customers.Any())
                {
                    MessageBox.Show("No hi ha clients per exportar. Afegeix algun client primer.",
                        "Sense dades", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string rutaPDF = System.IO.Path.Combine(baseDir, "CustomerReport.pdf");

                // Codi per carregar l'informe i afegir dades des de memòria (seguint el professor)
                Report report = new Report();
                report.Load(rutaInforme);

                // Assignem la llista de clients al DataSource de l'informe anomenat "Customers"
                report.RegisterData(Customers.ToList(), "Customers");

                // Habilitem el DataSource
                DataSourceBase dataSource = report.GetDataSource("Customers");
                dataSource.Enabled = true;

                // Vinculem el DataSource a la banda de dades
                DataBand? dataBand = report.FindObject("Data1") as DataBand;
                if (dataBand != null)
                    dataBand.DataSource = dataSource;

                // Vinculem el DataSource al total de clients
                Total? total = report.FindObject("TotalClients") as Total;
                if (total != null)
                    total.Evaluator = dataBand;

                // Preparem l'informe i l'exportem a PDF
                report.Prepare();
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    PDFSimpleExport pdfExport = new PDFSimpleExport();
                    report.Export(pdfExport, ms);
                    System.IO.File.WriteAllBytes(rutaPDF, ms.ToArray());
                }

                // Millora UX: obrim el PDF automàticament amb el visor del sistema
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(rutaPDF) { UseShellExecute = true });

                MessageBox.Show("Informe exportat correctament!\n\nDesat a: " + rutaPDF,
                    "Èxit", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}