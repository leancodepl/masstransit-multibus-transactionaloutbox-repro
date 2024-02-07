using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MassTransitMultiBusRepro;

public class Program
{
    const string DbConnectionString1 = "Server=localhost,11433;Database=App1;User Id=sa;Password=Passw12#;Encrypt=False";
    const string DbConnectionString2 = "Server=localhost,11433;Database=App2;User Id=sa;Password=Passw12#;Encrypt=False";

    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var db1 = scope.ServiceProvider.GetRequiredService<DbContext1>();
            await db1.Database.EnsureDeletedAsync();
            await db1.Database.EnsureCreatedAsync();
            Console.WriteLine("DB1 migrated");

            var publishEndpoint1 = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint1.Publish(new Message1());
            await db1.SaveChangesAsync();
            Console.WriteLine("Message1 published!");
        }

        using (var scope = host.Services.CreateScope())
        {
            var db2 = scope.ServiceProvider.GetRequiredService<DbContext2>();
            await db2.Database.EnsureDeletedAsync();
            await db2.Database.EnsureCreatedAsync();
            Console.WriteLine("DB2 migrated");

            var publishEndpoint2 = scope.ServiceProvider.GetRequiredService<Bind<IBus2, IPublishEndpoint>>().Value;
            await publishEndpoint2.Publish(new Message2());
            await db2.SaveChangesAsync();
            Console.WriteLine("Message2 published!");
        }

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddDbContext<DbContext1>(opts => opts.UseSqlServer(DbConnectionString1));
                services.AddDbContext<DbContext2>(opts => opts.UseSqlServer(DbConnectionString2));

                services.AddMassTransit(x =>
                {
                    x.AddEntityFrameworkOutbox<DbContext1>(outboxCfg =>
                    {
                        outboxCfg.UseBusOutbox();
                    });

                    x.AddConsumer<Consumer1>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                });

                services.AddMassTransit<IBus2>(x =>
                {
                    x.AddEntityFrameworkOutbox<DbContext2>(outboxCfg =>
                    {
                        outboxCfg.UseBusOutbox();
                    });

                    x.AddConsumer<Consumer2>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", 5673, "/", h => {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                });
            });
    }
}
