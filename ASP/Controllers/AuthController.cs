using ASP.Data.DAL;
using ASP.Data.Entities;
using ASP.Migrations;
using ASP.Models;
using ASP.Models.FrontendForm;
using ASP.Models.Home.Model;
using ASP.Models.Home.Signup.MailTemplates;
using ASP.Services.Email;
using ASP.Services.Kdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;

namespace ASP.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IEmailService _emailService;

        private readonly ILogger<AuthController> _logger;

        public AuthController(DataAccessor dataAccessor, IKdfService kdfService, IEmailService emailService, ILogger<AuthController> logger)
        {
            this._dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public object Get([FromQuery(Name = "e-mail")] String email, String? password)
        {
            var user = _dataAccessor.UserDao.Authorize(email, password ?? "");
            if (user == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new { Status = "Auth Failed" };
            }
            else
            {
                /*
				   Http-cecii -- спосіб для збереження з боку сервера даних, що
				   будуть доступними після перевантаження сторінки. 
				   Налаштунвання: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-8.0
				   Після налаштування сесії доступні через властивість HttpContext
				   Ідея сесій - збереження даних у формі "ключ-значення", причому
				   значення має бути серіалізовуваним (таким, що можна зберегти у файл)
				*/

                HttpContext.Session.SetString("auth-user-id", user.Id.ToString());

                return user;
            }
        }

        [HttpPost]
        public object Post(FrontendFormInput model)
        {
            try
            {
                String? fileName = null;
                if (model.Photo != null)
                {
                    String ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/img/avatars/";
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while (System.IO.File.Exists(pathName));
                    using var stream = System.IO.File.OpenWrite(pathName);
                    model.Photo.CopyTo(stream);
                }
                if (fileName == null)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return "File Image required";
                }
                _dataAccessor.UserDao.SignUpApi(model.Name, model.Email, fileName, model.Password, model.Date);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Added";
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return ex.Message;
            }
        }

        [HttpPut]
        public object Put()
        {
            return new { Status = "PUT Works" };
        }

        [HttpPatch]
        public object Patch(String email, String code)
        {
            if (_dataAccessor.UserDao.ConfirmEmail(email, code))
            {
                Response.StatusCode = StatusCodes.Status202Accepted;
                return new { Status = "Ok" };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status409Conflict;
                return new { StatusCode = "Error" };
            }
        }

        public String DoOther()
        {
            if (Request.Method == "RESTORE")
            {
                return DoRestorePassword();
            }
            Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return "Method not Allowed";
        }

        private String DoRestorePassword()
        {
            String? email = Request.Query["email"].FirstOrDefault();
            String? name = Request.Query["username"].FirstOrDefault();
            String? password;
            try
            {
                password = _dataAccessor.UserDao.RestorePassword(email!, name!);
                _logger.LogInformation("Password: " + password);
            }
            catch
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Empty or invalid inputs";
            }
            Response.StatusCode = StatusCodes.Status202Accepted;
            if (password == "")
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return "Error generate password";
            }
            RestorePasswordMailModel model = new RestorePasswordMailModel()
            {
                Password = password,
                User = name,
            };
            MailMessage mailMessage = new()
            {
                Subject = model.GetSubject(),
                IsBodyHtml = true,
                Body = model.GetBody()
            };
            mailMessage.To.Add(email!);
            try
            {
                _emailService.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
            }
            return "RESTORE works with email: " + email;
        }

        [HttpGet("token")]
        public Token? GetToken(String email, String? password)
        {
            var user = _dataAccessor.UserDao.Authorize(email, password ?? "");
            if (user == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return null;
            }
            Token? activeToken = _dataAccessor.UserDao.FindUserToken(user);
            if (activeToken == null)
            {
                _logger.LogInformation("token expired, new token creating");
                return _dataAccessor.UserDao.CreateTokenForUser(user); 
            }
            _logger.LogInformation("token findet");
            return activeToken;
        }
    }
}

/*	Схеми авторизації API. Токени
 *	Розрізняють дві групи схем
 *	- серверні cecii - підходить для Server-Page архітектури
 *	- токени - для SPA архітектури
 *	Токен (від англ. - жетон, посвідчення) - дані, що дозволяють
 *	автентифікувати запит від фронтенду
 *
 *	Back				Front
 *	  <------------ [login,password]
 *	[token: 123] -------->
 *	  <------------ [GET / rooms token: 123]
 *	[nepeвipкa
 *	токeна,
 *	відповідь] ------->
 */

/* Контролери поділяються на дві групи - API та MVC
 * MVC :
 *	- мають багато Action, кожен з яких запускається своїм Route
 *		/Home/Ioc	--> public ViewResult Ioc() { ... }
 *		/Home/Form	--> public ViewResult Form() { ... }
 *	при цьому метод запиту ролі не грає, можливо лише обмежити їх перелік
 *	GET /Home/Ioc, POST /Home/Ioc --> public ViewResult Ioc()
 *	- повернення - IActionResult, частіше за все ViewResult
 * 
 * API:
 *	- мають одну адресу [Route("api/auth")], а різні дії запускаються
 *		різними методами запитів
 *		GET api/auth	-->
 *		POST api/auth	-->	
 *		PUT api/auth	-->
 *	вся відмінність - у методі запиту, неможна потрапити до одного Action
 *	різними методами
 *	- повернення - дані, які автоматично перетворюються або в текст, або
 *	  в JSON (якщо тип повернення String - text/plain, якщо інший
 *	  object, List <... >, User -- application/json )
 *	
 *	
 *	
 *	[FromQuery]
 *
 *					api/auth?email=i.ua&password=123
 *							   |			  |		зв'язування параметрів - за іменами
 *	public object Get(String email, String password)
 *		1) запит (його частина - query) аналізується і зв' язується
 *		   з параметрами public object Get( ... ) за збігом імен
 *		2) якщо якійсь з необхідних параметрів не знайдено, то автоматично
 *		   формується відповідь 400 (Bad request)
 *		   Необхідні параметри - тип яких не містить Nullability (?)
 *	
 *	
 *							 /api/auth?e-mail=i.ua&password=123
 *										  |
 *	public object Get([FromQuery(Name="e-mail")] String email, String password)
 *	
 *	
 */