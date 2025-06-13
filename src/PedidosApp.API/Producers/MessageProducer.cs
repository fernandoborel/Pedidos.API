using PedidosApp.API.Entities;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PedidosApp.API.Producers;

/// <summary>
/// Classe para que possamos gravar mensagens na fila do RabbitMQ.
/// </summary>
public class MessageProducer
{
    private readonly string _host;
    private readonly string _queue;

    public MessageProducer(string host, string queue)
    {
        _host = host;
        _queue = queue;
    }

    public void SendMessage(Pedido pedido)
    {
        //Conectando no RabbitMQ
        var factory = new ConnectionFactory() { HostName = _host };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Criar a fila se ela não existir
        channel.QueueDeclare(queue: _queue,
                             exclusive: false, //Fila pode ser acessada por várias conexões simultaneamente(aplicações)
                             durable: false,
                             autoDelete: false,
                             arguments: null);

        //Serializar o pedido para JSON
        var mensagem = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pedido));

        //publicando / gravando a mensagem na fila
        channel.BasicPublish(exchange: "", //Sem exchange, a mensagem vai direto para a fila
                             routingKey: _queue,
                             basicProperties: null,
                             body: mensagem);

    }
}