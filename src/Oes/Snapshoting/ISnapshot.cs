namespace Oes.Snapshoting;

public interface ISnapshot
{
    public UInt16 Version  { get; }
    public UInt64 Revision { get; }
    public String Content  { get; }

    public T As<T>();
    public void Modify<T>(Action<T> fn);
}

public class Snapshot : ISnapshot
{
    public UInt16 Version  { get; }
    public UInt64 Revision { get; }
    public String Content  { get; }

    public Snapshot(UInt16 version, UInt64 revision, String content)
    {
        Version  = version;
        Revision = revision;
        Content  = content;
    }

    public T As<T>()
    {
        throw new NotImplementedException();
    }

    public void Modify<T>(Action<T> fn) => fn(As<T>());
}