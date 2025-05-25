using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace autorizacia
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public LoginPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

            // Загружаем сохраненные данные, если была отмечена галочка "Запомнить меня"
            var savedUser = _databaseService.GetSavedUser();
            if (savedUser != null)
            {
                EmailEntry.Text = savedUser.Email;
                PasswordEntry.Text = savedUser.Password;
                RememberMeCheckBox.IsChecked = true;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text;

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

            var user = _databaseService.GetUser(email, password);
            if (user != null)
            {
                // Сохраняем учетные данные, если отмечено "Запомнить меня"
                if (RememberMeCheckBox.IsChecked)
                {
                    _databaseService.SaveUserCredentials(email, password);
                }
                else
                {
                    _databaseService.ClearSavedUser();
                }

                await DisplayAlert("Успех", "Вход выполнен успешно!", "OK");

                // Переход на главную страницу после успешного входа
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await DisplayAlert("Ошибка", "Неверный email или пароль", "OK");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage());
        }
    }
}
