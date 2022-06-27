namespace Oes.Snapshoting;

public interface ISnapshotStorage
{
    public Task<ISnapshot?> ReadAsync(String id);
    public Task             WriteAsync(String id, ISnapshot snapshot);
}
