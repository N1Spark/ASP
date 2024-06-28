using ASP.Data.DAL;
using ASP.Data.Entities;
using ASP.Middleware;
using ASP.Models.Content.Location;
using ASP.Models.Content.Room;
using ASP.Models.Home.Signup.MailTemplates;
using ASP.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mail;
using System.Security.Claims;

namespace ASP.Controllers
{
    [Route("api/room")]
    [ApiController]
    public class RoomController(DataAccessor dataAccessor, ILogger<RoomController> logger, IEmailService emailService)
        : BackendController
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<RoomController> _logger = logger;
        private readonly IEmailService _emailService = emailService;

        [HttpGet("all/{id}")]
        public List<Room> GetRooms(String id)
        {
            // var location = _dataAccessor.ContentDao.GetLocationBySlug(id);
            List<Room> rooms;
            {
                rooms = _dataAccessor.ContentDao.GetRooms(id);
            }
            return rooms;
        }

        [HttpGet("{id}")]
        public Room? GetRoom([FromRoute] String id/*, 
            [FromQuery] int? year, [FromQuery] int? month*/)
        {
            var room = _dataAccessor.ContentDao.GetRoomBySlug(id);
            if (room == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }
            room.Reservations = room.Reservations
                .Where(r => r.Date >= DateTime.Today).ToList();

            //room.Reservations.ForEach(r => { r.Room = null!; r.User = null!; });

            return room;
        }

        [HttpPost]
        public String DoPost(RoomFormModel model)
        {
            try
            {
                String? fileName = null;
                if (model.Photo != null)
                {
                    String ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/img/content/";
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
                _dataAccessor.ContentDao.AddRoom(
                    name: model.Name,
                    description: model.Description,
                    photoUrl: fileName,
                    slug: model.Slug,
                    locationId: model.LocationId,
                    stars: model.Stars,
                    dailyPrice: model.DailyPrice);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Added";
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return ex.Message;
            }
        }

        [HttpGet("reserve/{id}")]
        public List<Reservation> GetReservations(String id)
        {
            Room? room;
            lock (this)
            {
                room = _dataAccessor.ContentDao.GetRoomBySlug(id);
            }
            return room?.Reservations;
        }

        [HttpPost("reserve")]
        public String ReserveRoom([FromBody] ReserveRoomFormModel model)
        {
            if (!base.isAuthenticated)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return "Authorization failed";
            }

            if (base.claims?.First(c => c.Type == ClaimTypes.Sid).Value
                != model.UserId.ToString())
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return "Ambiguous user identification";
            }

            Reservation? reservation = _dataAccessor.ContentDao
                .GetReservation(model.RoomId, model.Date);
            if (reservation != null)
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return "Room is reserved for requested date";
            }

            try
            {
                User? user = _dataAccessor.UserDao.GetUserById(model.UserId.ToString());
                Room? room = _dataAccessor.ContentDao.GetRoomById(model.RoomId.ToString());
                Location? location = _dataAccessor.ContentDao.GetLocationById(room.LocationId);
                ReservedRoomMailModel mailModel = new ReservedRoomMailModel()
                {
                    User = user?.Name,
                    Date = model.Date.ToString(),
                    Location = location?.Name,
                    Room = room?.Name,
                    Price = room?.DailyPrice.ToString()
                };
                MailMessage mailMessage = new()
                {
                    Subject = mailModel.GetSubject(),
                    IsBodyHtml = true,
                    Body = mailModel.GetBody()
                };
                mailMessage.To.Add(user?.Email!);
                _emailService.Send(mailMessage);
                _dataAccessor.ContentDao.ReserveRoom(model);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Reserved";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return ex.Message;
            }
        }

        [HttpDelete("reserve")]
        public String DropReservation([FromQuery] Guid reserveId)
        {
            if (reserveId == default)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Guid parse error";
            }
            try
            {
                _dataAccessor.ContentDao.DeleteReservation(reserveId);
                Response.StatusCode = StatusCodes.Status202Accepted;
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return ex.Message;
            }
        }

        [HttpPatch]
        public Room? DoPatch(String slug)
        {
            return _dataAccessor.ContentDao.GetRoomBySlug(slug);
        }
    }
}
