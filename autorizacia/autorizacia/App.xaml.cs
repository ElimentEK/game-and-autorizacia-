﻿using Xamarin.Forms;

namespace autorizacia
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the MainPage to a NavigationPage containing the LoginPage
            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
