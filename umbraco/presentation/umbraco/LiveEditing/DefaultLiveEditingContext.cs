using System;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.presentation.LiveEditing.Menu;
using umbraco.presentation.LiveEditing.Updates;
using System.Web.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace umbraco.presentation.LiveEditing
{
    /// <summary>
    /// Class that encapsulates all Live Editing information for an Umbraco context.
    /// </summary>
    public class DefaultLiveEditingContext : ILiveEditingContext
    {
        /// <summary>The current Live Editing menu.</summary>
        private ILiveEditingMenu m_Menu;

        /// <summary>Current object that maintains the update list accross different requests.</summary>
        private UpdateListSaver m_fieldUpdateSaver = new UpdateListSaver();

        /// <summary>
        /// Creates a default Live Editing context.
        /// </summary>
        public DefaultLiveEditingContext()
        {
            if (Enabled)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected virtual void Initialize()
        {
            if (m_Menu == null)
            {
                m_Menu = new DefaultLiveEditingMenu();
                m_Menu.Add(m_fieldUpdateSaver);
            }
        }

        #region ILiveEditingContext Members

        /// <summary>
        /// Returns whether Live Editing is enabled.
        /// </summary>
        /// <value><c>true</c> if Live Editing enabled;
        public bool Enabled
        {
            get
            {
                if (HttpContext.Current.Session == null || UmbracoEnsuredPage.CurrentUser == null)
                    return false;

                object umbEditMode = HttpContext.Current.Session["UmbracoLiveEditingMode"];
                return (umbEditMode != null && (bool)umbEditMode);
            }
            set
            {
                if (value && UmbracoEnsuredPage.CurrentUser == null)
                    throw new ApplicationException("Cannot switch to Live Editmode when user is not logged in.");

                HttpContext.Current.Session["UmbracoLiveEditingMode"] = value;
                if (value)
                {
                    Initialize();
                }
            }
        }

        /// <summary>
        /// Gets the Live Editing menu, or <c>null</c> if Live Editing is disabled.
        /// </summary>
        /// <value>
        /// The Live Editing menu, or <c>null</c> if Live Editing is disabled.
        /// </value>
        public ILiveEditingMenu Menu
        {
            get { return m_Menu; }
        }

        /// <summary>
        /// Gets the field updates made during this context.
        /// </summary>
        /// <value>The updates.</value>
        public IUpdateList Updates
        {
            get
            {
                IUpdateList value = m_fieldUpdateSaver.Updates;
                if (value == null)
                    m_fieldUpdateSaver.Updates = value = new DefaultUpdateList();
                return value;
            }
        }

        #endregion

        /// <summary>
        /// Dummy control used to save the Updates list into a hidden field.
        /// It cannot be stored inside the HTTP Context, because this context changes on every request
        /// and we need to have access to this information on every page postback.
        /// </summary>
        protected class UpdateListSaver : PlaceHolder
        {
            /// <summary>The fieldname used to save the updates in.</summary>
            protected const string UpdatesFieldName = "__LiveEditing_Updates";

            /// <summary>Internal list of updates.</summary>
            private IUpdateList m_Updates;

            /// <summary>Updates serializer.</summary>
            private static BinaryFormatter serializer = new BinaryFormatter();

            /// <summary>
            /// Gets or sets the updates.
            /// </summary>
            /// <value>The updates.</value>
            public IUpdateList Updates
            {
                get
                {
                    if (m_Updates == null)
                    {
                        // get the updates out of the hidden field
                        string value = HttpContext.Current.Request.Params[UpdatesFieldName];
                        if (!String.IsNullOrEmpty(value))
                        {
                            // decompress and deserialize the field value
                            byte[] deflated = Convert.FromBase64CharArray(value.ToCharArray(), 0, value.Length);
                            Stream inflaterStream = new InflaterInputStream(new MemoryStream(deflated),
                                                                            new Inflater(true));
                            m_Updates = serializer.Deserialize(inflaterStream) as IUpdateList;
                        }
                    }
                    return m_Updates;
                }
                set
                {
                    m_Updates = value;
                }
            }

            /// <summary>
            /// Saves the updates into a hidden field.
            /// </summary>
            /// <param name="writer">Ignored.</param>
            protected override void Render(HtmlTextWriter writer)
            {
                // serialize object into compressed stream
                MemoryStream deflatedStream = new MemoryStream();
                DeflaterOutputStream deflater = new DeflaterOutputStream(deflatedStream,
                                                                         new Deflater(Deflater.BEST_COMPRESSION, true));
                serializer.Serialize(deflater, m_Updates);
                deflater.Close();

                // get compressed characters
                byte[] deflatedBytes = deflatedStream.ToArray();
                char[] deflatedChars = new char[(int)(Math.Ceiling((double)deflatedBytes.Length / 3) * 4)];
                Convert.ToBase64CharArray(deflatedBytes, 0, deflatedBytes.Length, deflatedChars, 0);

                // register the compressed string as a hidden field
                ScriptManager.RegisterHiddenField(Page, UpdatesFieldName, new string(deflatedChars));
            }
        }
    }
}
