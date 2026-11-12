using System;
using System.Collections.Generic;

namespace prakt_8_trpo.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Role { get; set; }

    public string? PasswordHash { get; set; }
}
