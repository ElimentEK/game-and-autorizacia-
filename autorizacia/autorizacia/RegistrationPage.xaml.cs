using autorizacia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace autorizacia
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrationPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        public RegistrationPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Ошибка", "Пожалуйста, введите email", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Пожалуйста, введите пароль", "OK");
                return;
            }
            if (password != confirmPassword)
            {
                await DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
                return;
            }
            if (_databaseService.UserExists(email))
            {
                await DisplayAlert("Ошибка", "Email уже зарегистрирован", "OK");
                return;
            }
            var user = new User
            {
                Email = email,
                Password = password
            };
            _databaseService.SaveUser(user);
            await DisplayAlert("Успех", "Регистрация завершена успешно!", "OK");
            await Navigation.PopAsync();
        }
    }
}