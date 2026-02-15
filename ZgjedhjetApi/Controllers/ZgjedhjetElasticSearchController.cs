using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using StackExchange.Redis;
using System.Text.Json;
using ZgjedhjetApi.Data;
using ZgjedhjetApi.Enums;
using ZgjedhjetApi.Models.DTOs;
using ZgjedhjetApi.Models.Entities;

namespace ZgjedhjetApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZgjedhjetElasticSearchController : ControllerBase
    {
        private readonly ILogger<ZgjedhjetElasticSearchController> _logger;
        private readonly LifeDbContext _context;
        private readonly IElasticClient _elasticClient;
        private readonly IConnectionMultiplexer _redis;
        private const string IndexName = "zgjedhjet";

        public ZgjedhjetElasticSearchController(
            ILogger<ZgjedhjetElasticSearchController> logger,
            LifeDbContext context,
            IElasticClient elasticClient,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _context = context;
            _elasticClient = elasticClient;
            _redis = redis;
        }

        /// <summary>
        /// POST endpoint to migrate data from SQL to Elasticsearch
        /// </summary>
        [HttpPost("migrate")]
        public async Task<ActionResult<MigrationResponse>> MigrateToElasticsearch()
        {
            try
            {
                // Create index with custom mapping if it doesn't exist
                var indexExists = await _elasticClient.Indices.ExistsAsync(IndexName);
                
                if (!indexExists.Exists)
                {
                    var createIndexResponse = await _elasticClient.Indices.CreateAsync(IndexName, c => c
                        .Settings(s => s
                            .Analysis(a => a
                                .Analyzers(an => an
                                    .Custom("komuna_analyzer", ca => ca
                                        .Tokenizer("standard")
                                        .Filters("lowercase", "asciifolding")
                                    )
                                )
                            )
                        )
                        .Map<ZgjedhjetDocument>(m => m
                            .Properties(p => p
                                .Keyword(k => k.Name(n => n.Kategoria))
                                .Text(t => t
                                    .Name(n => n.Komuna)
                                    .Analyzer("komuna_analyzer")
                                    .Fields(f => f
                                        .Keyword(k => k.Name("keyword"))
                                    )
                                )
                                .Keyword(k => k.Name(n => n.Qendra_e_Votimit))
                                .Keyword(k => k.Name(n => n.VendVotimi))
                                .Number(n => n.Name(d => d.Partia111).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia112).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia113).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia114).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia115).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia116).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia117).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia118).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia119).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia120).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia121).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia122).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia123).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia124).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia125).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia126).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia127).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia128).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia129).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia130).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia131).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia132).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia133).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia134).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia135).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia136).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia137).Type(NumberType.Integer))
                                .Number(n => n.Name(d => d.Partia138).Type(NumberType.Integer))
                            )
                        )
                    );

                    if (!createIndexResponse.IsValid)
                    {
                        _logger.LogError("Failed to create index: {Error}", createIndexResponse.DebugInformation);
                        return StatusCode(500, new MigrationResponse
                        {
                            Success = false,
                            Message = "Failed to create Elasticsearch index",
                            Error = createIndexResponse.ServerError?.Error?.Reason
                        });
                    }
                }

                // Read all data from SQL database
                var sqlData = await _context.Zgjedhjet.ToListAsync();

                if (!sqlData.Any())
                {
                    return Ok(new MigrationResponse
                    {
                        Success = true,
                        Message = "No data to migrate",
                        RecordsMigrated = 0
                    });
                }

                // Transform to Elasticsearch documents
                var documents = sqlData.Select(z => new ZgjedhjetDocument
                {
                    Id = z.Id,
                    Kategoria = z.Kategoria,
                    Komuna = z.Komuna,
                    Qendra_e_Votimit = z.Qendra_e_Votimit,
                    VendVotimi = z.VendVotimi,
                    Partia111 = z.Partia111,
                    Partia112 = z.Partia112,
                    Partia113 = z.Partia113,
                    Partia114 = z.Partia114,
                    Partia115 = z.Partia115,
                    Partia116 = z.Partia116,
                    Partia117 = z.Partia117,
                    Partia118 = z.Partia118,
                    Partia119 = z.Partia119,
                    Partia120 = z.Partia120,
                    Partia121 = z.Partia121,
                    Partia122 = z.Partia122,
                    Partia123 = z.Partia123,
                    Partia124 = z.Partia124,
                    Partia125 = z.Partia125,
                    Partia126 = z.Partia126,
                    Partia127 = z.Partia127,
                    Partia128 = z.Partia128,
                    Partia129 = z.Partia129,
                    Partia130 = z.Partia130,
                    Partia131 = z.Partia131,
                    Partia132 = z.Partia132,
                    Partia133 = z.Partia133,
                    Partia134 = z.Partia134,
                    Partia135 = z.Partia135,
                    Partia136 = z.Partia136,
                    Partia137 = z.Partia137,
                    Partia138 = z.Partia138
                }).ToList();

                // Bulk index documents
                var bulkResponse = await _elasticClient.BulkAsync(b => b
                    .Index(IndexName)
                    .IndexMany(documents)
                );

                if (!bulkResponse.IsValid)
                {
                    _logger.LogError("Bulk indexing failed: {Error}", bulkResponse.DebugInformation);
                    return StatusCode(500, new MigrationResponse
                    {
                        Success = false,
                        Message = "Failed to index documents",
                        Error = bulkResponse.ServerError?.Error?.Reason
                    });
                }

                _logger.LogInformation("Successfully migrated {Count} records to Elasticsearch", documents.Count);

                return Ok(new MigrationResponse
                {
                    Success = true,
                    Message = $"Successfully migrated {documents.Count} records to Elasticsearch",
                    RecordsMigrated = documents.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration to Elasticsearch");
                return StatusCode(500, new MigrationResponse
                {
                    Success = false,
                    Message = "Internal server error during migration",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET endpoint to retrieve and filter electoral data from Elasticsearch
        /// Identical to Assignment 1 - Point 4, but reads from Elasticsearch instead of SQL
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ZgjedhjetAggregatedResponse>> GetZgjedhjet(
            [FromQuery] Kategoria? kategoria = null,
            [FromQuery] Komuna? komuna = null,
            [FromQuery] string? qendra_e_votimit = null,
            [FromQuery] string? vendvotimi = null,
            [FromQuery] Partia? partia = null)
        {
            try
            {
                // Build Elasticsearch query
                var searchRequest = new SearchRequest<ZgjedhjetDocument>(IndexName)
                {
                    Size = 10000, // Retrieve all documents
                    Query = BuildQuery(kategoria, komuna, qendra_e_votimit, vendvotimi)
                };

                var searchResponse = await _elasticClient.SearchAsync<ZgjedhjetDocument>(searchRequest);

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch query failed: {Error}", searchResponse.DebugInformation);
                    return StatusCode(500, new { message = "Error querying Elasticsearch" });
                }

                // Handle 404 for specific filters
                if (!string.IsNullOrWhiteSpace(qendra_e_votimit) && !searchResponse.Documents.Any())
                {
                    return NotFound(new { message = $"Qendra e Votimit '{qendra_e_votimit}' not found" });
                }

                if (!string.IsNullOrWhiteSpace(vendvotimi) && !searchResponse.Documents.Any())
                {
                    return NotFound(new { message = $"VendVotimi '{vendvotimi}' not found" });
                }

                var data = searchResponse.Documents.ToList();
                var response = new ZgjedhjetAggregatedResponse();

                // Aggregate votes per party (identical logic to Assignment 1)
                if (partia.HasValue && partia.Value != Partia.TeGjitha)
                {
                    var partiaName = partia.Value.ToString();
                    var totalVota = CalculatePartiaTotalVota(data, partiaName);

                    response.Results.Add(new PartiaVotesResponse
                    {
                        Partia = partiaName,
                        TotalVota = totalVota
                    });
                }
                else
                {
                    // Aggregate votes for all parties
                    for (int i = 111; i <= 138; i++)
                    {
                        var partiaName = $"Partia{i}";
                        var totalVota = CalculatePartiaTotalVota(data, partiaName);

                        response.Results.Add(new PartiaVotesResponse
                        {
                            Partia = partiaName,
                            TotalVota = totalVota
                        });
                    }
                }

                _logger.LogInformation("Retrieved {Count} party results from Elasticsearch", response.Results.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered data from Elasticsearch");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// GET endpoint for municipality suggestion/autocomplete
        /// </summary>
        [HttpGet("suggest")]
        public async Task<ActionResult<List<string>>> SuggestKomuna(
            [FromQuery] string query,
            [FromQuery] int top = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Ok(new List<string>());
                }

                // Search for municipalities using match_phrase_prefix for prefix matching
                var searchResponse = await _elasticClient.SearchAsync<ZgjedhjetDocument>(s => s
                    .Index(IndexName)
                    .Size(0)
                    .Aggregations(a => a
                        .Terms("unique_komunat", t => t
                            .Field(f => f.Komuna.Suffix("keyword"))
                            .Size(10000)
                        )
                    )
                    .Query(q => q
                        .MatchPhrasePrefix(m => m
                            .Field(f => f.Komuna)
                            .Query(query)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Suggest query failed: {Error}", searchResponse.DebugInformation);
                    return Ok(new List<string>());
                }

                // Extract unique municipalities from aggregation
                var termsAgg = searchResponse.Aggregations.Terms("unique_komunat");
                var suggestions = termsAgg.Buckets
                    .Select(b => b.Key)
                    .Take(top)
                    .ToList();

                // Record statistics in Redis for each suggested municipality
                if (suggestions.Any())
                {
                    await RecordSuggestionsInRedis(suggestions);
                }

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during municipality suggestion");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// GET endpoint to retrieve statistics of most suggested municipalities from Redis
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<List<KomunaSuggestionStatistic>>> GetSuggestionStatistics(
            [FromQuery] int top = 10)
        {
            try
            {
                var db = _redis.GetDatabase();
                var statistics = new List<KomunaSuggestionStatistic>();

                // Get all komuna keys from Redis sorted set
                var sortedEntries = await db.SortedSetRangeByRankWithScoresAsync(
                    "komuna:suggestions",
                    0,
                    -1,
                    Order.Descending
                );

                foreach (var entry in sortedEntries.Take(top))
                {
                    statistics.Add(new KomunaSuggestionStatistic
                    {
                        Komuna = entry.Element.ToString(),
                        NrISugjerimeve = (int)entry.Score
                    });
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suggestion statistics from Redis");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Helper methods

        private QueryContainer BuildQuery(
            Kategoria? kategoria,
            Komuna? komuna,
            string? qendra_e_votimit,
            string? vendvotimi)
        {
            var mustQueries = new List<QueryContainer>();

            if (kategoria.HasValue && kategoria.Value != Kategoria.TeGjitha)
            {
                mustQueries.Add(new TermQuery
                {
                    Field = "kategoria",
                    Value = kategoria.Value.ToString()
                });
            }

            if (komuna.HasValue && komuna.Value != Komuna.TeGjitha)
            {
                mustQueries.Add(new TermQuery
                {
                    Field = "komuna.keyword",
                    Value = komuna.Value.ToString()
                });
            }

            if (!string.IsNullOrWhiteSpace(qendra_e_votimit))
            {
                mustQueries.Add(new TermQuery
                {
                    Field = "qendra_e_Votimit",
                    Value = qendra_e_votimit
                });
            }

            if (!string.IsNullOrWhiteSpace(vendvotimi))
            {
                mustQueries.Add(new TermQuery
                {
                    Field = "vendVotimi",
                    Value = vendvotimi
                });
            }

            if (!mustQueries.Any())
            {
                return new MatchAllQuery();
            }

            return new BoolQuery
            {
                Must = mustQueries
            };
        }

        private int CalculatePartiaTotalVota(List<ZgjedhjetDocument> data, string partiaName)
        {
            var propertyInfo = typeof(ZgjedhjetDocument).GetProperty(partiaName);
            if (propertyInfo == null)
                return 0;

            return data.Sum(z => (int)(propertyInfo.GetValue(z) ?? 0));
        }

        private async Task RecordSuggestionsInRedis(List<string> suggestions)
        {
            try
            {
                var db = _redis.GetDatabase();

                // Increment suggestion count for each municipality using sorted set
                foreach (var komuna in suggestions)
                {
                    await db.SortedSetIncrementAsync("komuna:suggestions", komuna, 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record suggestions in Redis");
            }
        }
    }

    // Elasticsearch document model
    public class ZgjedhjetDocument
    {
        public int Id { get; set; }
        public string Kategoria { get; set; } = string.Empty;
        public string Komuna { get; set; } = string.Empty;
        public string Qendra_e_Votimit { get; set; } = string.Empty;
        public string VendVotimi { get; set; } = string.Empty;
        public int Partia111 { get; set; }
        public int Partia112 { get; set; }
        public int Partia113 { get; set; }
        public int Partia114 { get; set; }
        public int Partia115 { get; set; }
        public int Partia116 { get; set; }
        public int Partia117 { get; set; }
        public int Partia118 { get; set; }
        public int Partia119 { get; set; }
        public int Partia120 { get; set; }
        public int Partia121 { get; set; }
        public int Partia122 { get; set; }
        public int Partia123 { get; set; }
        public int Partia124 { get; set; }
        public int Partia125 { get; set; }
        public int Partia126 { get; set; }
        public int Partia127 { get; set; }
        public int Partia128 { get; set; }
        public int Partia129 { get; set; }
        public int Partia130 { get; set; }
        public int Partia131 { get; set; }
        public int Partia132 { get; set; }
        public int Partia133 { get; set; }
        public int Partia134 { get; set; }
        public int Partia135 { get; set; }
        public int Partia136 { get; set; }
        public int Partia137 { get; set; }
        public int Partia138 { get; set; }
    }
}