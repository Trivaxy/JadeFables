namespace JadeFables.Core
{
	public interface IOrderedLoadable
	{
		void Load();
		void Unload();
		float Priority { get; }
	}
}
