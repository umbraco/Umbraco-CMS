using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
	[DataContract(Name = "dictionaryItem", Namespace = "")]
	public class DictionaryItemDisplay : EntityBasic, INotificationModel
	{
		public DictionaryItemDisplay()
		{
			Notifications = new List<Notification>();
		}

		[DataMember(Name = "parentGuid")]
		public Guid ParentGuid { get; set; }

		[DataMember(Name = "translations")]
		public IEnumerable<DictionaryTranslationDisplay> Translations { get; set; }

		[DataMember(Name = "notifications")]
		public List<Notification> Notifications { get; private set; }
	}
}