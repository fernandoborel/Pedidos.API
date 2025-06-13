using Microsoft.AspNetCore.Mvc;
using PedidosApp.API.Entities;
using PedidosApp.API.Producers;
using PedidosApp.API.Records;
using PedidosApp.API.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

//Registrar o repositório de pedidos como um serviço
builder.Services.AddScoped(
    map => new PedidoRepository(builder.Configuration.GetConnectionString("PedidosBD")));

//Registrar o MessageProducer como um serviço
builder.Services.AddScoped(
    map => new MessageProducer(builder.Configuration["RabbitMQ:Host"], builder.Configuration["RabbitMQ:Queue"]));


var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

//ENDPOINT para cadastro de pedidos
app.MapPost("/api/pedidos", async ([FromBody] PedidoRequest request, PedidoRepository pedidoRepository, 
    MessageProducer messageProducer) =>
{
    //capturar os dados do pedido
    var pedido = new Pedido
    {
        Id = Guid.NewGuid(),
        DataHora = DateTime.Now,
        Cliente = request.Cliente,
        Valor = request.Valor,
        Detalhes = request.Detalhes
    };

    //inserir o pedido no banco de dados
    await pedidoRepository.Inserir(pedido);

    //enviar o pedido para a fila RabbitMQ
    messageProducer.SendMessage(pedido);

    //retornar o pedido inserido
    return Results.Created("/api/pedidos/", new PedidoResponse(
            pedido.Id.Value,
            pedido.Cliente ?? string.Empty,
            pedido.Valor,
            pedido.DataHora.Value,
            pedido.Detalhes ?? string.Empty
        ));
}).Produces<PedidoResponse>(StatusCodes.Status201Created);

app.Run();