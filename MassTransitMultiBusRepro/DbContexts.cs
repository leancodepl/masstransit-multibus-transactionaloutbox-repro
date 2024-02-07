using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MassTransitMultiBusRepro;

public interface IDbContext<TSelf>
    where TSelf : DbContext, IDbContext<TSelf>
{
    static abstract TSelf Create(DbContextOptions<TSelf> options);
}

public class DbContext1 : DbContext, IDbContext<DbContext1>
{
    public DbContext1(DbContextOptions<DbContext1> options) : base(options)
    { }

    public static DbContext1 Create(DbContextOptions<DbContext1> options) => new(options);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("dbo1");
        builder.AddTransactionalOutboxEntities();
    }

}

public class DbContext2 : DbContext, IDbContext<DbContext2>
{
    public DbContext2(DbContextOptions<DbContext2> options) : base(options)
    { }

    public static DbContext2 Create(DbContextOptions<DbContext2> options) => new(options);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("dbo2");
        builder.AddTransactionalOutboxEntities();
    }
}
