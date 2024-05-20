using System.Text.Json.Serialization;

namespace ASP.Data.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public String? EmailConfirmCode { get; set; } // code or null
        public string? AvatarUrl { get; set; }
		[JsonIgnore] public string Salt { get; set; } // 3a RFC-2898
        [JsonIgnore] public string Derivedkey { get; set; } // 3a RFC-2898
		public DateTime? Birthdate { get; set; }
        public DateTime? DeleteDt { get; set; }
		public String? Role { get; set; }

        [JsonIgnore] public List<Reservation> Reservations { get; set; }
    }
}


/*
*	Category: Hotels Apartments Resorts Villas
*	Location: Hotel1 Hotel2 Hotel3
*	Room: Room101 Room102 Room103
*/