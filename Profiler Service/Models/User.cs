using System.ComponentModel.DataAnnotations;

namespace Profiler_Service.Models
{
    public class User
    {
        [Key]
        public string Login { set; get; }

        [Required]
        public string Password { set; get; }

        [Required]
        public string DisplayedName { set; get; }

        public string Contacts { set; get; }

        public override string ToString()
        {
            return "Login: " + Login + "; " +
                "Password: " + Password + "; " +
                "Displayed name: " + DisplayedName + "; " +
                "contacts: " + Contacts;
        }
    }
}