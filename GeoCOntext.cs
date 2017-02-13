using Microsoft.EntityFrameworkCore;

public class GeoContext : DbContext
{   
    public DbSet<Postcode> Postcodes { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Filename=./BRIDGEPOINT.db");
    }
}