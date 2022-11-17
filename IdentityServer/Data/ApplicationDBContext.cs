using IdentityServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data
{
    public class ApplicationDBContext:IdentityDbContext<TrimarkUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> option):base(option)
        {

        }
    }
}
