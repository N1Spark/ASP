using ASP.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.Home.Signup.MailTemplates
{
    public class SignupMailModel
    {
        public string? User { get; set; }
        public string? Code { get; set; }
        public string? Slug { get; set; }
        public string? Scheme { get; set; }
        public string? Host { get; set; }
        public string? GetSubject()
        {
            return "Подтверждение почты";
        }
        public string GetBody()
        {
            return $"<p>Здравствуйте {User}. Для подтверждения почты введите на сайте код</p>" +
            $"<h2 style='color: orange'>{Code}</h2>" +
            $"<p>или перейдите по <a href='{Scheme}://{Host}/Home/ConfirmEmail/{Slug}'>этой ссылке</a></p>";
        }

    }
}
