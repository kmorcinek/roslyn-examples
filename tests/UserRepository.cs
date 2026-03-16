using System.Collections.Generic;
using System.Linq;

namespace SampleApp;

public class UserRepository
{
    private readonly IEnumerable<User> _users;

    public UserRepository(IEnumerable<User> users)
    {
        _users = users;
    }

    public int CountActiveUsers()
    {
        // Should be refactored: Where + Count -> Count
        return _users.Where(u => u.IsActive).Count();
    }

    public int CountAdmins()
    {
        // Should be refactored: Where + Count -> Count
        return _users.Where(u => u.Role == "Admin").Count();
    }

    public bool HasAnyUser()
    {
        // No predicate — should NOT be changed
        return _users.Any();
    }
}

public class User
{
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
}
