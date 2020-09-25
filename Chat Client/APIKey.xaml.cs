using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for APIKey.xaml
    /// </summary>
    public partial class APIKey : Window
    {
        private readonly Regex apiKeyFormat = new Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{20}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");

        public bool ValidKey = false;

        public APIKey()
        {
            InitializeComponent();
            txtApiKey.Text = Properties.Settings.Default.apiKey;
        }

        private async void ValidateKey()
        {
            if (apiKeyFormat.IsMatch(txtApiKey.Text))
            {
                txtApiKey.IsEnabled = false;
                var connection = new Connection(txtApiKey.Text);
                var client = new Gw2Client(connection);
                var tokeninfo = await client.WebApi.V2.TokenInfo.GetAsync();

                var hasAccount = false;
                var hasCharacters = false;
                var hasGuilds = false;

                //Set all to bad
                imgAccount.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/1444524.png"));
                imgCharacters.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/1444524.png"));
                imgGuilds.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/1444524.png"));

                //Check Permissions
                if (HasPermission(tokeninfo, TokenPermission.Account))
                {
                    hasAccount = true;
                    imgAccount.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/1444520.png"));
                }
                if (HasPermission(tokeninfo, TokenPermission.Characters))
                {
                    hasCharacters = true;
                    imgCharacters.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/1444520.png"));
                }
                if (HasPermission(tokeninfo, TokenPermission.Guilds))
                {
                    hasGuilds = true;
                    imgGuilds.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/1444520.png"));
                }

                ValidKey = (hasAccount && hasCharacters && hasGuilds);
                client.Dispose();
                txtApiKey.IsEnabled = true;
            }
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidKey)
            {
                //Save key
                Properties.Settings.Default.apiKey = txtApiKey.Text;
                Properties.Settings.Default.Save();
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("API Key is missing permissions. Make sure that it has \"Account, Characters and Guilds\"", "Missing Permissions", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void txtApiKey_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateKey();
        }

        private bool HasPermission(TokenInfo TokenInfo, ApiEnum<TokenPermission> Permission)
        {
            return (TokenInfo.Permissions.Contains(Permission));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateKey();
        }
    }
}
