using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Web.Website.Models;

public abstract class MemberModelBuilderBase
{
    private readonly IShortStringHelper _shortStringHelper;

    public MemberModelBuilderBase(IMemberTypeService memberTypeService, IShortStringHelper shortStringHelper)
    {
        MemberTypeService = memberTypeService;
        _shortStringHelper = shortStringHelper;
    }

    public IMemberTypeService MemberTypeService { get; }

    protected List<MemberPropertyModel> GetMemberPropertiesViewModel(IMemberType memberType, IMember? member = null)
    {
        var viewProperties = new List<MemberPropertyModel>();

        var builtIns = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper).Select(x => x.Key).ToArray();

        IOrderedEnumerable<IPropertyType> propertyTypes = memberType.PropertyTypes
            .Where(x => builtIns.Contains(x.Alias) == false && memberType.MemberCanEditProperty(x.Alias))
            .OrderBy(p => p.SortOrder);

        foreach (IPropertyType prop in propertyTypes)
        {
            var value = string.Empty;
            if (member != null)
            {
                IProperty? propValue = member.Properties[prop.Alias];
                if (propValue != null && propValue.GetValue() != null)
                {
                    value = propValue.GetValue()?.ToString();
                }
            }

            var viewProperty = new MemberPropertyModel { Alias = prop.Alias, Name = prop.Name, Value = value };

            // TODO: Perhaps one day we'll ship with our own EditorTempates but for now developers
            // can just render their own.

            ////This is a rudimentary check to see what data template we should render
            //// if developers want to change the template they can do so dynamically in their views or controllers
            //// for a given property.
            ////These are the default built-in MVC template types: “Boolean”, “Decimal”, “EmailAddress”, “HiddenInput”, “HTML”, “Object”, “String”, “Text”, and “Url”
            //// by default we'll render a text box since we've defined that metadata on the UmbracoProperty.Value property directly.
            // if (prop.DataTypeId == new Guid(Constants.PropertyEditors.TrueFalse))
            // {
            //    viewProperty.EditorTemplate = "UmbracoBoolean";
            // }
            // else
            // {
            //    switch (prop.DataTypeDatabaseType)
            //    {
            //        case DataTypeDatabaseType.Integer:
            //            viewProperty.EditorTemplate = "Decimal";
            //            break;
            //        case DataTypeDatabaseType.Ntext:
            //            viewProperty.EditorTemplate = "Text";
            //            break;
            //        case DataTypeDatabaseType.Date:
            //        case DataTypeDatabaseType.Nvarchar:
            //            break;
            //    }
            // }
            viewProperties.Add(viewProperty);
        }

        return viewProperties;
    }
}
