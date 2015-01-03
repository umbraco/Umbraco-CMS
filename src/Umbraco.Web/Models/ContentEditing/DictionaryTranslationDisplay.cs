using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
	[DataContract(Name = "translation", Namespace = "")]
	public class DictionaryTranslationDisplay
	{
		[DataMember(Name = "language", IsRequired = true)]
		public string Language { get; set; }

		[DataMember(Name = "value", IsRequired = true)]
		public object Value { get; set; }
	}
}