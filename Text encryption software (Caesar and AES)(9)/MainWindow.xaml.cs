using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace TextEncryption
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string inputText = InputTextBox.Text;
            string encryptedText = string.Empty;

            string selectedMethod = (EncryptionMethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (!string.IsNullOrEmpty(selectedMethod))
            {
                switch (selectedMethod)
                {
                    case "Шифр Цезаря":
                        encryptedText = CaesarCipher(inputText, 3); break;
                    case "AES":
                        encryptedText = AESEncrypt(inputText, "your-password");
                        break;
                }
            }

            OutputTextBox.Text = encryptedText;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string encryptedText = OutputTextBox.Text;

            if (!string.IsNullOrWhiteSpace(encryptedText))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    DefaultExt = "txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, encryptedText);
                    MessageBox.Show("Файл успешно сохранен!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Нет текста для сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Clear();
            OutputTextBox.Clear();
            EncryptionMethodComboBox.SelectedIndex = -1;
        }

        private string CaesarCipher(string input, int shift)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                char shiftedChar = c;
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'A' : 'a';
                    shiftedChar = (char)((((c + shift) - offset + 26) % 26) + offset);
                }
                result.Append(shiftedChar);
            }

            return result.ToString();
        }

        private string AESEncrypt(string plainText, string password)
        {
            byte[] salt = GenerateRandomSalt();
            using (var aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(password, salt, 1000);
                aes.Key = key.GetBytes(16);
                aes.IV = key.GetBytes(16);

                using (var ms = new MemoryStream())
                {
                    ms.Write(salt, 0, salt.Length);
                    using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private byte[] GenerateRandomSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }
    }
}
