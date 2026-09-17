using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public class ApplicationRole:IdentityRole<Guid>
    {
        public ApplicationRole() : base()
        {
        }
        
        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}
