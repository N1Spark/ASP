using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.FrontendForm
{
	public class FrontendFormInput
	{
        //// для вхідних даних збіг імен (у класі та JSON) вимагається
        //public String UserName { get; set; } = null!;
        //public String UserEmail { get; set; } = null!;
        //public bool UserTerm {  get; set; } = false;
        //public String UserGen { get; set; } = null!;
        //public DateTime UserDate { get; set; }

        [FromForm(Name = "user-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "user-email")]
        public String Email { get; set; } = null!;

        [FromForm(Name = "user-password")]
        public String Password { get; set; } = null!;

        [FromForm(Name = "user-repeat")]
        public String Repeat { get; set; } = null!;

        [FromForm(Name = "user-birthdate")]
        public DateTime Date { get; set; }

        [FromForm(Name = "user-agreement")]
        public bool Term { get; set; }

        [FromForm(Name = "user-avatar")]
        public IFormFile Photo { get; set; } = null!;
    }
}
