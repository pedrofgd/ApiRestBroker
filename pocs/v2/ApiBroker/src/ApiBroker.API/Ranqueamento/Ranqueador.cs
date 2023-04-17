using ApiBroker.API.Identificacao;

namespace ApiBroker.API.Ranqueamento;

public class Ranqueador
{
    public async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao)
    {
        var nomeRecurso = solicitacao.Nome;

        var provedores = await ObterTodosProvedores(nomeRecurso);
        
        var criterios = solicitacao.Criterios;
        var provedoresDisponiveis = provedores
            .Where(x => (int)x["error_rate"] < criterios.ErrorBudgetHora)
            .OrderBy(x => x["response_time"])
            .Select(p => (string)p["name"])
            .ToList();

        return !provedoresDisponiveis.Any() ? new List<string>() : provedoresDisponiveis;
    }

    private async Task<List<Dictionary<string, object>>> ObterTodosProvedores(string nomeRecurso)
    {
        using var influx = InfluxDbClientFactory.OpenConnection();
        var queryApi = influx.GetQueryApi();

        var fluxTables = await queryApi.QueryAsync(Query(nomeRecurso), "broker");

        var providers = (from fluxTable in fluxTables
            from fluxRecord in fluxTable.Records
            select new Dictionary<string, object>
            {
                { "name", fluxRecord.GetValueByKey("provider") },
                { "response_time", Convert.ToDouble(fluxRecord.GetValueByKey("mean_latency")) },
                { "error_rate", Convert.ToInt32(fluxRecord.GetValueByKey("error_count")) }
            }).ToList();

        return providers;
    }
    
    private static string Query(string nomeRecurso)
    {
        /*
         * todo: rever range na consulta (talvez ser parte da configuração do cliente)
         * todo: utilizando "logs-alt" por enquanto, mudar para "logs"
         *  Ajustar script para marcar errorCount como 0 caso a consulta não encontre
         *  nenhum registro em que sucesso._value = 0
         */
        return 
            "ranking = () => {\n" +
            "    meanLatency = from(bucket: \"logs\")\n" +
            "        |> range(start: -1h)\n" +
            "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
            $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
            "        |> filter(fn: (r) => r[\"_field\"] == \"latencia\")\n" +
            "        |> mean()\n" +
            "    errorCount = from(bucket: \"logs\")\n" +
            "        |> range(start: -1h)\n" +
            "        |> filter(fn: (r) => r[\"_measurement\"] == \"metricas_recursos\")\n" +
            "        |> filter(fn: (r) => r[\"_field\"] == \"sucesso\")\n" +
            $"       |> filter(fn: (r) => r[\"nome_recurso\"] == \"{nomeRecurso}\")\n" +
            "        |> filter(fn: (r) => r[\"_value\"] == 0, onEmpty: \"keep\")\n" +
            "        |> count()\n" +
            "    return join(tables: {meanLatency: meanLatency, errorCount: errorCount}, on: [\"nome_provedor\"])\n" +
            "        |> map(fn: (r) => ({provider: r.nome_provedor, mean_latency: r._value_meanLatency, " +
            "               error_count: r._value_errorCount}))\n" +
            "        |> sort(columns: [\"error_count\"])\n" +
            "        |> yield(name: \"ranking\")\n" +
            "}\n" +
            "ranking()";
    }
}