using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using WPF_MVVM_SPA_Template.Models;

namespace WPF_MVVM_SPA_Template.Services
{
    public static class DataService
    {
        private static readonly string FilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "customers_data.json");

        public static void SaveCustomers(ObservableCollection<Customer> customers)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(customers, options);
            File.WriteAllText(FilePath, jsonString);
        }

        public static ObservableCollection<Customer> LoadCustomers()
        {
            if (!File.Exists(FilePath))
                return new ObservableCollection<Customer>();

            try
            {
                string jsonString = File.ReadAllText(FilePath);
                var list = JsonSerializer.Deserialize<List<Customer>>(jsonString);
                return new ObservableCollection<Customer>(list ?? new List<Customer>());
            }
            catch
            {
                return new ObservableCollection<Customer>();
            }
        }
    }
}