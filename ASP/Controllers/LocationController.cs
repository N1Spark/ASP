using ASP.Data.DAL;
using ASP.Data.Entities;
using ASP.Middleware;
using ASP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Web;

namespace ASP.Controllers
{
	[Route("api/location")]
	[ApiController]
	public class LocationController : BackendController
    {
		private readonly DataAccessor _dataAccessor;
        private readonly ILogger<CategoryController> _logger;

        public LocationController(DataAccessor dataAccessor, ILogger<CategoryController> logger)
		{
			_dataAccessor = dataAccessor;
            _logger = logger;
        }
        private String? getAdminAuthMessage()
        {
            if (!isAuthenticated)
            {
                // якщо авторизація не пройдена, то повідомлення в Items
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return HttpContext.Items[nameof(AuthTokenMiddleware)]?.ToString() ?? "Auth required";
            }

            if (!isAdmin)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return "Access to API forbidden";
            }
            return null;
        }

        [HttpGet("{id}")]
		public List<Location> DoGet(String id)
		{
			return _dataAccessor.ContentDao.GetLocations(id, isAdmin);
		}

		[HttpPost]
		public String Post(LocationFormModel model)
		{
            if (getAdminAuthMessage() is String msg) { return msg; }
            try
            {
				String? fileName = null;
				if (model.Photo != null)
				{
					string ext = Path.GetExtension(model.Photo.FileName);
					String path = Directory.GetCurrentDirectory() + "/wwwroot/img/content/";
					String pathName;
					do
					{
						fileName = RandomStringService.GenerateFilename(10) + ext;
						pathName = path + fileName;
					}
					while (System.IO.File.Exists(pathName));

					using var steam = System.IO.File.OpenWrite(pathName);
					model.Photo.CopyTo(steam);
				}
				_dataAccessor.ContentDao.AddLocation(
					name: model.Name,
					description: model.Description,
					CategoryId: model.CategoryId,
					Stars: model.Stars,
					PhotoUrl: fileName,
					slug: model.Slug);
				Response.StatusCode = StatusCodes.Status201Created;
				return "OK";
			}
			catch (Exception ex)
			{
                Response.StatusCode = StatusCodes.Status400BadRequest;
				return ex.Message;
            }
		}

		[HttpPatch]
		public Location? DoPatch(String slug)
		{
			return _dataAccessor.ContentDao.GetLocationBySlug(slug);
		}

        [HttpPut]
        public String DoPut([FromForm] LocationFormModel model)
        {
            if (getAdminAuthMessage() is String msg) { return msg; }
            // перевіряємо CategoryId на наявність
            if (model?.LocationId == null || model.LocationId == default(Guid))
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return "Missing required parameter: 'location-id'";
            }
            // перевіряємо чи є взагалі така категорія
            Location? location = _dataAccessor.ContentDao.GetLocationById(model.LocationId);
            if (location == null)
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return $"Parameter 'category-id' ({model.LocationId}) belongs to no entity";
            }
            // Оновлюємо дані за принципом: якщо даних немає, то залишаються попередні значення
            if (!String.IsNullOrEmpty(model.Name)) location.Name = model.Name;
            if (!String.IsNullOrEmpty(model.Description)) location.Description = model.Description;
            if (!String.IsNullOrEmpty(model.Slug)) location.Slug = model.Slug;
            if (location.Stars != model.Stars) location.Stars = model.Stars;
            if (model.Photo != null)        // передається новий файл - зберігаємо новий, видаляємо старий
            {
                try
                {
                    String? fileName = null;
                    string ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/img/content/";
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while (System.IO.File.Exists(pathName));
                    using var steam = System.IO.File.OpenWrite(pathName);
                    model.Photo.CopyTo(steam);
                    // новий файл успішно завантажений - видаляємо старий
                    if (!String.IsNullOrEmpty(location.PhotoUrl))
                    {
                        try
                        {
                            System.IO.File.Delete(path + location.PhotoUrl);
                        }
                        catch { _logger.LogWarning(location.PhotoUrl + " not deleted"); }
                    }
                    // зберігаємо нове ім'я
                    location.PhotoUrl = fileName;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.Message);
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return "ERROR";
                }
            }
            _dataAccessor.ContentDao.UpdateLocation(location);
            Response.StatusCode = StatusCodes.Status200OK;
            return "Updated";
        }

        [HttpDelete("{id}")]
        public String DoDelete(Guid id)
        {
            if (getAdminAuthMessage() is String msg) { return msg; }
            _dataAccessor.ContentDao.DeleteLocation(id);
            Response.StatusCode = StatusCodes.Status202Accepted;
            return "OK";
        }

        public String DoOther()
        {

            // дані про метод запиту - у Request. Method
            if (Request.Method == "RESTORE")
            {
                return DoRestore();
            }
            Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return "Method not Allowed";
        }

        private String DoRestore()
        {
            if (getAdminAuthMessage() is String msg) { return msg; }
            // Через відсутність атрибутів, автоматичного зв'язування параметрі
            // немає, параметри дістаємо з колекцій Request
            String? id = Request.Query["id"].FirstOrDefault();
            try
            {
                _dataAccessor.ContentDao.RestoreLocation(Guid.Parse(id!));
            }
            catch
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Empty or invalid id";
            }
            Response.StatusCode = StatusCodes.Status202Accepted;
            return "RESTORE works with id: " + id;
        }
    }

	public class LocationFormModel
	{
        [FromForm(Name = "loc-category-id")]
        public Guid CategoryId { get; set; }
        
        [FromForm(Name = "location-id")]
        public Guid? LocationId { get; set; }

        [FromForm(Name = "location-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "location-description")]
        public String Description { get; set; } = null!;

        [FromForm(Name = "location-slug")]
        public String Slug { get; set; } = null!;

        [FromForm(Name = "location-stars")]
        public int Stars { get; set; }

        [FromForm(Name = "location-photo")]
        public IFormFile Photo { get; set; } = null!;
    }
}
