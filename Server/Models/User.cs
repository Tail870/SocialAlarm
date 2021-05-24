using System.ComponentModel.DataAnnotations;

namespace Models
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
                "Password: " + "***" + "; " +
                "Displayed name: " + DisplayedName + "; " +
                "contacts: " + Contacts;
        }
    }
}