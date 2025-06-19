namespace SecureFileStorage.Core.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<UserRole> UserRoles { get; set; }
}