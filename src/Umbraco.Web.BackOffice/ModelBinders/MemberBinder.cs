using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Web.BackOffice.ModelBinders;

/// <summary>
///     The model binder for <see cref="T:Umbraco.Web.Models.ContentEditing.MemberSave" />
/// </summary>
internal class MemberBinder : IModelBinder
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly ContentModelBinderHelper _modelBinderHelper;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;

    public MemberBinder(
        IJsonSerializer jsonSerializer,
        IHostingEnvironment hostingEnvironment,
        IShortStringHelper shortStringHelper,
        IUmbracoMapper umbracoMapper,
        IMemberService memberService,
        IMemberTypeService memberTypeService)
    {
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
        _modelBinderHelper = new ContentModelBinderHelper();
    }

    /// <summary>
    ///     Creates the model from the request and binds it to the context
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        MemberSave? model =
            await _modelBinderHelper.BindModelFromMultipartRequestAsync<MemberSave>(_jsonSerializer, _hostingEnvironment, bindingContext);
        if (model == null)
        {
            return;
        }

        model.PersistedContent =
            ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

        //create the dto from the persisted model
        if (model.PersistedContent != null)
        {
            model.PropertyCollectionDto =
                _umbracoMapper.Map<IMember, ContentPropertyCollectionDto>(model.PersistedContent);
            //now map all of the saved values to the dto
            _modelBinderHelper.MapPropertyValuesFromSaved(model, model.PropertyCollectionDto);
        }

        model.Name = model.Name?.Trim();

        bindingContext.Result = ModelBindingResult.Success(model);
    }

    /// <summary>
    ///     Returns an IMember instance used to bind values to and save (depending on the membership scenario)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private IMember GetExisting(MemberSave model) => GetExisting(model.Key);

    private IMember GetExisting(Guid key)
    {
        IMember? member = _memberService.GetByKey(key);
        if (member == null)
        {
            throw new InvalidOperationException("Could not find member with key " + key);
        }

        return member;
    }

    /// <summary>
    ///     Gets an instance of IMember used when creating a member
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <remarks>
    ///     Depending on whether a custom membership provider is configured this will return different results.
    /// </remarks>
    private IMember CreateNew(MemberSave model)
    {
        IMemberType? contentType = _memberTypeService.Get(model.ContentTypeAlias)
            ?? throw new InvalidOperationException("No member type found with alias " + model.ContentTypeAlias);

        // return the new member with the details filled in
        return new Member(model.Name, model.Email, model.Username, model.Password?.NewPassword, contentType);
    }
}
