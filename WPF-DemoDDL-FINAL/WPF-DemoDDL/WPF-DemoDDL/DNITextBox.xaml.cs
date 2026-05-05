using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_DemoDDL
{
    public partial class DNITextBox : UserControl
    {

        // 1. DependencyProperty per al DNI (Permet el Binding Two-Way)
        public static readonly DependencyProperty DniProperty = DependencyProperty.Register(
            "Dni",
            typeof(string),
            typeof(DNITextBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDniChanged)); // Mètode que es dispara si el DNI canvia des del ViewModel

        public string Dni
        {
            get => (string)GetValue(DniProperty);
            set => SetValue(DniProperty, value);
        }

        // 2. DependencyProperty per a IsValid (permetrà saber des de fora si és vàlid)
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(
            "IsValid",
            typeof(bool),
            typeof(DNITextBox),
            new PropertyMetadata(false));

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            private set => SetValue(IsValidProperty, value);
        }

        // EXTRA: Propietat pel Placeholder
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(DNITextBox), new PropertyMetadata("Ex: 12345678A"));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        // DependencyProperty per al TOOLTIP
        public static readonly DependencyProperty TooltipMessageProperty = DependencyProperty.Register(
            "TooltipMessage", typeof(string), typeof(DNITextBox), new PropertyMetadata(string.Empty));

        public string TooltipMessage
        {
            get => (string)GetValue(TooltipMessageProperty);
            set => SetValue(TooltipMessageProperty, value);
        }

        public DNITextBox()
        {
            InitializeComponent();
            TooltipMessage = "Format: 8 números + 1 lletra";
        }

        // Mètode que es crida automàticament si el DNI canvia des del Binding
        private static void OnDniChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DNITextBox)d;
            var newValue = (string)e.NewValue;

            // Actualitzem el textbox visual sense provocar un bucle
            if (control.InnerTextBox.Text != newValue)
            {
                control.InnerTextBox.Text = newValue;
            }

            // Apliquem feedback visual
            control.ApplyVisualFeedback();
        }

        // Mètode que es crida quan l'usuari tecleja a la pantalla
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Actualitzem la DependencyProperty
            Dni = InnerTextBox.Text;

            // Mostrar o amagar el Placeholder segons si hi ha text
            PlaceholderLabel.Visibility = string.IsNullOrEmpty(Dni) ? Visibility.Visible : Visibility.Collapsed;
        }

        // LostFocus: mantenim per coherència amb la resta de controls
        private void InnerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // La validació ja és en temps real, no cal fer res especial
        }

        // ========================
        // VALIDACIÓ PROGRESSIVA
        // ========================
        // Lògica caràcter a caràcter adaptada al format DNI: 8 dígits + 1 lletra
        private void ApplyVisualFeedback()
        {
            string dni = Dni;

            // Camp buit: neutre
            if (string.IsNullOrWhiteSpace(dni))
            {
                IsValid = false;
                ResetVisuals();
                return;
            }

            int len = dni.Length;

            // REGLA 1: Si supera 9 caràcters → vermell immediatament
            if (len > 9)
            {
                IsValid = false;
                ShowError("Massa caràcters. Màxim: 8 números + 1 lletra");
                return;
            }

            // REGLA 2: Comprovem que els primers caràcters (fins a 8) siguin tots dígits
            int digitsToCheck = Math.Min(len, 8);
            for (int i = 0; i < digitsToCheck; i++)
            {
                if (!char.IsDigit(dni[i]))
                {
                    // Hi ha un caràcter no numèric en les posicions 1-8 → vermell
                    IsValid = false;
                    ShowError("Els 8 primers caràcters han de ser números");
                    return;
                }
            }

            // REGLA 3: Si encara no hem arribat a 9 caràcters i tots són dígits → neutre
            if (len < 9)
            {
                IsValid = false;
                ResetVisuals();
                return;
            }

            // REGLA 4: Tenim exactament 9 caràcters. El 9è ha de ser una lletra (a-z o A-Z)
            if (char.IsLetter(dni[8]))
            {
                // Format perfecte: 8 dígits + 1 lletra → VERD
                IsValid = true;
                ShowValid("DNI correcte ✓");
            }
            else
            {
                // El 9è caràcter no és una lletra → vermell
                IsValid = false;
                ShowError("El 9è caràcter ha de ser una lletra");
            }
        }

        // Feedback visual: Error (vermell)
        private void ShowError(string message)
        {
            InnerTextBox.BorderBrush = Brushes.Red;
            InnerTextBox.Background = new SolidColorBrush(Color.FromArgb(20, 255, 0, 0));
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = Brushes.Red;
            TooltipMessage = message;
        }

        // Feedback visual: Vàlid (verd)
        private void ShowValid(string message)
        {
            InnerTextBox.ClearValue(Control.BorderBrushProperty);
            InnerTextBox.Background = new SolidColorBrush(Color.FromArgb(20, 0, 180, 0));
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            TooltipMessage = message;
        }

        // Feedback visual: Neutre (per defecte)
        private void ResetVisuals()
        {
            InnerTextBox.ClearValue(Control.BorderBrushProperty);
            InnerTextBox.ClearValue(Control.BackgroundProperty);
            ValidationMessage.Text = "Format requerit: 8 números i 1 lletra.";
            ValidationMessage.Foreground = (Brush)FindResource("ValidationTextBrush");
            TooltipMessage = "Format: 8 números + 1 lletra";
        }

        // Algoritme real per calcular la lletra del DNI (conservat per ús futur)
        private bool CheckDniLetter(string dni)
        {
            try
            {
                string numbers = dni.Substring(0, 8);
                string letter = dni.Substring(8, 1).ToUpper();

                string validLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
                int numbersInt = int.Parse(numbers);
                int index = numbersInt % 23;

                string expectedLetter = validLetters[index].ToString();

                return letter == expectedLetter;
            }
            catch
            {
                return false;
            }
        }
    }
}