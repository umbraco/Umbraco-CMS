using Lucene.Net.Store;

public class RandomIdRAMDirectory : RAMDirectory
{
    private readonly string _lockId = Guid.NewGuid().ToString();

    public override string GetLockID() => _lockId;
}
