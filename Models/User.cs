using System;
using System.Collections.Generic;

namespace prakt_8_trpo.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; } = string.Empty;

    public string? Role { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }
}
