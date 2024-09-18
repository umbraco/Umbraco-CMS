using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

/// <summary>
///     Provides a base class for all builders.
/// </summary>
public class BuilderBase : IBuilderBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BuilderBase" /> class with a list of models to generate,
    ///     the result of code parsing, and a models namespace.
    /// </summary>
    /// <param name="umbracoService"></param>
    public BuilderBase(UmbracoServices umbracoService)
    {
        Config = new ModelsBuilderSettings();

        // can be null or empty, we'll manage
        ModelsNamespace = Config.ModelsNamespace;
        IList<TypeModel> allTypes = umbracoService.GetAllTypes();
        Prepare(allTypes);
    }

    /// <summary>
    ///     For testing purposes only.
    ///     Parameterless constructor.
    ///     You WILL have to call Prepare(types) to set the models to generate.
    /// </summary>
    public BuilderBase()
    {
        Config = new ModelsBuilderSettings();

        // can be null or empty, we'll manage
        ModelsNamespace = Config.ModelsNamespace;
    }

    /// <summary>
    ///     Gets or sets a value indicating the namespace to use for the models.
    /// </summary>
    /// <remarks>May be overriden by code attributes.</remarks>
    public string ModelsNamespace { get; set; }

    /// <inheritdoc/>
    public IList<string> Using { get; set; } =
    [
        "System",
        "System.Linq.Expressions",
        "Umbraco.Cms.Core.Models.PublishedContent",
        "Umbraco.Cms.Core.PublishedCache",
        "Umbraco.Cms.Infrastructure.ModelsBuilder",
        "Umbraco.Cms.Core",
        "Umbraco.Extensions",
    ];

    /// <summary>
    ///     Gets or sets the list of all models.
    /// </summary>
    /// <remarks>Includes those that are ignored.</remarks>
    private IList<TypeModel>? TypeModels { get; set; }

    /// <summary>
    ///     For testing purposes only.
    /// </summary>
    public string? ModelsNamespaceForTests { get; set; }

    protected ModelsBuilderSettings Config { get; }


    /// <inheritdoc/>
    public IEnumerable<TypeModel> GetModelsToGenerate() => TypeModels ?? new List<TypeModel>();
    private void SetModelsToGenerate(IEnumerable<TypeModel>  types) => TypeModels = types.ToList();

    /// <inheritdoc/>
    public string GetModelsNamespace()
    {
        if (ModelsNamespaceForTests != null)
        {
            return ModelsNamespaceForTests;
        }

        // if builder was initialized with a namespace, use that one
        if (!string.IsNullOrWhiteSpace(ModelsNamespace))
        {
            return ModelsNamespace;
        }

        // use configured else fallback to default
        return string.IsNullOrWhiteSpace(Config.ModelsNamespace)
            ? Constants.ModelsBuilder.DefaultModelsNamespace
            : Config.ModelsNamespace;
    }

    /// <summary>
    ///     Prepares generation by processing the result of code parsing.
    /// </summary>
    public void Prepare(IEnumerable<TypeModel> types)
    {
        SetModelsToGenerate(types);
        TypeModel.MapModelTypes(GetModelsToGenerate().ToList(), ModelsNamespace);

        var isInMemoryMode = Config.ModelsMode == ModelsMode.InMemoryAuto;

        // for the first two of these two tests,
        //  always throw, even in InMemory mode: cannot happen unless ppl start fidling with attributes to rename
        //  things, and then they should pay attention to the generation error log - there's no magic here
        // for the last one, don't throw in InMemory mode, see comment

        // ensure we have no duplicates type names
        foreach (IGrouping<string, TypeModel> xx in GetModelsToGenerate().GroupBy(x => x.ClrName).Where(x => x.Count() > 1))
        {
            throw new InvalidOperationException($"Type name \"{xx.Key}\" is used"
                                                + $" for types with alias {string.Join(", ", xx.Select(x => x.ItemType + ":\"" + x.Alias + "\""))}. Names have to be unique."
                                                + " Consider using an attribute to assign different names to conflicting types.");
        }

        // ensure we have no duplicates property names
        foreach (TypeModel typeModel in GetModelsToGenerate())
        {
            foreach (IGrouping<string, PropertyModel> xx in typeModel.Properties.GroupBy(x => x.ClrName)
                         .Where(x => x.Count() > 1))
            {
                throw new InvalidOperationException(
                    $"Property name \"{xx.Key}\" in type {typeModel.ItemType}:\"{typeModel.Alias}\""
                    + $" is used for properties with alias {string.Join(", ", xx.Select(x => "\"" + x.Alias + "\""))}. Names have to be unique."
                    + " Consider using an attribute to assign different names to conflicting properties.");
            }
        }

        // ensure content & property type don't have identical name (csharp hates it)
        foreach (TypeModel typeModel in GetModelsToGenerate())
        {
            foreach (PropertyModel xx in typeModel.Properties.Where(x => x.ClrName == typeModel.ClrName))
            {
                if (!isInMemoryMode)
                {
                    throw new InvalidOperationException(
                        $"The model class for content type with alias \"{typeModel.Alias}\" is named \"{xx.ClrName}\"."
                        + $" CSharp does not support using the same name for the property with alias \"{xx.Alias}\"."
                        + " Consider using an attribute to assign a different name to the property.");
                }

                // in InMemory mode we generate commented out properties with an error message,
                // instead of throwing, because then it kills the sites and ppl don't understand why
                xx.AddError($"The class {typeModel.ClrName} cannot implement this property, because"
                            + $" CSharp does not support naming the property with alias \"{xx.Alias}\" with the same name as content type with alias \"{typeModel.Alias}\"."
                            + " Consider using an attribute to assign a different name to the property.");

                // will not be implemented on interface nor class
                // note: we will still create the static getter, and implement the property on other classes...
            }
        }

        // ensure we have no collision between base types
        // NO: we may want to define a base class in a partial, on a model that has a parent
        // we are NOT checking that the defined base type does maintain the inheritance chain
        // foreach (var xx in _typeModels.Where(x => !x.IsContentIgnored).Where(x => x.BaseType != null && x.HasBase))
        //    throw new InvalidOperationException(string.Format("Type alias \"{0}\" has more than one parent class.",
        //        xx.Alias));

        // discover interfaces that need to be declared / implemented
        foreach (TypeModel typeModel in GetModelsToGenerate())
        {
            // collect all the (non-removed) types implemented at parent level
            // ie the parent content types and the mixins content types, recursively
            var parentImplems = new List<TypeModel>();
            if (typeModel.BaseType != null)
            {
                TypeModel.CollectImplems(parentImplems, typeModel.BaseType);
            }

            // interfaces we must declare we implement (initially empty)
            // ie this type's mixins, except those that have been removed,
            // and except those that are already declared at the parent level
            // in other words, DeclaringInterfaces is "local mixins"
            IEnumerable<TypeModel> declaring = typeModel.MixinTypes
                .Except(parentImplems);
            typeModel.DeclaringInterfaces.AddRange(declaring);

            // interfaces we must actually implement (initially empty)
            // if we declare we implement a mixin interface, we must actually implement
            // its properties, all recursively (ie if the mixin interface implements...)
            // so, starting with local mixins, we collect all the (non-removed) types above them
            var mixinImplems = new List<TypeModel>();
            foreach (TypeModel i in typeModel.DeclaringInterfaces)
            {
                TypeModel.CollectImplems(mixinImplems, i);
            }

            // and then we remove from that list anything that is already declared at the parent level
            typeModel.ImplementingInterfaces.AddRange(mixinImplems.Except(parentImplems));
        }

        // ensure elements don't inherit from non-elements
        foreach (TypeModel typeModel in GetModelsToGenerate().Where(x => x.IsElement))
        {
            if (typeModel.BaseType != null && !typeModel.BaseType.IsElement)
            {
                throw new InvalidOperationException(
                    $"Cannot generate model for type '{typeModel.Alias}' because it is an element type, but its parent type '{typeModel.BaseType.Alias}' is not.");
            }

            var errs = typeModel.MixinTypes.Where(x => !x.IsElement).ToList();
            if (errs.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot generate model for type '{typeModel.Alias}' because it is an element type, but it is composed of {string.Join(", ", errs.Select(x => "'" + x.Alias + "'"))} which {(errs.Count == 1 ? "is" : "are")} not.");
            }
        }
    }

    /// <inheritdoc/>
    public string GetModelsBaseClassName(TypeModel type) =>

        // default
        type.IsElement ? "PublishedElementModel" : "PublishedContentModel";
}
