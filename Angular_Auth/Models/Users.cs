using System.ComponentModel.DataAnnotations;

namespace Angular_Auth.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string UserName { get; set; }

        public string password { get; set; }

        public string Token { get; set; }

        public string Role { get; set; }

        public int Myproperty { get; set; }

        public string Email { get; set; }
    }
}
