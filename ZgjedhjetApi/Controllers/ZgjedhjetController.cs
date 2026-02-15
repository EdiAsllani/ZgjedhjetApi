using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZgjedhjetApi.Data;
using ZgjedhjetApi.Enums;
using ZgjedhjetApi.Models.DTOs;
using ZgjedhjetApi.Models.Entities;

namespace ZgjedhjetApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZgjedhjetController : ControllerBase
    {
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
            var response = new CsvImportResponse();
            
            // Check if file was provided
            if (file == null || file.Length == 0)
            {
                response.Success = false;
                response.Message = "No file uploaded";
                response.Errors.Add("File is required");
                return BadRequest(response);
            }

            // Ensure the file is a CSV
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

                // Read the header line; return error if file is empty
                var header = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(header))
                {
                    response.Success = false;
                    response.Message = "CSV file is empty";
                    response.Errors.Add("No header found in CSV");
                    return BadRequest(response);
                }

                lineNumber++;

                // Process each CSV line
                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line))
                        continue; // skip empty lines

                    try
                    {
                        var values = ParseCsvLine(line);

                        // Validate expected number of columns
                        if (values.Length < 32)
                        {
                            response.Errors.Add($"Line {lineNumber}: Insufficient columns");
                            continue;
                        }

                        // Map CSV values to entity
                        var record = new Zgjedhjet
                        {
                            Kategoria = values[0]?.Trim() ?? string.Empty,
                            Komuna = values[1]?.Trim() ?? string.Empty,
                            Qendra_e_Votimit = values[2]?.Trim() ?? string.Empty,
                            VendVotimi = values[3]?.Trim() ?? string.Empty,
                            Partia111 = ParseInt(values[4]),
                            // ... other parties ...
                            Partia138 = ParseInt(values[31]),
                        };

                        records.Add(record);
                    }
                    catch (Exception ex)
                    {
                        // Log and record errors for individual lines without stopping import
                        response.Errors.Add($"Line {lineNumber}: {ex.Message}");
                        _logger.LogWarning(ex, "Error parsing line {LineNumber}", lineNumber);
                    }
                }

                // Save all valid records to database
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
                // Catch any unexpected error during the import
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
                var query = _context.Zgjedhjet.AsQueryable();

                // Apply filters if provided
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
                    // Check if the specified voting center exists
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
                    // Check if the specified voting place exists
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

                // Aggregate votes per party
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

                _logger.LogInformation("Retrieved {Count} party results", response.Results.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Catch any unexpected error when querying/filtering
                _logger.LogError(ex, "Error retrieving filtered data");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Helper to sum votes for a specific party
        private int CalculatePartiaTotalVota(List<Zgjedhjet> data, string partiaName)
        {
            var propertyInfo = typeof(Zgjedhjet).GetProperty(partiaName);
            if (propertyInfo == null)
                return 0;

            return data.Sum(z => (int)(propertyInfo.GetValue(z) ?? 0));
        }

        // Parse CSV line respecting quoted values
        private string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            var insideQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                    insideQuotes = !insideQuotes;
                else if (c == ',' && !insideQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                    currentValue.Append(c);
            }

            values.Add(currentValue.ToString());
            return values.ToArray();
        }

        // Safely parse integer, return 0 if invalid
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
