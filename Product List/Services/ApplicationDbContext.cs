using Microsoft.EntityFrameworkCore;
using Product_List.Models;

namespace Product_List.Services
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions options):base(options) 
        {
            
        }
        public DbSet<MyProperty> MyProperty {  get; set; }
    }
}
