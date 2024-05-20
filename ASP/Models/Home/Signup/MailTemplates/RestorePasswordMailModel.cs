using ASP.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.Home.Signup.MailTemplates
{
    public class RestorePasswordMailModel
    {
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? GetSubject()
        {
            return "Восстановление почты";
        }
        public string GetBody()
        {
            return $"<p>Здравствуйте {User}. Ваша учетная запись была успешна восстановлена. Для входа используйте пароль:</p>" +
            $"<h2 style='color: orange'>{Password}</h2>";
        }
    }
}
