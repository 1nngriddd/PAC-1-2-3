using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_DemoDDL
{
    [ToolboxItem(true)]
    public partial class EmailTextBox : UserControl
    {
        public EmailTextBox()
        {
            InitializeComponent();
            TooltipMessage = "Introdueix el teu correu electrònic";
        }

        // 1. DependencyProperty per l'EMAIL (Requisit obligatori)
        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(EmailTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Email
        {
            get => (string)GetValue(EmailProperty);
            set => SetValue(EmailProperty, value);
        }

        // 2. DependencyProperty per al TOOLTIP (Punt extra)
        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register("TooltipMessage", typeof(string), typeof(EmailTextBox),
            new PropertyMetadata(string.Empty));

        public string TooltipMessage
        {
            get => (string)GetValue(TooltipMessageProperty);
            set => SetValue(TooltipMessageProperty, value);
        }

        // 3. Propietat IsValid (Requisit obligatori per saber si està bé)
        public bool IsValid { get; private set; }

        // 4. Propietat Placeholder
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(EmailTextBox), new PropertyMetadata("Ex: usuari@empresa.com"));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        // LostFocus: validació completa en sortir
        private void InternalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyVisualFeedback(fromLostFocus: true);
        }

        // TextChanged: validació progressiva en temps real
        private void InternalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderLabel != null) 
            {
                PlaceholderLabel.Visibility = string.IsNullOrEmpty(InternalTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            ApplyVisualFeedback(fromLostFocus: false);
        }

        // ========================
        // VALIDACIÓ PROGRESSIVA
        // ========================
        private void ApplyVisualFeedback(bool fromLostFocus)
        {
            string email = Email;

            // Camp buit: neutre
            if (string.IsNullOrEmpty(email))
            {
                TooltipMessage = "Introdueix el teu correu electrònic";
                IsValid = false;
                ResetVisuals();
                return;
            }

            // 1. Email COMPLET i vàlid → VERD
            if (IsValidEmail(email))
            {
                TooltipMessage = "Email correcte.";
                IsValid = true;
                ShowValid("Email correcte ✓");
                return;
            }

            // 2. No és complet
            IsValid = false;

            if (!IsValidPartialEmail(email))
            {
                // Patró clarament erroni (espais, doble @, caràcters invàlids...) → VERMELL
                TooltipMessage = "Email no vàlid. Format esperat: usuari@domini.com";
                ShowError("Caràcters no vàlids detectats");
            }
            else if (fromLostFocus)
            {
                // Ha sortit del camp amb un email incomplet → VERMELL
                TooltipMessage = "Email no vàlid. Format esperat: usuari@domini.com";
                ShowError("Email incomplet. Esperat: usuari@domini.com");
            }
            else
            {
                // Format parcial correcte, encara escrivint → NEUTRE
                TooltipMessage = "Introdueix el teu correu electrònic";
                ResetVisuals();
            }
        }

        // Comprova si el text PODRIA arribar a ser un email vàlid
        private bool IsValidPartialEmail(string email)
        {
            if (email.Contains(' ')) return false;
            var validChars = new Regex(@"^[\w\.\-\+@]+$");
            if (!validChars.IsMatch(email)) return false;
            if (email.StartsWith("@")) return false;
            int atCount = email.Split('@').Length - 1;
            if (atCount > 1) return false;
            if (email.Contains("..")) return false;
            if (email.StartsWith(".")) return false;

            // Si hi ha @ i un punt al domini, comprovem que el TLD no superi 4 caràcters
            int atIndex = email.IndexOf('@');
            if (atIndex >= 0)
            {
                string domainPart = email.Substring(atIndex + 1);
                int lastDot = domainPart.LastIndexOf('.');
                if (lastDot >= 0)
                {
                    string tld = domainPart.Substring(lastDot + 1);
                    if (tld.Length > 4) return false; // TLD massa llarg (màxim: .com, .info, .name)
                }
            }

            return true;
        }

        // Feedback visual: Error (vermell) — sense canviar gruix per evitar moviment
        private void ShowError(string message)
        {
            InternalTextBox.BorderBrush = Brushes.Red;
            InternalTextBox.Background = new SolidColorBrush(Color.FromArgb(20, 255, 0, 0));
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = Brushes.Red;
        }

        // Feedback visual: Vàlid (verd)
        private void ShowValid(string message)
        {
            InternalTextBox.ClearValue(Control.BorderBrushProperty);
            InternalTextBox.Background = new SolidColorBrush(Color.FromArgb(20, 0, 180, 0));
            ValidationMessage.Text = message;
            ValidationMessage.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
        }

        // Feedback visual: Neutre
        private void ResetVisuals()
        {
            InternalTextBox.ClearValue(Control.BorderBrushProperty);
            InternalTextBox.ClearValue(Control.BackgroundProperty);
            ValidationMessage.Text = "Introdueix una adreça de correu vàlida.";
            ValidationMessage.Foreground = (Brush)FindResource("ValidationTextBrush");
        }

        // Expressió regular per a email complet
        private bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(email);
        }
    }
}