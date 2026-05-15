using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace WPF_MVVM_SPA_Template.Models
{
    public class Customer : INotifyPropertyChanged
    {
        private int _id;
        private string? _dni;
        private string? _firstName;
        private string? _lastName;
        private string? _email;
        private string? _phone;
        private DateTime _dataAlta;
        private List<double> _rendimentMensual = new List<double>();

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string? Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); }
        }

        public string? FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string? LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string? Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string? Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public DateTime DataAlta
        {
            get => _dataAlta;
            set { _dataAlta = value; OnPropertyChanged(); }
        }

        // El constructor es manté igual per assignar la data actual
        public Customer()
        {
            DataAlta = DateTime.Now; // Això assegura que SEMPRE surti la data d'avui 
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //Crea una còpia idèntica d'aquest objecte (per editar sense por)
        public Customer Clone()
        {
            return new Customer
            {
                Id = this.Id,
                Dni = this.Dni,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Phone = this.Phone,
                DataAlta = this.DataAlta,
                RendimentMensual = new List<double>(this.RendimentMensual)
            };
        }

        //Copia les dades d'un altre client dins d'aquest per quan guardem
        public void CopyFrom(Customer source)
        {
            this.Id = source.Id;
            this.Dni = source.Dni;
            this.FirstName = source.FirstName;
            this.LastName = source.LastName;
            this.Email = source.Email;
            this.Phone = source.Phone;
            this.DataAlta = source.DataAlta;
        }

        public List<double> RendimentMensual
        {
            get => _rendimentMensual;
            set { _rendimentMensual = value; OnPropertyChanged(); }
        }
    }
}