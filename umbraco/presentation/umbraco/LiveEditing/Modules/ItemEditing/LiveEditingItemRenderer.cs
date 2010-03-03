using System.Web.UI;
using umbraco.presentation.LiveEditing.Updates;
using umbraco.presentation.templateControls;
using umbraco.presentation.nodeFactory;

namespace umbraco.presentation.LiveEditing.Modules.ItemEditing
{
    /// <summary>
    /// ItemRenderer that surrounds regular output with a marker tag to enable Live Editing.
    /// </summary>
    public class LiveEditingItemRenderer : ItemRenderer
    {
        /// <summary>Returns the instance of <see cref="LiveEditingItemRenderer"/>.</summary>
        public new readonly static LiveEditingItemRenderer Instance = new LiveEditingItemRenderer();

        /// <summary>Name of the marker tag.</summary>
        protected const string LiveEditingMarkerTag = "umbraco:iteminfo";

        /// <summary>Name of the marker tag's ID attribute.</summary>
        protected const string LiveEditingMarkerIdAttribute = "itemId";

        /// <summary>Name of the marker tag's field name attribute.</summary>
        protected const string LiveEditingFieldNameAttribute = "name";

        /// <summary>Name of the marker tag's node ID attribute.</summary>
        protected const string LiveEditingNodeIdAttribute = "nodeId";

        /// <summary>Format string of the tooltip.</summary>
        protected const string TooltipTextFormat = "Click to edit the {0} field of '{1}'";

        /// <summary>Default text to display if the item's contents are empty.</summary>
        protected const string DefaultEmptyTextFormat = "<strong>Click here to edit {0}.</strong>";

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveEditingItemRenderer"/> class.
        /// </summary>
        protected LiveEditingItemRenderer()
        {
            // Singleton pattern.
        }

        /// <summary>
        /// Inits the specified item. To be called from the OnInit method of Item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void Init(Item item)
        {
            // Postpone the ParseMacros call (base.Init calls ParseMacros here),
            // because we want the updated values to appear in the item,
            // and they are only available after the Load event. 
            // This breaks macro postback support, but we don't need that in Live Editing mode.
        }

        /// <summary>
        /// Forces macros to be parsed.
        /// This can be useful if controls, that are created as a result of the parsing,
        /// need to be available.
        /// </summary>
        /// <remarks>This can cause problems however, when called too early. (see comments inside Init)</remarks>
        /// <param name="item">The item.</param>
        public void ForceParseMacros(Item item)
        {
            ParseMacros(item);
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object,
        /// which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">
        ///     The <see cref="T:System.Web.UI.HtmlTextWriter"/>
        ///     object that receives the server control content.
        /// </param>
        public override void Render(Item item, HtmlTextWriter writer)
        {
            // call ParseMacros now, as we postponed that in Init
            ParseMacros(item);

            bool useLiveEditing = item.CanUseLiveEditing;

            // render the marker field opening tag
            if (useLiveEditing)
            {
                // add needed attributes for Live Editing
                writer.AddAttribute(LiveEditingMarkerIdAttribute, item.ItemId.ToString());
                writer.AddAttribute(LiveEditingFieldNameAttribute, item.Field);
                writer.AddAttribute(LiveEditingNodeIdAttribute, item.GetParsedNodeId().ToString());

                // add descriptive tooltip
                try
                {
                    writer.AddAttribute("title", string.Format(TooltipTextFormat, item.Field,
                                                               new Node(item.GetParsedNodeId().Value).Name));
                }
                catch{}

         
                writer.RenderBeginTag(LiveEditingMarkerTag);
            }

            try
            {
                // render the item as we used to
                base.Render(item, writer);
            }

            finally
            {
                // render the marker field closing tag
                if (useLiveEditing)
                {
                    writer.RenderEndTag();
                }
            }
        }

        /// <summary>
        /// Renders the field contents.
        /// Checks via the NodeId attribute whether to fetch data from another page than the current one.
        /// </summary>
        /// <returns>
        /// A string of field contents (macros not parsed)
        /// </returns>
        protected override string GetFieldContents(Item item)
        {
            // check if an update has been made to the field, and use the updated value istead
            IUpdateList updates = UmbracoContext.Current.LiveEditingContext.Updates;
            ItemUpdate latestUpdate = updates.GetLatest<ItemUpdate>(u => u.NodeId == item.GetParsedNodeId()
                                                                      && u.Field == item.Field);
            if (latestUpdate != null)
                return latestUpdate.Data as string; //can't use ToString() as a null value will throw an exception
            else
                return base.GetFieldContents(item);
        }

        /// <summary>
        /// Gets the text to display if the field contents are empty.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The text to display.</returns>
        protected override string GetEmptyText(Item item)
        {
            string baseEmptyText = base.GetEmptyText(item) ?? string.Empty;

            // always make sure that there is some text, so that empty fields can be edited
            return baseEmptyText.Trim().Length > 0 ? baseEmptyText : string.Format(DefaultEmptyTextFormat, item.Field);
        }
    }
}
