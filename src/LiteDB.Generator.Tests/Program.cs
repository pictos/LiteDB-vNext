namespace LiteDB;

public class Program
{
	public static void Main(string[] args)
	{

	}
}

unsafe interface IBla
{
	bool ReadPage(byte* page, uint positionID);

	byte* Beta { get; set; }
}



[AutoInterface(typeof(IDisposable))]
public unsafe class DiskService : IDiskService
{
	public string? Name { get; set; }

	public byte* Beta { get; set; }

	public DiskService()
	{
	}

	public virtual void Dispose()
	{
		throw new NotImplementedException();
	}

	public void ReadPage(byte* page, uint positionID)
	{ 
	}

	public virtual bool Open(Span<byte> buffer, Stream stream, int length) { return false; }

	//public Stream RendStreamReader()
	//{
	//	throw new NotImplementedException();
	//}

	public void ReturnReader(Stream stream)
	{
	}
}

[AutoInterface]
public class StreamPool : IStreamPool
{
	public StreamPool(int limit)
	{
	}

	public Stream RendStreamReader()
	{
		return new MemoryStream();
	}

	public void ReturnReader(Stream stream)
	{
	}

}