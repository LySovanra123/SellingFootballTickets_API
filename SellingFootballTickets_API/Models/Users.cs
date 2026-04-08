using System.ComponentModel.DataAnnotations;

namespace SellingFootballTickets_API.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }    
        public string Description { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        [Required]
        public bool Active { get; set; }
        [Required]
        public string Role { get; set; }
        public ICollection<Orders> Orders { get; set; }
    }
}
