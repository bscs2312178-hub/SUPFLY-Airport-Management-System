using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SUPFLY.Areas.Identity.Data;

// Add profile data for application users by adding properties to the SUPFLYUser class
public class SUPFLYUser : IdentityUser
{
    public override string UserName { get; set; } = null!;

    // Fix 2: Explicitly initialize the inherited Email property (often required).
    public override string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}