using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Web.Website.Models;

/// <summary>
///     Builds a <see cref="RegisterModel" /> for use on the front-end
/// </summary>
public class RegisterModelBuilder : MemberModelBuilderBase
{
    private bool _lookupProperties;
    private string? _memberTypeAlias;
    private string? _redirectUrl;
    private bool _usernameIsEmail;
    private bool _automaticLogIn = true;

    public RegisterModelBuilder(IMemberTypeService memberTypeService, IShortStringHelper shortStringHelper)
        : base(memberTypeService, shortStringHelper)
    {
    }

    public RegisterModelBuilder WithRedirectUrl(string? redirectUrl)
    {
        _redirectUrl = redirectUrl;
        return this;
    }

    public RegisterModelBuilder UsernameIsEmail(bool usernameIsEmail = true)
    {
        _usernameIsEmail = usernameIsEmail;
        return this;
    }

    public RegisterModelBuilder WithMemberTypeAlias(string memberTypeAlias)
    {
        _memberTypeAlias = memberTypeAlias;
        return this;
    }

    public RegisterModelBuilder WithCustomProperties(bool lookupProperties)
    {
        _lookupProperties = lookupProperties;
        return this;
    }

    public RegisterModelBuilder WithAutomaticLogIn(bool automaticLogIn = true)
    {
        _automaticLogIn = automaticLogIn;
        return this;
    }

    public RegisterModel Build()
    {
        var providedOrDefaultMemberTypeAlias = _memberTypeAlias ?? Constants.Conventions.MemberTypes.DefaultAlias;
        IMemberType? memberType = MemberTypeService.Get(providedOrDefaultMemberTypeAlias);
        if (memberType == null)
        {
            throw new InvalidOperationException(
                $"Could not find a member type with alias: {providedOrDefaultMemberTypeAlias}.");
        }

        var model = new RegisterModel
        {
            RedirectUrl = _redirectUrl,
            MemberTypeAlias = providedOrDefaultMemberTypeAlias,
            UsernameIsEmail = _usernameIsEmail,
            MemberProperties = _lookupProperties
                ? GetMemberPropertiesViewModel(memberType)
                : Enumerable.Empty<MemberPropertyModel>().ToList(),
            AutomaticLogIn = _automaticLogIn
        };
        return model;
    }
}
