using System.ComponentModel.DataAnnotations;

namespace Fantasy.Shared.Entites
{
    public class Country
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = null!;

        public ICollection<Team> Teams { get; set; }

        public int TeamsCount => Teams == null ? 0 : Teams.Count;
        public ICollection<User> Users { get; set; }

        public int UsersCount => Users == null ? 0 : Users.Count;
    }
}