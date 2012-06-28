using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees; // TreeDefinitionCollection

namespace umbraco.cms.presentation.Trees.RelationTypes
{
    /// <summary>
    /// This class will dynamically register a tree without having to populate the umbracoAppTree table (RegisterRelationTypeTree.sql)
    /// http://www.shazwazza.com/post/Dynamically-registering-custom-trees-without-writing-to-UmbracoAppTree.aspx
    /// </summary>
    public class AddRelationTypeTree : ApplicationStartupHandler
    {
        /// <summary>
        /// Initializes a new instance of the AddRelationTypeTree class. 
        /// </summary>
        public AddRelationTypeTree()
        {
            Application developerSection = Application.getByAlias("developer");

            ApplicationTree relationTypesApplicationTree = new ApplicationTree(false, true, 100, "developer", "relationTypesTree", "Relation Types", ".sprTreeFolder", ".sprTreeFolder_0", "umbraco", "cms.presentation.Trees.RelationTypes.RelationTypeTree", null);

            TreeDefinition relationTypesTreeDefinition = new TreeDefinition(typeof(umbraco.cms.presentation.Trees.RelationTypes.RelationTypeTree), relationTypesApplicationTree, developerSection);

            TreeDefinitionCollection.Instance.Add(relationTypesTreeDefinition);
        }
    }
}