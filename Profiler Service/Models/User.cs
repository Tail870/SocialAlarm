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

        public string? Contacts { set; get; } = "";

        public override string ToString()
        {
            string result = string.Empty;
            if (Login != null)
                result += ("Login: " + Login.ToString() + "; ");
            else
                result += ("Login: [null]; ");
            if (Login != null)
                result += ("Password: " + DisplayedName.ToString() + "; ");
            else
                result += ("Password: [null]; ");
            if (Login != null)
                result += ("Displayed name: " + Password.ToString() + "; ");
            else
                result += ("Displayed name: [null]; ");
            if (Login != null)
                result += ("Contacs: " + Contacts.ToString() + ". ");
            else
                result += ("Contacs: [null].");
            return result;
        }
    }
}