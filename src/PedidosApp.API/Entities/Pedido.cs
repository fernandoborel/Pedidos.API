namespace PedidosApp.API.Entities;

public class Pedido
{
    public Guid? Id { get; set; }
    public DateTime? DataHora { get; set; }
    public decimal Valor { get; set; }
    public string? Cliente { get; set; }
    public string? Detalhes { get; set; }
}