using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System;

namespace PAC_4_Calculadora
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _nomActual = "";
        private List<double> _numeros = new List<double>();
        private List<string> _operadors = new List<string>();
        private bool _error = false;
        private bool _resultatRecent = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Value_Click(object sender, RoutedEventArgs e)
        {
            if (_error) return;

            if (_resultatRecent)
            {
                _numeros.Clear();
                _resultatRecent = false;
            }

            Button boto = (Button)sender;
            string digit = boto.Tag.ToString();

            _nomActual += digit;
            ActualitzarDisplay();
        }

        //Botons operació divisió, multiplicació i resta suma
        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            if (_error) return;

            string op = ((Button)sender).Tag.ToString();

            if (_resultatRecent)
            {
                _resultatRecent = false;
                _operadors.Add(op);
                ActualitzarDisplay();
                return;
            }

            if (_nomActual == "" && _numeros.Count == _operadors.Count)
            {
                MostrarError();
                return;
            }

            if (_nomActual != "")
            {
                _numeros.Add(double.Parse(_nomActual));
                _nomActual = "";
            }

            _operadors.Add(op);
            ActualitzarDisplay();
        }

        // Botó =
        private void Result_Click(object sender, RoutedEventArgs e)
        {
            if (_error) return;

            if (_nomActual == "")
            {
                MostrarError();
                return;
            }

            _numeros.Add(double.Parse(_nomActual));
            _nomActual = "";

            try
            {
                double resultat = Calcula(_numeros, _operadors);
                Display.Text = resultat.ToString();

                _numeros.Clear();
                _operadors.Clear();
                _numeros.Add(resultat);     
                _resultatRecent = true;     
            }
            catch
            {
                MostrarError();
                return;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _nomActual = "";
            _numeros.Clear();
            _operadors.Clear();
            _error = false;
            _resultatRecent = false;   
            Display.Text = "0";
        }

        private double Calcula(List<double> nums, List<string> ops)
        {
            List<double> n = new List<double>(nums);
            List<string> o = new List<string>(ops);

            int i = 0;
            while (i < o.Count)
            {

                if (o[i] == "*" || o[i] == "/")
                {
                    double r;
                    if (o[i] == "*")
                    {
                        r = n[i] * n[i + 1];
                    }
                    else
                    {
                        if (n[i + 1] == 0)
                            throw new DivideByZeroException();
                        r = n[i] / n[i + 1];
                    }

                    n[i] = r;
                    n.RemoveAt(i + 1);
                    o.RemoveAt(i);
                }
                else
                {
                    i++;
                }

            }

            double resultat = n[0];
            for (int j = 0; j < o.Count; j++)
            {
                if (o[j] == "+")
                    resultat += n[j + 1];
                else
                    resultat -= n[j + 1];
            }
            return resultat;


        }

        private void ActualitzarDisplay()
        {
            string text = "";

            for (int i = 0; i < _numeros.Count; i++)
            {
                text += _numeros[i].ToString();

                if (i < _operadors.Count)
                    text += " " + SimbolBonic(_operadors[i]) + " ";
            }

            text += _nomActual;

            Display.Text = text == "" ? "0" : text;
        }

        private string SimbolBonic(string op)
        {
            switch (op)
            {
                case "*": return "×";
                case "/": return "÷";
                case "-": return "−";
                default: return op;
            }
        }

        private void MostrarError()
        {
            _error = true;
            _nomActual = "";
            _numeros.Clear();
            _operadors.Clear();
            Display.Text = "Error";
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



    }
}