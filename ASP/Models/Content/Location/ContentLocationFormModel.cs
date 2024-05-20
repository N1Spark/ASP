using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.Content.Location
{
    public class ContentLocationFormModel : Controller
    {
        [FromForm(Name = "room-name")]
        public String RoomName { get; set; } = null!;


        [FromForm(Name = "room-description")]
        public String RoomDescription { get; set; } = null!;


        [FromForm(Name = "room-slug")]
        public String Slug { get; set; } = null!;


        [FromForm(Name = "room-stars")]
        public int RoomStars { get; set; }

        [FromForm(Name = "room-photo")]
        public IFormFile RoomPhoto { get; set; } = null!;


        [FromForm(Name = "room-price")]
        public Double RoomPrice { get; set; }

        [FromForm(Name = "signup-button")]
        public bool HasData { get; set; } = false;

        public String? SavedPhotoFilename { get; set; }
    }
}
