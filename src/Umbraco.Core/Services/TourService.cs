using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;


/**
 * TODO: This implementation is not the greatest,
 * ideally we should store tour information in its own table
 * making it its own feature, instead of an ad-hoc "add-on" to users.
 * additionally we should probably not store it as a JSON blob, but instead as a proper table.
 * For now we'll keep doing the deserialize/serialize dance here,
 * because there is no reason to spend cycles to deserialize/serialize the tour data every time we fetch/save a user.
 */
public class TourService : ITourService
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IUserService _userService;

    public TourService(
        IJsonSerializer jsonSerializer,
        IUserService userService)
    {
        _jsonSerializer = jsonSerializer;
        _userService = userService;
    }

    /// <inheritdoc />
    public async Task<TourOperationStatus> SetAsync(UserTourStatus status, Guid userKey)
    {
        IUser? user = await _userService.GetAsync(userKey);

        if (user is null)
        {
            return TourOperationStatus.UserNotFound;
        }

        // If the user currently have no tour data, we can just add the data and save it.
        if (string.IsNullOrWhiteSpace(user.TourData))
        {
            List<UserTourStatus> tours = new() { status };
            user.TourData = _jsonSerializer.Serialize(tours);
            _userService.Save(user);

            return TourOperationStatus.Success;
        }

        // Otherwise we have to check it it already exists, and if so, replace it.
        List<UserTourStatus> existingTours =
            _jsonSerializer.Deserialize<IEnumerable<UserTourStatus>>(user.TourData)?.ToList() ?? new List<UserTourStatus>();
        UserTourStatus? found = existingTours.FirstOrDefault(x => x.Alias == status.Alias);

        if (found is not null)
        {
            existingTours.Remove(found);
        }

        existingTours.Add(status);

        user.TourData = _jsonSerializer.Serialize(existingTours);
        _userService.Save(user);
        return TourOperationStatus.Success;
    }

    /// <inheritdoc />
    public async Task<Attempt<IEnumerable<UserTourStatus>, TourOperationStatus>> GetAllAsync(Guid userKey)
    {
        IUser? user = await _userService.GetAsync(userKey);

        if (user is null)
        {
            return Attempt.FailWithStatus(TourOperationStatus.UserNotFound, Enumerable.Empty<UserTourStatus>());
        }

        // No tour data, we'll just return empty.
        if (string.IsNullOrWhiteSpace(user.TourData))
        {
            return Attempt.SucceedWithStatus(TourOperationStatus.Success, Enumerable.Empty<UserTourStatus>());
        }

        IEnumerable<UserTourStatus> tours = _jsonSerializer.Deserialize<IEnumerable<UserTourStatus>>(user.TourData)
                                            ?? Enumerable.Empty<UserTourStatus>();

        return Attempt.SucceedWithStatus(TourOperationStatus.Success, tours);
    }
}
