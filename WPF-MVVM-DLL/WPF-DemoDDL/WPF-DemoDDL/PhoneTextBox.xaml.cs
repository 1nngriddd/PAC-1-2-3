using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF_DemoDDL
{
    [ToolboxItem(true)]
    public partial class PhoneTextBox : UserControl
    {
        public PhoneTextBox()
        {
            InitializeComponent();
            TooltipMessage = "Format: 9-15 dígits. Prefix + opcional.";
        }

        // 1. DependencyProperty per al TELÈFON
        public static readonly DependencyProperty PhoneNumberProperty =
            DependencyProperty.Register("PhoneNumber", typeof(string), typeof(PhoneTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string PhoneNumber
        {
            get => (string)GetValue(PhoneNumberProperty);
            set => SetValue(PhoneNumberProperty, value);
        }

        // 2. DependencyProperty per al TOOLTIP
        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register("TooltipMessage", typeof(string), typeof(PhoneTextBox),
            new PropertyMetadata(string.Empty));

        public string TooltipMessage
        {
            get => (string)GetValue(TooltipMessageProperty);
            set => SetValue(TooltipMessageProperty, value);
        }

        // 3. Propietat Placeholder
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(PhoneTextBox), new PropertyMetadata("Ex: +34 600 000 000"));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public bool IsValid { get; private set; }

        // FILTRATGE: Només dígits i + (el + només al principi)
        private void InternalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permetre + només com a primer caràcter
            if (e.Text == "+")
            {
                if (InternalTextBox.Text.Length > 0 || InternalTextBox.CaretIndex > 0)
                {
                    e.Handled = true; // + només al principi
                }
                return;
            }

            // Només dígits
            if (!Regex.IsMatch(e.Text, @"^[0-9]+$"))
            {
                e.Handled = true;
            }
        }

        // Bloquejar espai (PreviewTextInput no captura espais en WPF)
        private void InternalTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // No permetem espais al telèfon
            }
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
        // Format internacional flexible: +? seguit de 9-15 dígits
        private void ApplyVisualFeedback(bool fromLostFocus)
        {
            string phone = PhoneNumber;

            // Camp buit: neutre
            if (string.IsNullOrEmpty(phone))
            {
                TooltipMessage = "Format: 9-15 dígits. Prefix + opcional.";
                IsValid = false;
                ResetVisuals();
                return;
            }

            // Extreure només els dígits (sense el +)
            string digitsOnly = phone.StartsWith("+") ? phone.Substring(1) : phone;
            int digitCount = digitsOnly.Length;

            // Format complet vàlid: entre 9 i 15 dígits
            bool isComplete = digitCount >= 9 && digitCount <= 15;

            if (isComplete)
            {
                // FORMAT VÀLID → VERD
                TooltipMessage = "Telèfon correcte.";
                IsValid = true;
                ShowValid("Telèfon correcte ✓");
                return;
            }

            IsValid = false;

            if (digitCount > 15)
            {
                // Massa dígits → VERMELL immediat
                TooltipMessage = "Telèfon massa llarg. Màxim 15 dígits.";
                ShowError($"Massa dígits ({digitCount}/15)");
            }
            else if (fromLostFocus && digitCount < 9)
            {
                // Ha sortit del camp amb menys de 9 dígits → VERMELL
                TooltipMessage = "Telèfon incomplet. Mínim 9 dígits.";
                ShowError($"Falten {9 - digitCount} dígits");
            }
            else
            {
                // Encara escrivint (< 9 dígits) → NEUTRE
                TooltipMessage = "Format: 9-15 dígits. Prefix + opcional.";
                ResetVisuals();
            }
        }

        // Feedback visual: Error (vermell)
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
            ValidationMessage.Text = "De 9 a 15 dígits. Pots incloure el prefix (+).";
            ValidationMessage.Foreground = (Brush)FindResource("ValidationTextBrush");
        }
    }
}