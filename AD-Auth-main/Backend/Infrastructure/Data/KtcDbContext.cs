using Microsoft.EntityFrameworkCore;

namespace KtcWeb.Infrastructure.Data
{
    public class KtcDbContext : DbContext
    {
        public KtcDbContext(DbContextOptions<KtcDbContext> options) 
            : base(options)
        {
        }

        public DbSet<ClientAtmDto> Clients { get; set; } = null!;
        public DbSet<CurrentStatus> CurrentStatus { get; set; } = null!;
        public DbSet<ComponentList> ComponentList { get; set; } = null!;
        public DbSet<PropertyList> PropertyList { get; set; } = null!;
        public DbSet<ValueList> ValueList { get; set; } = null!;
        public DbSet<AssetHistory> AssetHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client principal (keyless)
            modelBuilder.Entity<ClientAtmDto>()
                        .ToTable("Clients")
                        .HasNoKey();

            // Ignorer toutes les navigations pour éviter les erreurs de relationship
            modelBuilder.Entity<ClientAtmDto>()
                        .Ignore(c => c.Branch);

            modelBuilder.Entity<BranchDto>()
                        .HasNoKey();

            modelBuilder.Entity<BusinessDto>()
                        .HasNoKey();

            modelBuilder.Entity<RegionDto>()
                        .HasNoKey();

            modelBuilder.Entity<CurrentStatus>()
                        .HasNoKey();

            modelBuilder.Entity<AssetHistory>()
                        .HasNoKey();
        }
    }
}

