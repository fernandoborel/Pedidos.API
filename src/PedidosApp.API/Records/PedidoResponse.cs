namespace PedidosApp.API.Records;

/// <summary>
/// Modelo de dados para as respostas de pedidos.
/// </summary>
public record PedidoResponse
(
    Guid Id,
    string Cliente,
    decimal Valor,
    DateTime DataHora,
    string Detalhes
);