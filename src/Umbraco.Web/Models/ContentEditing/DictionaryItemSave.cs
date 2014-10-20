using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
	[DataContract(Name = "dictionaryItem", Namespace = "")]
	public class DictionaryItemSave : EntityBasic
	{
		[DataMember(Name = "action", IsRequired = true)]
		[Required]
		public ContentSaveAction Action { get; set; }

		[DataMember(Name = "parentId")]
		public Guid ParentId { get; set; }

		[DataMember(Name = "itemKey")]
		public string ItemKey { get; set; }

		[DataMember(Name = "translations")]
		public IEnumerable<DictionaryTranslationDisplay> Translations { get; set; }

		[JsonIgnore]
		internal IDictionaryItem PersistedDictionaryItem { get; set; }
	}
}