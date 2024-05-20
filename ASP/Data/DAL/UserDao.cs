using ASP.Data.Entities;
using ASP.Models;
using ASP.Models.Home.Model;
using ASP.Services.Kdf;
using Microsoft.EntityFrameworkCore;

namespace ASP.Data.DAL
{
    public class UserDao
    {
        private readonly Object _dblocker;
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;

        public UserDao(DataContext dataContext, IKdfService kdfService, object dblocker)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
            _dblocker = dblocker;
        }

        public User? GetUserById(String id)
        {
            User? user;
            try
            {
                lock (_dblocker)
                {
                    user = _dataContext.Users.Find(Guid.Parse(id));
                }
            }
            catch { return null; }
            return user;
        }
        public User? GetUserByToken(Guid token)
        {
            User? user;
            lock (_dblocker)
            {
                user = _dataContext.Tokens
                    .Include(t => t.User)
                    .FirstOrDefault(t => t.id == token)
                    ?.User;
            }
            return user;
        }

        public Token? FindUserToken(User user)
        {
            Token? userToken = _dataContext.Tokens.FirstOrDefault(t => t.UserId == user.Id);
            DateTime today = DateTime.Now;
            if (userToken == null || userToken.ExpireDt < today)
            {
                return null;
            }
            return userToken;
        }

        public Token? CreateTokenForUser(User user)
        {
            return CreateTokenForUser(user.Id);
        }
        public Token? CreateTokenForUser(Guid userId)
        {
            Token token = new()
            {
                id = Guid.NewGuid(),
                UserId = userId,
                SubmitDt = DateTime.Now,
                ExpireDt = DateTime.Now.AddDays(1)
            };
            _dataContext.Tokens.Add(token);
            try
            {
                lock (_dblocker)
                {
                    _dataContext.SaveChanges();
                }
                return token;
            }
            catch
            {
                _dataContext.Tokens.Remove(token);
                return null;
            }
        }

        public User? Authorize(String email, String password)
        {
            var user = _dataContext
                .Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null ||
                user.Derivedkey != _kdfService.DerivedKey(user.Salt, password))
            {
                return null;
            }
            return user;
        }

        public void Signup(User user)
        {
            if (user.Id == default)
            {
                user.Id = Guid.NewGuid();
            }
            _dataContext.Users.Add(user);
            _dataContext.SaveChanges();
        }

        public Boolean ConfirmEmail(String email, String code)
        {
            // Find user by Email
            User? user;
            lock (_dblocker)
            {
                user = _dataContext.Users.FirstOrDefault(x => x.Email == email);
            }
            if (user == null || user.EmailConfirmCode != code) return false;
            user.EmailConfirmCode = null;
            lock (_dblocker)
            {
                _dataContext.SaveChanges();
            }
            return true;
        }

        public String RestorePassword(String email, String name)
        {
            User? user;
            lock (_dblocker)
            {
                user = _dataContext.Users.FirstOrDefault(x => x.Email == email && x.Name == name);
            }
            if (user == null) return "";
            String salt = RandomStringService.GenerateSalt(10);
            String password = RandomStringService.GenerateOTP(8);
            user.Salt = salt;
            user.Derivedkey = _kdfService.DerivedKey(salt, password);
            lock (_dblocker)
            {
                _dataContext.SaveChanges();
            }
            return password;
        }
    }
}

/* DAL - Data Access Layer - сукупність ycіх DAO
 * DAO - Data Access Object - набір методів для роботи з сутністю
 */
