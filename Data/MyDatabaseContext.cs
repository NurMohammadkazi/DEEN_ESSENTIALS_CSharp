using Deen_Essentials.Models;
using Microsoft.EntityFrameworkCore;
//using Deen_Essentials.Web.Models.Entities;

namespace Deen_Essentials.Web.Data
{
    public class MyDatabaseContext: DbContext
    {
        public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options):base(options)
        {
             
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
