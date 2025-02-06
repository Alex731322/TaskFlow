using TaskFlow.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }  // Новое свойство
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } = "User";
    public List<Task> Tasks { get; set; }
    public RefreshToken RefreshToken { get; set; }
    public List<Follow> Followers { get; set; } = new();
    public List<Follow> Following { get; set; } = new();
    public List<Post> Posts { get; set; } = new();
}