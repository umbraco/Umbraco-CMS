using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        [SetUp]
        public virtual void Initialize()
        {
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;
            PluginManager.Current = new PluginManager(false);
            MappingResolver.Current = new MappingResolver(
                () => PluginManager.Current.ResolveAssignedMapperTypes());

            //MappingResolver.Current = new MappingResolver(
            //    new[]
            //        {
            //            typeof(ContentMapper), 
            //            typeof(ContentTypeMapper),
            //            typeof(DataTypeDefinitionMapper),
            //            typeof(DictionaryMapper),
            //            typeof(DictionaryTranslationMapper),
            //            typeof(LanguageMapper),
            //            typeof(MediaMapper),
            //            typeof(MediaTypeMapper),
            //            typeof(PropertyGroupMapper),
            //            typeof(PropertyMapper),
            //            typeof(PropertyTypeMapper),
            //            typeof(RelationMapper),
            //            typeof(RelationTypeMapper),
            //            typeof(ServerRegistrationMapper),
            //            typeof(UserMapper),
            //            typeof(UserTypeMapper)
            //        });
            Resolution.Freeze();
            SetUp();
        }

        public virtual void SetUp()
        {}

        [TearDown]
        public virtual void TearDown()
        {
            MappingResolver.Reset();
            SqlSyntaxContext.SqlSyntaxProvider = null;
            PluginManager.Current = null;
        }
    }
}