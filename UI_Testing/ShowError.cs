using MaterialSkin.Controls;
using System.Net;
using System.Windows.Forms;

namespace UI_Testing
{
    class ShowError
    {
        public void ShowErr(Form owner, string context, string errorText, HttpStatusCode? statusCode = null)
        {
            string statusInfo = statusCode.HasValue ? $" ({(int)statusCode} {statusCode.Value})" : "";
            string message = $"{context}{statusInfo}\n{errorText}\n\nЕсли ошибка не проходит, свяжитесь по Telegram: https://t.me/DI_KEN9";

            MaterialMessageBox.Show(owner, message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
