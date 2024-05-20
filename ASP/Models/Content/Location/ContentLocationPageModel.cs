namespace ASP.Models.Content.Location
{
	public class ContentLocationPageModel
	{
		public Data.Entities.Location Location { get; set; } = null!;
		public List<Data.Entities.Room> Rooms { get; set; } = [];
        public ContentLocationFormModel? FormModel { get; set; }
        public Dictionary<String, String>? ValidationErrors { get; set; }

    }
}
