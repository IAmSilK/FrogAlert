using FrogAlert.Auth;
using FrogAlert.Database;
using FrogAlert.Database.Models;
using FrogAlert.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FrogAlert.Controllers
{
    [ApiController]
    [ApiKeyFilter]
    [Route("[controller]")]
    public class EnvironmentController : ControllerBase
    {
        private readonly ILogger<EnvironmentController> _logger;
        private readonly FrogAlertDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public EnvironmentController(ILogger<EnvironmentController> logger,
            FrogAlertDbContext dbContext,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets a list of environment snapshots based on the given filters. Sorted in order of newest to oldest.
        /// </summary>
        /// <param name="count">Number of environment snapshots to be returned.</param>
        /// <param name="skip">Number of environment snapshots to be skipped.</param>
        /// <param name="startTime">(Optional) Earliest time/date of environment snapshots (inclusive). Format: <code>2022-02-19T16:04:14.291Z</code></param>
        /// <param name="endTime">(Optional) Latest time/date of environment snapshots (exclusive). Format: <code>2022-02-19T16:04:14.291Z</code></param>
        /// <returns>List of environment snapshots.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<EnvironmentSnapshotDto>> ListAsync(
            [Range(0, 500)] int count = 100,
            int skip = 0,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null)
        {
            IQueryable<EnvironmentSnapshot> environmentSnapshots = _dbContext.Environment.OrderByDescending(x => x.Time);

            if (startTime.HasValue)
            {
                environmentSnapshots = environmentSnapshots.Where(x => x.Time >= startTime);
            }

            if (endTime.HasValue)
            {
                environmentSnapshots = environmentSnapshots.Where(x => x.Time < endTime);
            }

            environmentSnapshots = environmentSnapshots.Skip(skip).Take(count);

            var environmentSnapshotsList = await environmentSnapshots.ToListAsync();

            return environmentSnapshotsList.Select(snapshot => new EnvironmentSnapshotDto()
            {
                Time = snapshot.Time,
                Humidity = snapshot.Humidity,
                TempC = snapshot.TempC
            });
        }

        /// <summary>
        /// Adds an environment snapshot with the given properties to the database.
        /// </summary>
        /// <param name="tempC">The temperature to be recorded (in celsius).</param>
        /// <param name="humidity">The humidity to be recorded (in percent, for example, 70 is 70%).</param>
        /// <param name="time">(Optional) The time the snapshot was taken. If not specified, will use current time during request processing. Format: <code>2022-02-19T16:04:14.291Z</code></param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitAsync(
            float tempC,
            [Range(0, float.MaxValue)] float humidity,
            DateTimeOffset? time = null)
        {
            if (!time.HasValue)
            {
                time = DateTimeOffset.UtcNow;
            }
            else
            {
                time = time.Value.ToUniversalTime();

                if (time > DateTimeOffset.UtcNow)
                {
                    return BadRequest("Time cannot be in future");
                }
            }

            var maxAge = _configuration.GetSection("EnvironmentSnapshots").GetValue<float>("MaxSubmitAgeSeconds");
            
            if ((DateTimeOffset.UtcNow - time.Value).TotalSeconds > maxAge)
            {
                return BadRequest($"Time cannot be more than {maxAge:#} seconds in the past");
            }

            var environmentSnapshot = new EnvironmentSnapshot
            {
                Time = time.Value,
                TempC = tempC,
                Humidity = humidity
            };

            _logger.LogDebug("New environment snapshot. Time: {Time} Temperature: {Temperature} Humidity: {Humidity}",
                environmentSnapshot.Time, environmentSnapshot.TempC, environmentSnapshot.Humidity);

            await _dbContext.Environment.AddAsync(environmentSnapshot);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}