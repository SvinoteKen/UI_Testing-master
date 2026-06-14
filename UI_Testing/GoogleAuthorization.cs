using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MaterialSkin.Controls;
using MaterialSkin;
using UI_Testing;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Util;

namespace UI_Testing
{
    internal class GoogleAuthorization
    {
        private const string CredPath = "credentials.json";
        private const string TokenPath = "token";
        private static readonly string[] Scopes =
        {
        "https://www.googleapis.com/auth/spreadsheets"
        };
        private static ShowError Error = new ShowError();
        /// <summary>
        /// Главный метод: можно ли продолжать работу
        /// </summary>
        public static async Task<bool> CanProceedAsync()
        {
            // 1. Проверка admin.txt
            if (!IsAdminAllowed())
            {
                MaterialMessageBox.Show(Application.OpenForms[1],
                    "У вас нет прав на просмотр этого раздела.\nОбратитесь к администратору за доступом.\nTelegram: https://t.me/DI_KEN9",
                    "Доступ запрещён",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            // 2. Получение токена
            var token = await GetOAuth2TokenInternalAsync(showError: false);

            if (string.IsNullOrEmpty(token))
            {
                // пробуем второй раз (уже с UI авторизации)
                ShowAuthMessage();
                token = await GetOAuth2TokenInternalAsync(showError: true);

                if (string.IsNullOrEmpty(token))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка admin.txt
        /// </summary>
        private static bool IsAdminAllowed()
        {
            const string adminFile = "admin.txt";

            if (!File.Exists(adminFile))
                return false;

            var text = File.ReadAllText(adminFile);
            return text.Contains("123"); // можно усложнить потом
        }

        /// <summary>
        /// Получение токена (внутренний метод)
        /// </summary>
        private static async Task<string> GetOAuth2TokenInternalAsync(bool showError)
        {
            try
            {
                if (!File.Exists(CredPath))
                {
                    if (showError)
                        MaterialMessageBox.Show(Application.OpenForms[1], "Файл credentials.json не найден.", "Ошибка");

                    return null;
                }
                if (!Directory.Exists(TokenPath))
                {
                    ShowAuthMessage();
                }
                using (var stream = new FileStream(CredPath, FileMode.Open, FileAccess.Read))
                {
                    var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(TokenPath, true)
                    );

                    // если токен устарел
                    if (credential.Token.IsStale)
                    {
                        if (!await credential.RefreshTokenAsync(CancellationToken.None))
                        {
                            // удаляем токен
                            var dir = new DirectoryInfo(TokenPath);
                            if (dir.Exists) dir.Delete(true);

                            // уведомление перед повторной авторизацией
                            ShowAuthMessage();

                            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                secrets,
                                Scopes,
                                "user",
                                CancellationToken.None,
                                new FileDataStore(TokenPath, true)
                            );
                        }
                    }

                    return credential.Token.AccessToken;
                }
            }
            catch (Exception ex)
            {
                if (showError)
                    Error.ShowErr(Application.OpenForms[1], "Ошибка получения токена.", ex.Message);

                return null;
            }
        }
        private static void ShowAuthMessage()
        {
            MaterialMessageBox.Show( 
                "Токен доступа Google отсутствует или истёк.\n\n" +
                "Сейчас откроется страница авторизации Google.\n" +
                "Пожалуйста, выполните вход и предоставьте доступ к Google Таблицам.",
                "Требуется авторизация"
            );
        }
    }
}
