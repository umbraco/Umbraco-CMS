using System.Linq;
using System.Text;

namespace Umbraco.Core.IO
{
	/// <summary>
	/// A custom file system provider for media
	/// </summary>
	[FileSystemProvider("media")]
	internal class MediaFileSystem : FileSystemWrapper
	{
		public MediaFileSystem(IFileSystem wrapped)
			: base(wrapped)
		{
		}
	}
}
