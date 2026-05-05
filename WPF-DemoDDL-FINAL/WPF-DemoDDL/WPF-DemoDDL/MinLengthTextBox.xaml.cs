using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace WPF_DemoDDL
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [ToolboxItem(true)]
    public partial class MinLengthTextBox : UserControl
    {
        // Flag Hybrid: indica si l'usuari ja ha sortit del camp alguna vegada
        private bool _hasBeenTouched = false;

        public MinLengthTextBox()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Actualitzem el text inicial amb el MinLength real configurat
                ValidationMessage.Text = $"Mínim {MinLength} caràcters";
            };
        }
        // DependencyProperty per al Text (Propietat per a WPF)

        // Si la propietat no es DependencyProperty, no es pot fer el binding a la propietat Text del control. Això és perquè les propietats dependents permeten que el sistema de lligams de dades (data binding) de WPF funcioni correctament, i també permeten que les propietats siguin notificades quan canvien, cosa que és essencial per a la funcionalitat del control.
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", //nom de la propietat, per a no liarse amb el nom de la variable, ficar el mateix nom amb property radere. TextProperty
            typeof(string), //tipus de la propietat
            typeof(MinLengthTextBox), //Tipus del objecte que la conte
            new PropertyMetadata(string.Empty));  //valor per defecte

        public string Text
        {
            get { return (string)GetValue(TextProperty); } //Obtenim el valor de la propietat Text
            set { SetValue(TextProperty, value); } //Assignem el valor a la propietat Text
        }

        // DependencyProperty per a la màscara (Propietat per a WPF), en aquest cas per a la longitud mínima
        public static readonly DependencyProperty MinLengthProperty = DependencyProperty.Register(
            "MinLength",
            typeof(int),
            typeof(MinLengthTextBox),
            new PropertyMetadata(2)); //Per defecte demanarem 2 caràcters
        public int MinLength
        {           //despres de crear la propietat dependent, es crea la propietat
            get { return (int)GetValue(MinLengthProperty); }
            set { SetValue(MinLengthProperty, value); }
        }

        // DependencyProperty per al Placeholder
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(MinLengthTextBox), new PropertyMetadata(string.Empty));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        // HYBRID: Quan l'usuari surt del camp per primer cop, activem la validació completa
        private void InternalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _hasBeenTouched = true;
            ApplyVisualFeedback();
        }

        // Signatura de l'esdeveniment. S'executa CADA COP que el text canvia dins del TextBox
        private void InternalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Mostrar o amagar el Placeholder
            PlaceholderLabel.Visibility = string.IsNullOrEmpty(InternalTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ApplyVisualFeedback();
        }

        // HYBRID: Lògica visual centralitzada
        private void ApplyVisualFeedback()
        {
            string currentText = InternalTextBox.Text;

            // Camp buit: sempre neutre (sense color)
            if (string.IsNullOrEmpty(currentText))
            {
                ResetVisuals($"Mínim {MinLength} caràcters");
                return;
            }

            bool isValid = currentText.Trim().Length >= MinLength;

            if (isValid)
            {
                // Cas CORRECTE: fons verd claret (sempre, en temps real)
                InternalTextBox.ClearValue(Control.BorderBrushProperty);
                InternalTextBox.Background = new SolidColorBrush(Color.FromArgb(20, 0, 180, 0));
                ValidationMessage.Text = "Correcte ✓";
                ValidationMessage.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            }
            else if (_hasBeenTouched)
            {
                // Cas INCORRECTE i ja ha estat "tocat": vora vermella
                InternalTextBox.BorderBrush = Brushes.Red;
                InternalTextBox.Background = new SolidColorBrush(Color.FromArgb(20, 255, 0, 0));
                ValidationMessage.Text = $"Falten {MinLength - currentText.Trim().Length} caràcters";
                ValidationMessage.Foreground = Brushes.Red;
            }
            else
            {
                // Cas INCORRECTE però encara NO ha estat "tocat": neutre (sense vermell)
                ResetVisuals($"Mínim {MinLength} caràcters");
            }
        }

        private void ResetVisuals(string message = "")
        {
            InternalTextBox.ClearValue(Control.BorderBrushProperty);
            InternalTextBox.ClearValue(Control.BackgroundProperty);
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = (Brush)FindResource("ValidationTextBrush");
        }

        private void InternalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex curta: \p{L} inclou totes les lletres amb accents, ñ, ç, etc.
            Regex regex = new Regex(@"^[\p{L}\s]+$");

            // Si el caràcter premut no coincideix amb el patró bloquegem l'entrada
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        // Bloquejar espai com a primer caràcter (PreviewTextInput no captura espais en WPF)
        private void InternalTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && string.IsNullOrEmpty(InternalTextBox.Text))
            {
                e.Handled = true;
            }
        }

    }

}
