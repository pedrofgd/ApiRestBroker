using ApiBroker.API.Configuracoes;
using ApiBroker.API.Healthcheck;

namespace ApiBroker.API.Inicializacao;

public class Inicializador
{
    private readonly ILogger<Inicializador> _logger;

    public Inicializador()
    {
        _logger = BrokerLoggerFactory.Factory().CreateLogger<Inicializador>();
    }

    public void Iniciar(IConfiguration configuration)
    {
        var recursos = ConfiguracoesUtils.ObterTodosRecursos(configuration);
        foreach (var recurso in recursos)
        {
            if (!recurso.Provedores.Any())
                continue;

            foreach (var provedor in recurso.Provedores)
            {
                _logger.LogInformation("Inicializando {NomeRecurso}/{NomeProvedor}",
                    recurso.Nome, provedor.Nome);
                CheckFireAndForget(recurso.Nome, provedor);
            }
        }
    }

    private void CheckFireAndForget(string nomeRecurso, ProvedorSettings provedor)
    {
        var healthchecker = new Healthchecker();
#pragma warning disable CS4014
        healthchecker.Check(nomeRecurso, provedor);
#pragma warning restore CS4014
    }
}