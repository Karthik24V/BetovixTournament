
namespace Tournament.Domain.DataBase.Entity;

public class User
{
    public long Id { get; set; }
    public string RealName { get; set; }
    public string UniqueName { get; set; } // Game alias
    public string Email { get; set; }
    public string Country { get; set; }
    public bool IsActive { get; set; }
    public long Points { get; set; }
    public DateTime RegisteredOn { get; set; }

    public ICollection<TournamentParticipant> TournamentParticipations { get; set; }
}
