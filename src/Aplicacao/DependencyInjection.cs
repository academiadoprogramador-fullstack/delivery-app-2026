using DeliveryApp.Aplicacao.Modulos.Pedidos.Mensageria;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryApp.Aplicacao;

public static class DependencyInjection
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException("A ConnectionString \"RabbitMq\" não foi configurada");

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            // Configura a injeção dos Consumers
            config.AddConsumer<CriarPedidoConsumer>();
            config.AddConsumer<AlterarStatusPedidoConsumer>();

            config.UsingRabbitMq((context, rabbitMq) =>
            {
                rabbitMq.Host(new Uri(rabbitMqConnectionString));

                rabbitMq.ReceiveEndpoint("pedidos-criados", endpoint =>
                {
                    endpoint.PrefetchCount = 4; // Quantas mensagens o RabbitMQ deve carregar adiantado
                    endpoint.ConcurrentMessageLimit = 2; // Quantos consumers serão instanciados em paralelo
                    endpoint.UseMessageRetry(DefaultMessageRetryIntervals); // Quantas re-tentativas serão feitas e o intervalo entre elas

                    endpoint.ConfigureConsumer<CriarPedidoConsumer>(context);
                });

                rabbitMq.ReceiveEndpoint("pedidos-atualizados", endpoint =>
                {
                    endpoint.PrefetchCount = 4;
                    endpoint.ConcurrentMessageLimit = 2;
                    endpoint.UseMessageRetry(DefaultMessageRetryIntervals);

                    endpoint.ConfigureConsumer<AlterarStatusPedidoConsumer>(context);
                });
            });
        });

        services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = true;
            options.StartTimeout = TimeSpan.FromSeconds(30);
        });
    }

    private static void DefaultMessageRetryIntervals(IRetryConfigurator retry) => retry.Intervals(
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15)
    );
}
