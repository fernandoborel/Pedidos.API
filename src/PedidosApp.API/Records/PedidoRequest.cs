namespace PedidosApp.API.Records;

/// <summary>
/// Modelo de dados para requisições de pedidos.
/// </summary>
public record PedidoRequest
(
    string Cliente,
    decimal Valor,
    string Detalhes
);