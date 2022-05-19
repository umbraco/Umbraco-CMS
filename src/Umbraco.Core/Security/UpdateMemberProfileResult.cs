namespace Umbraco.Cms.Core.Security;

public class UpdateMemberProfileResult
{
    private UpdateMemberProfileResult()
    {
    }

    public UpdateMemberProfileStatus Status { get; private set; }

    public string? ErrorMessage { get; private set; }

    public static UpdateMemberProfileResult Success() =>
        new UpdateMemberProfileResult { Status = UpdateMemberProfileStatus.Success };

    public static UpdateMemberProfileResult Error(string message) =>
        new UpdateMemberProfileResult { Status = UpdateMemberProfileStatus.Error, ErrorMessage = message };
}
