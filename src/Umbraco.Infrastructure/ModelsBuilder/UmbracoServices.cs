using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

public sealed class UmbracoServices
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoServices" /> class.
    /// </summary>
    public UmbracoServices(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        IShortStringHelper shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
        _publishedContentTypeFactory = publishedContentTypeFactory;
        _shortStringHelper = shortStringHelper;
    }

    public static string GetClrName(IShortStringHelper shortStringHelper, string? name, string alias) =>

        // ModelsBuilder's legacy - but not ideal
        alias.ToCleanString(shortStringHelper, CleanStringType.ConvertCase | CleanStringType.PascalCase);

    #region Services

    public IList<TypeModel> GetAllTypes()
    {
        var types = new List<TypeModel>();

        // TODO: this will require 3 rather large SQL queries on startup in ModelsMode.InMemoryAuto mode. I know that these will be cached after lookup but it will slow
        // down startup time ... BUT these queries are also used in NuCache on startup so we can't really avoid them. Maybe one day we can
        // load all of these in in one query and still have them cached per service, and/or somehow improve the perf of these since they are used on startup
        // in more than one place.
        types.AddRange(GetTypes(
            PublishedItemType.Content,
            _contentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray()));
        types.AddRange(GetTypes(
            PublishedItemType.Media,
            _mediaTypeService.GetAll().Cast<IContentTypeComposition>().ToArray()));
        types.AddRange(GetTypes(
            PublishedItemType.Member,
            _memberTypeService.GetAll().Cast<IContentTypeComposition>().ToArray()));

        return EnsureDistinctAliases(types);
    }

    public IList<TypeModel> GetContentTypes()
    {
        IContentTypeComposition[] contentTypes = _contentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
        return GetTypes(PublishedItemType.Content, contentTypes); // aliases have to be unique here
    }

    public IList<TypeModel> GetMediaTypes()
    {
        IContentTypeComposition[] contentTypes = _mediaTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
        return GetTypes(PublishedItemType.Media, contentTypes); // aliases have to be unique here
    }

    public IList<TypeModel> GetMemberTypes()
    {
        IContentTypeComposition[] memberTypes = _memberTypeService.GetAll().Cast<IContentTypeComposition>().ToArray();
        return GetTypes(PublishedItemType.Member, memberTypes); // aliases have to be unique here
    }

    internal static IList<TypeModel> EnsureDistinctAliases(IList<TypeModel> typeModels)
    {
        IEnumerable<IGrouping<string, TypeModel>> groups = typeModels.GroupBy(x => x.Alias.ToLowerInvariant());
        foreach (IGrouping<string, TypeModel> group in groups.Where(x => x.Count() > 1))
        {
            throw new NotSupportedException($"Alias \"{group.Key}\" is used by types"
                                            + $" {string.Join(", ", group.Select(x => x.ItemType + ":\"" + x.Alias + "\""))}. Aliases have to be unique."
                                            + " One of the aliases must be modified in order to use the ModelsBuilder.");
        }

        return typeModels;
    }

    private IList<TypeModel> GetTypes(PublishedItemType itemType, IContentTypeComposition[] contentTypes)
    {
        var typeModels = new List<TypeModel>();
        var uniqueTypes = new HashSet<string>();

        // get the types and the properties
        foreach (IContentTypeComposition contentType in contentTypes)
        {
            var typeModel = new TypeModel
            {
                Id = contentType.Id,
                Alias = contentType.Alias,
                ClrName = GetClrName(_shortStringHelper, contentType.Name, contentType.Alias),
                ParentId = contentType.ParentId,
                Name = contentType.Name,
                Description = contentType.Description,
            };

            // of course this should never happen, but when it happens, better detect it
            // else we end up with weird nullrefs everywhere
            if (uniqueTypes.Contains(typeModel.ClrName))
            {
                throw new PanicException($"Panic: duplicate type ClrName \"{typeModel.ClrName}\".");
            }

            uniqueTypes.Add(typeModel.ClrName);

            IPublishedContentType publishedContentType = _publishedContentTypeFactory.CreateContentType(contentType);
            switch (itemType)
            {
                case PublishedItemType.Content:
                    typeModel.ItemType = publishedContentType.ItemType == PublishedItemType.Element
                        ? TypeModel.ItemTypes.Element
                        : TypeModel.ItemTypes.Content;
                    break;
                case PublishedItemType.Media:
                    typeModel.ItemType = publishedContentType.ItemType == PublishedItemType.Element
                        ? TypeModel.ItemTypes.Element
                        : TypeModel.ItemTypes.Media;
                    break;
                case PublishedItemType.Member:
                    typeModel.ItemType = publishedContentType.ItemType == PublishedItemType.Element
                        ? TypeModel.ItemTypes.Element
                        : TypeModel.ItemTypes.Member;
                    break;
                default:
                    throw new InvalidOperationException(string.Format(
                        "Unsupported PublishedItemType \"{0}\".",
                        itemType));
            }

            typeModels.Add(typeModel);

            foreach (IPropertyType propertyType in contentType.PropertyTypes)
            {
                var propertyModel = new PropertyModel
                {
                    Alias = propertyType.Alias,
                    ClrName = GetClrName(_shortStringHelper, propertyType.Name, propertyType.Alias),
                    Name = propertyType.Name,
                    Description = propertyType.Description,
                };

                IPublishedPropertyType? publishedPropertyType =
                    publishedContentType.GetPropertyType(propertyType.Alias);
                if (publishedPropertyType == null)
                {
                    throw new PanicException(
                        $"Panic: could not get published property type {contentType.Alias}.{propertyType.Alias}.");
                }

                propertyModel.ModelClrType = publishedPropertyType.ModelClrType;

                typeModel.Properties.Add(propertyModel);
            }
        }

        // wire the base types
        foreach (TypeModel typeModel in typeModels.Where(x => x.ParentId > 0))
        {
            typeModel.BaseType = typeModels.SingleOrDefault(x => x.Id == typeModel.ParentId);

            // Umbraco 7.4 introduces content types containers, so even though ParentId > 0, the parent might
            // not be a content type - here we assume that BaseType being null while ParentId > 0 means that
            // the parent is a container (and we don't check).
            typeModel.IsParent = typeModel.BaseType != null;
        }

        // discover mixins
        foreach (IContentTypeComposition contentType in contentTypes)
        {
            TypeModel? typeModel = typeModels.SingleOrDefault(x => x.Id == contentType.Id);
            if (typeModel == null)
            {
                throw new PanicException("Panic: no type model matching content type.");
            }

            IEnumerable<IContentTypeComposition> compositionTypes;
            if (contentType is IMediaType contentTypeAsMedia)
            {
                compositionTypes = contentTypeAsMedia.ContentTypeComposition;
            }
            else if (contentType is IContentType contentTypeAsContent)
            {
                compositionTypes = contentTypeAsContent.ContentTypeComposition;
            }
            else if (contentType is IMemberType contentTypeAsMember)
            {
                compositionTypes = contentTypeAsMember.ContentTypeComposition;
            }
            else
            {
                throw new PanicException(string.Format(
                    "Panic: unsupported type \"{0}\".",
                    contentType.GetType().FullName));
            }

            foreach (IContentTypeComposition compositionType in compositionTypes)
            {
                TypeModel? compositionModel = typeModels.SingleOrDefault(x => x.Id == compositionType.Id);
                if (compositionModel == null)
                {
                    throw new PanicException("Panic: composition type does not exist.");
                }

                if (compositionType.Id == contentType.ParentId)
                {
                    continue;
                }

                // add to mixins
                typeModel.MixinTypes.Add(compositionModel);

                // mark as mixin - as well as parents
                compositionModel.IsMixin = true;
                while ((compositionModel = compositionModel.BaseType) != null)
                {
                    compositionModel.IsMixin = true;
                }
            }
        }

        return typeModels;
    }

    #endregion
}
