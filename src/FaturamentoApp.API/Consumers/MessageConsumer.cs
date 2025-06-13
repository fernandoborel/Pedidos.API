using Dapper;
using Microsoft.Data.SqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FaturamentoApp.API.Consumers;

/// <summary>
/// Serviço para ler e processar mensagens de fila.
/// </summary>
public class MessageConsumer : BackgroundService
{
    private readonly string _connectionString;
    private readonly string _host;
    private readonly string _queue;

    public MessageConsumer(string connectionString, string host, string queue)
    {
        _connectionString = connectionString;
        _host = host;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //conectando no RabbitMQ
        var factory = new ConnectionFactory() { HostName = _host };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        //acessando a fila
        channel.QueueDeclare(queue: _queue,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        //rotina de leitura da fila de forma incremental
        consumer.Received += async (sender, args) =>
        {
            //lendo a mensagem da fila
            var payload = args.Body.ToArray();

            //ler a mensagem em bytes
            var message = Encoding.UTF8.GetString(payload);

            var pedido = JsonSerializer.Deserialize<Pedido>(message);

            //gravar os dados do faturamente no banco de dados
            await RegistrarFaturamento(pedido);

            //confirmação de recebimento da mensagem
            channel.BasicAck(args.DeliveryTag, false);
        };

        //fazendo a execução do consumer
        channel.BasicConsume(
            queue: _queue,
            autoAck: false,
            consumer: consumer);
    }

    private async Task RegistrarFaturamento(Pedido pedido)
    {
        var query = @"
             INSERT INTO FATURAMENTO(ID, VALOR, DATAHORA, DADOS, PEDIDO_ID)
             VALUES(@Id, @Valor, @DataHora, @Dados, @PedidoId)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(query, new
            {
                @Id = Guid.NewGuid(),
                @Valor = pedido.Valor,
                @DataHora = pedido.DataHora,
                @Dados = $"Cliente: {pedido.Cliente}, Detalhes: {pedido.Detalhes}", @PedidoId = pedido.Id });
        }
    } 
}


public class Pedido
{
    public Guid? Id { get; set; }
    public DateTime? DataHora { get; set; }
    public string? Cliente { get; set; }
    public decimal? Valor { get; set; }
    public string? Detalhes { get; set; }
}
