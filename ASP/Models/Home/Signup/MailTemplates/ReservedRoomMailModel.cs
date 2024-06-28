namespace ASP.Models.Home.Signup.MailTemplates
{
    public class ReservedRoomMailModel
    {
        public string? User { get; set; }
        public string? Room { get; set; }
        public string? Location { get; set; }
        public string? Date { get; set; }
        public string? Price { get; set; }
        public string? GetSubject()
        {
            return "Reservation";
        }
        public string GetBody()
        {
            return $"<h2>Hello {User}. Inform you about reservation at {Location}, room {Room}, price {Price}₴.</h2>" +
            $"<p>Reservation date {Date}</p>";
        }
    }
}
