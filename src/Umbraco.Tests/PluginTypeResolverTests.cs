using System.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
	public class PluginTypeResolverTests
	{

		[Test]
		public void Ensure_Only_One_Type_List_Created()
		{
			var foundTypes1 = PluginTypeResolver.Current.ResolveFindMeTypes();
			var foundTypes2 = PluginTypeResolver.Current.ResolveFindMeTypes();
			Assert.AreEqual(1, PluginTypeResolver.Current.GetTypeLists().Count);
		}

		[Test]
		public void Resolves_Types()
		{
			var foundTypes1 = PluginTypeResolver.Current.ResolveFindMeTypes();			
			Assert.AreEqual(2, foundTypes1.Count());
		}

		public interface IFindMe
		{

		}

		public class FindMe1 : IFindMe
		{

		}

		public class FindMe2 : IFindMe
		{

		}

	}
}