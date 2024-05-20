using ASP.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace ASP.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController(IEmailService emailService) : ControllerBase
    {
        private readonly IEmailService _emailService = emailService;

        [HttpGet]
        public Object DoGet() 
        {
            try
            {
                MailMessage mailMessage = new()
                {
                    IsBodyHtml = true,
                    Body = $"<h1>Шановний користувач !</h1>" +
                    $"<p style='color: steelblue'>Вiтаємо на сaйTi " +
                    $"<a href='{Request.Host}'>ASP</a></p>"
                };
                mailMessage.To.Add(new MailAddress("proviryalovich@gmail.com"));
                _emailService.Send(mailMessage);

                return new { Sent = "Ok" };

            }
            catch(Exception ex)
            {
                return new { Error = ex.Message };
            }
        }
    }
}


/* PobOTa 3 E-mail
1. SMTP - Simple Mail Transfer Protocol
протокол надсилання E-mail
[Email-server] ---- >[Email-server]
SMTP/send   IMAP\receive
[Backend]   [User-mailbox]
Для роботи з E-mail необхідно створити поштову скриню (mailbox)
на поштовому сервісі, який підтримує SMTP (у бажаному тарифному плані)
Далі на прикладі Gmail

2. Організація налаштувань
!!! Паролі від пошти (та інших ресурсів) не можна розміщувати на
відкритих репозиторіях і не бажано навіть на закритих.
Задля цього створюються два файли конфігурації - один реальний, який
вилучається з репозиторію, інший - демонстраційний, який є в репозиторії
та містить приклад заповнення конфігурації.
- У файл gitignore додаємо запис emailconfig. json
- створюємо у проєкті файл emailconfig. json
- створюємо у проєкті файл emailconfig.sample.json
= бажано перевірити - зробити коміт і переконатись, що лише один файл
є у репозитopii - emailconfig.sample.json

3. Налаштовуємо або дізнаємось налаштування SMTP (на прикладі Gmail)
- переходимо до обікового запису (https://myaccount.google.com/)
- (зліва) Безпека - (по центру) Вхід - двоетапна автентифікація
- Пароли приложений - створити новий - копіюємо до emailconfig. json
- Після заповнення emailconfig.json, робимо його копію до
emailconfig.sample. json i видаляємо в ній усі персональні дані
Сяк правило замість них залишають примітки на кшталт *** CHANGE_ME *** )
= Додаємо нову конфігурацію до загальних налаштувань у Program.cs
 */