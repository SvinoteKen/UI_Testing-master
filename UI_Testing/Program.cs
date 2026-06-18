using System;
using System.Windows.Forms;

namespace UI_Testing
{
    static class Status
    {
        public static bool Enter { get; set; }
    }
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //DialogResult result;
            //using (var loginForm = new LoginForm())
            //{
            //    result = loginForm.ShowDialog();
            //    MessageBox.Show(result.ToString());
            //}

            //if (result == DialogResult.OK)
            //{
            //    Application.Run(new MainForm(null, null)); // Передай, если нужно
            //}
            //else
            //{
            //    Application.Exit();
            //}
            Application.Run(new LoginForm());
        }
    }
}
