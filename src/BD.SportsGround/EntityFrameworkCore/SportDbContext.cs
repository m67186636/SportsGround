using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;

namespace BD.SportsGround.EntityFrameworkCore
{
    public class SportDbContext :DbContext
    {


        public SportDbContext(DbContextOptions<SportDbContext> options)
            : base(options)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Ground>(b =>
            {
                b.ToTable("Ground");
                b.Property(x => x.Id);
                var properties = typeof(Ground).GetProperties().Where(x => x.CanWrite && x.CanRead).ToArray();
                foreach (var property in properties)
                {
                    if (property.GetCustomAttribute<JsonPropertyAttribute>() != null) 
                        b.Property(property.Name);
                }
                b.HasKey(x => x.Id);
            });
        }
    }
}
