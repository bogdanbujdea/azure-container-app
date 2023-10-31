using Gremlin.Net.Driver;
using Microsoft.Extensions.Options;

namespace Exercises.API
{
    public static class GremlinWrapper
    {
        public static void Initialize(ConfigurationManager config)
        {
            Configuration = config;
        }

        public static ConfigurationManager Configuration { get; set; }

        public static async Task<ResultSet<dynamic>> CreateEntity(string type, string name)
        {
            var gremlinClient = CreateGremlinClient();
            var vertexLabel = type;
            var vertexType = type;
            var vertexName = name;
            return await gremlinClient.SubmitAsync<dynamic>($"g.addV('{vertexLabel}').property('type', '${vertexType}').property('name', '{vertexName}')");
        }


        private static GremlinClient CreateGremlinClient()
        {
            var gremlinServer = new GremlinServer(
                hostname: "exercises-graph-db.gremlin.cosmosdb.azure.com",
                port: 443,
                enableSsl: true,
                username: "/dbs/Exercises/colls/exercisesdb",
                password: Configuration["GREMLIN_SERVER_PASSWORD"]);
            var gremlinClient = new GremlinClient(gremlinServer,
                new Gremlin.Net.Structure.IO.GraphSON.GraphSON2MessageSerializer());
            return gremlinClient;
        }

        public static async Task<ResultSet<dynamic>> LinkEntities(string firstEntity, string secondEntity, string relationshipName)
        {
            var gremlinClient = CreateGremlinClient();
            return await gremlinClient.SubmitAsync<dynamic>($"g.V().has('name', '{firstEntity}')" +
                                                     $".addE('{relationshipName}')" +
                                                     $".to(g.V().has('name', '{secondEntity}'))");
        }

        public static async Task<bool> VertexExistsAsync(string vertexName)
        {
            var gremlinClient = CreateGremlinClient();
            var query = $"g.V().has('name', '{vertexName}')";
            var resultSet = await gremlinClient.SubmitAsync<dynamic>(query);

            return resultSet.Count > 0;
        }

    }

    public class EnvVariables
    {
        public string GremlinServerPassword { get; set; }
        public string SqlConnectionString { get; set; }
    }
}
