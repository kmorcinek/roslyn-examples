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
        return _users.Count(u => u.IsActive);
    }

    public int CountAdmins()
    {
        // Should be refactored: Where + Count -> Count
        return _users.Count(u => u.Role == "Admin");
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
