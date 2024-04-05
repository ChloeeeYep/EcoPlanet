using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace EcoPlanet.Areas.Identity.Data;

// Add profile data for application users by adding properties to the EcoPlanetUser class
public class EcoPlanetUser : IdentityUser
{
    [PersonalData]
    public string ? FullName { get; set; }

    [PersonalData]
    public string ? Address { get; set; }

    [PersonalData]
    public DateTime DOB { get; set; }

    [PersonalData]
    public char UserType { get; set; }

    public bool isSubscribed { get; set; } = false;

}

