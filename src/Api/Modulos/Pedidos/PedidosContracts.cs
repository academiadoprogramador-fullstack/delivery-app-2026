namespace DeliveryApp.WebApi.Modulos.Pedidos;

public sealed record ItemCriarPedidoRequest(
    Guid ProdutoId,
    uint Quantidade,
    string? Observacao,
    IReadOnlyList<Guid> ComplementosIds
);

public sealed record CriarPedidoRequest(
    Guid EstabelecimentoId,
    string EnderecoEntrega,
    IReadOnlyList<ItemCriarPedidoRequest> Itens
);

public sealed record CriarPedidoResponse(Guid PedidoId);
