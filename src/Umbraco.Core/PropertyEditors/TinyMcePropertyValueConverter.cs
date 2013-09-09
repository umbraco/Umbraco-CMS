using System;
using System.Web;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
	/// <summary>
	/// Value converter for the RTE so that it always returns IHtmlString so that Html.Raw doesn't have to be used.
	/// </summary>
	internal class TinyMcePropertyValueConverter : PropertyValueConverter
	{
	    public override string AssociatedPropertyEditorAlias
	    {
            get { return Constants.PropertyEditors.TinyMCEv3Alias; }
	    }

        public override Attempt<object> ConvertSourceToObject(object valueToConvert, PublishedPropertyDefinition propertyDefinition, bool isPreviewing)
	    {
            return new Attempt<object>(true, new HtmlString(valueToConvert.ToString()));
	    }
	}
}