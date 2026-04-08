using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
internal class DistributedJobRepository : IDistributedJobRepository
{
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;

    public DistributedJobRepository(IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor)
    {
        _scopeAccessor = scopeAccessor;
    }

    /// <inheritdoc/>
    public async Task<DistributedBackgroundJobModel?> GetByNameAsync(string jobName)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not get distributed jobs");
        }

        return await _scopeAccessor.AmbientScope.ExecuteWithContextAsync(async db =>
        {
            DistributedJobDto? dto = db.DistributedJob
                .FirstOrDefault(x => x.Name == jobName);

            return dto is null ? null : MapFromDto(dto);
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DistributedBackgroundJobModel>> GetAllAsync()
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not get distributed jobs");
        }

        return await _scopeAccessor.AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<DistributedJobDto> jobs = await db.DistributedJob
                .ToListAsync();

            return jobs.Select(MapFromDto);
        });
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(DistributedBackgroundJobModel distributedBackgroundJob)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return;
        }

        DistributedJobDto dto = MapToDto(distributedBackgroundJob);

        await _scopeAccessor.AmbientScope.ExecuteWithContextAsync<DistributedJobDto>(async db =>
        {
             db.DistributedJob
                .Update(dto);

             await db.SaveChangesAsync();
        });
    }

    /// <inheritdoc/>
    public async Task AddAsync(DistributedBackgroundJobModel distributedBackgroundJob)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not add distributed job");
        }

        DistributedJobDto dto = MapToDto(distributedBackgroundJob);

        await _scopeAccessor.AmbientScope.ExecuteWithContextAsync<DistributedJobDto>(async db =>
        {
            db.DistributedJob
                .Add(dto);

            await db.SaveChangesAsync();
        });
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(DistributedBackgroundJobModel distributedBackgroundJob)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not delete distributed job");
        }

        DistributedJobDto dto = MapToDto(distributedBackgroundJob);

        await _scopeAccessor.AmbientScope.ExecuteWithContextAsync<DistributedJobDto>(async db =>
        {
            int rowsAffected = await db.DistributedJob
                .Where(x => x.Id == dto.Id)
                .ExecuteDeleteAsync();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "Could not delete distributed job, it may have already been deleted");
            }
        });
    }

    /// <inheritdoc/>
    public async Task AddAsync(IEnumerable<DistributedBackgroundJobModel> jobs)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not add distributed jobs");
        }

        IEnumerable<DistributedJobDto> dtos = jobs.Select(MapToDto);

        await _scopeAccessor.AmbientScope.ExecuteWithContextAsync<DistributedJobDto>(async db =>
        {
            await db.DistributedJob
                .AddRangeAsync(dtos);
        });
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(IEnumerable<DistributedBackgroundJobModel> jobs)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not delete distributed jobs");
        }

        var jobIds = jobs.Select(x => x.Id).ToArray();
        if (jobIds.Length is 0)
        {
            return;
        }

        await _scopeAccessor.AmbientScope.ExecuteWithContextAsync<DistributedJobDto>(async db =>
        {
            await db.DistributedJob
                .Where(x => jobIds.Contains(x.Id))
                .ExecuteDeleteAsync();
        });
    }

    private DistributedJobDto MapToDto(DistributedBackgroundJobModel model) =>
        new()
        {
            Id = model.Id,
            Name = model.Name,
            Period = model.Period.Ticks,
            LastRun = model.LastRun,
            IsRunning = model.IsRunning,
            LastAttemptedRun = model.LastAttemptedRun,
        };

    private DistributedBackgroundJobModel MapFromDto(DistributedJobDto jobDto) =>
        new()
        {
            Id = jobDto.Id,
            Name = jobDto.Name,
            Period = TimeSpan.FromTicks(jobDto.Period),
            LastRun = jobDto.LastRun,
            IsRunning = jobDto.IsRunning,
            LastAttemptedRun = jobDto.LastAttemptedRun,
        };
}
