using Auditing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Auditing.Infrastructure.Persistence
{
    public class AuditingDbContext : DbContext
    {
        public AuditingDbContext(DbContextOptions<AuditingDbContext> options) : base(options)
        {
        }

        public DbSet<Audit> Audits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Allow use plural on DbSet since table name uses singular
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.Relational().TableName = entityType.DisplayName();
            }
        }

    }
}