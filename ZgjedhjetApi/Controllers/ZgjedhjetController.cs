using Microsoft.AspNetCore.Mvc;
using ZgjedhjetApi.Data;
using ZgjedhjetApi.Enums;
using ZgjedhjetApi.Models.DTOs;
using ZgjedhjetApi.Models.Entities;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ZgjedhjetApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZgjedhjetController : ControllerBase
    {
        // YOUR CODE HERE
        // IT IS UP TO YOU TO DECIDE IF YOU WILL USE DB CONTEXT HERE OR THROUGH SERVICES/REPOSITORIES
        private readonly ILogger<ZgjedhjetController> _logger;
        private readonly LifeDbContext _context;

        public ZgjedhjetController(ILogger<ZgjedhjetController> logger, LifeDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// POST endpoint to import CSV file
        /// </summary>
        [HttpPost("import")]
        public async Task<ActionResult<CsvImportResponse>> MigrateData(IFormFile file)
        {
            // YOUR CODE HERE
            var response = new CsvImportResponse();
            
            if (file == null || file.Length == 0)
            {
                response.Success = false;
                response.Message = "No file uploaded";
                response.Errors.Add("File is required");
                return BadRequest(response);
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                response.Success = false;
                response.Message = "Invalid file format";
                response.Errors.Add("Only CSV files allowed");
                return BadRequest(response);
            }

            var records = new List<Zgjedhjet>();
            var lineNumber = 0;

            try
            {
                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);

                // Read header and respond with error message if header is not found meaning file is empty
                var header = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(header))
                {
                    response.Success = false;
                    response.Message = "CSV file is empty";
                    response.Errors.Add("No header found in CSV");
                    return BadRequest(response);
                }

                lineNumber++;

                // Process each line 
                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var values = ParseCsvLine(line);

                        if (values.Length < 32)     // 4 basic fields + 28 parties
                        {
                            response.Errors.Add($"Line {lineNumber}: Insufficient columns");
                            continue;
                        }

                        var record = new Zgjedhjet
                        {
                            Kategoria = values[0]?.Trim() ?? string.Empty,
                            Komuna = values[1]?.Trim() ?? string.Empty,
                            Qendra_e_Votimit = values[2]?.Trim() ?? string.Empty,
                            VendVotimi = values[3]?.Trim() ?? string.Empty,
                            Partia111 = ParseInt(values[4]),
                            Partia112 = ParseInt(values[5]),
                            Partia113 = ParseInt(values[6]),
                            Partia114 = ParseInt(values[7]),
                            Partia115 = ParseInt(values[8]),
                            Partia116 = ParseInt(values[9]),
                            Partia117 = ParseInt(values[10]),
                            Partia118 = ParseInt(values[11]),
                            Partia119 = ParseInt(values[12]),
                            Partia120 = ParseInt(values[13]),
                            Partia121 = ParseInt(values[14]),
                            Partia122 = ParseInt(values[15]),
                            Partia123 = ParseInt(values[16]),
                            Partia124 = ParseInt(values[17]),
                            Partia125 = ParseInt(values[18]),
                            Partia126 = ParseInt(values[19]),
                            Partia127 = ParseInt(values[20]),
                            Partia128 = ParseInt(values[21]),
                            Partia129 = ParseInt(values[22]),
                            Partia130 = ParseInt(values[23]),
                            Partia131 = ParseInt(values[24]),
                            Partia132 = ParseInt(values[25]),
                            Partia133 = ParseInt(values[26]),
                            Partia134 = ParseInt(values[27]),
                            Partia135 = ParseInt(values[28]),
                            Partia136 = ParseInt(values[29]),
                            Partia137 = ParseInt(values[30]),
                            Partia138 = ParseInt(values[31]),
                        };

                        records.Add(record);
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Line {lineNumber}: {ex.Message}");
                        _logger.LogWarning(ex, "Error parsing line {LineNumber}", lineNumber);
                    }
                }

                // Save to database
                if (records.Any())
                {
                    await _context.Zgjedhjet.AddRangeAsync(records);
                    await _context.SaveChangesAsync();

                    response.Success = true;
                    response.Message = $"Successfully imported {records.Count} records";
                    response.RecordsImported = records.Count;

                    _logger.LogInformation("Successfully imported {Count} records", records.Count);
                }
                else
                {
                    response.Success = false;
                    response.Message = "No valid records found in CSV";
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CSV import");

                response.Success = false;
                response.Message = "Internal server error during import";
                response.Errors.Add(ex.Message);

                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// GET endpoint to retrieve and filter electoral data
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
                // Starting with all the data available
                var query = _context.Zgjedhjet.AsQueryable();

                // Applying filters to the data
                if (kategoria.HasValue && kategoria.Value != Kategoria.TeGjitha)
                {
                    var kategoriaStr = kategoria.Value.ToString();
                    query = query.Where(z => z.Kategoria == kategoriaStr);
                }

                if (komuna.HasValue && komuna.Value != Komuna.TeGjitha)
                {
                    var komunaStr = komuna.Value.ToString();
                    query = query.Where(z => z.Komuna == komunaStr);
                }

                if (!string.IsNullOrWhiteSpace(qendra_e_votimit))
                {
                    var exists = await _context.Zgjedhjet
                        .AnyAsync(z => z.Qendra_e_Votimit == qendra_e_votimit);

                    if (!exists)
                    {
                        _logger.LogWarning("Qendra e Votimit not found: {QendraVotimit}", qendra_e_votimit);
                        return NotFound(new { message = $"Qendra e Votimit '{qendra_e_votimit}' not found" });
                    }

                    query = query.Where(z => z.Qendra_e_Votimit == qendra_e_votimit);
                }

                if (!string.IsNullOrWhiteSpace(vendvotimi))
                {
                    var exists = await _context.Zgjedhjet
                        .AnyAsync(z => z.VendVotimi == vendvotimi);

                    if (!exists)
                    {
                        _logger.LogWarning("VendVotimi not found: {VendVotimi}", vendvotimi);
                        return NotFound(new { message = $"VendVotimi '{vendvotimi}' not found" });
                    }

                    query = query.Where(z => z.VendVotimi == vendvotimi);
                }

                var data = await query.ToListAsync();

                var response = new ZgjedhjetAggregatedResponse();

                // Rezultate e Agreguara (Aggregated Results)
                if (partia.HasValue && partia.Value != Partia.TeGjitha)
                {
                    // Returning only the selected party
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
                    // Returning all parties
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

                _logger.LogInformation("Retrieved {Count} party results", response.Results.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered data");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private int CalculatePartiaTotalVota(List<Zgjedhjet> data, string partiaName)
        {
            var propertyInfo = typeof(Zgjedhjet).GetProperty(partiaName);
            if (propertyInfo == null)
                return 0;

            return data.Sum(z => (int)(propertyInfo.GetValue(z) ?? 0));
        }

        private string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            var insideQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (c == ',' && !insideQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString());
            return values.ToArray();
        }

        private int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (int.TryParse(value.Trim(), out int result))
                return result;

            return 0;
        }
    }
}
