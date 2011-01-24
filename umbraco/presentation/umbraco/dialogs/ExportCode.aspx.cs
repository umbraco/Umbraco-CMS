using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.IO;

namespace umbraco.presentation.umbraco.dialogs
{
	public partial class ExportCode : BasePages.UmbracoEnsuredPage
	{
		private Dictionary<Guid, Type> dataTypeMapping = new Dictionary<Guid, Type>();
		private const string EXPORT_FOLDER = "/exported-doctypes/";

		private List<DocumentType> _docTypes;
		public List<DocumentType> DocTypes
		{
			get
			{
				if (_docTypes == null)
				{
					_docTypes = DocumentType.GetAllAsList();
				}
				return _docTypes;
			}
		}

		#region POCO Template
		private readonly static string POCO_TEMPLATE = @"using System;
using System.Linq;
using umbraco.Linq.Core;
using System.Collections.Generic;

namespace {0} {{
	public partial class {1}DataContext : UmbracoDataContext{2} {{
		#region Partials
		partial void OnCreated();
		#endregion
		
		public {1}DataContext() : base()
		{{
			OnCreated();
		}}

		public {1}DataContext(UmbracoDataProvider provider) : base(provider)
		{{
			OnCreated();
		}}

		{3}
	}}

	{4}
}}";
		#endregion

		#region Abstraction Template
		private readonly static string POCO_ABSTRACTION_TEMPLATE = @"using System;
using System.Linq;
using umbraco.Linq.Core;
using System.Collections.Generic;

namespace {0} {{
	public partial interface I{1}DataContext : IUmbracoDataContext {{
		{2}
	}}

	{3}
}}";
		#endregion

		#region Tree Template
		private readonly static string TREE_TEMPLATE = @"
		public Tree<{0}> {0}s
		{{
			get
			{{
				return this.LoadTree<{0}>();
			}}
		}}";
		#endregion

		#region Abstraction Tree Template
		private readonly static string TREE_ABSTRACTION_TEMPLATE = @"
		IEnumerable<I{0}> I{1}DataContext.{0}s
		{{
			get
			{{
				return this.LoadTree<{0}>().OfType<I{0}>();
			}}
		}}";
		#endregion

		#region Class Template
		//0 - Alias
		//1 - class name
		//2 - interface or string.Empty
		//3 - properties
		//4 - child relationships
		//5 - interface explicit implementation
		//6 - description
		private readonly static string CLASS_TEMPLATE = @"
	/// <summary>
	/// {6}
	/// </summary>
	[UmbracoInfo(""{0}"")]
	[System.Runtime.Serialization.DataContractAttribute()]
	[DocType()]
	public partial class {1} : {7} {2} {{
		public {1}() {{
		}}
		{3}
		{4}
		{5}
}}";
		#endregion

		#region Interface Template
		//0 - Class name
		//1 - properties
		//2 - child relationshipts
		private readonly static string INTERFACE_TEMPLATE = @"
	public partial interface I{0} : I{3} {{
		{1}
		{2}
}}";
		#endregion

		#region Properties Template
		private readonly static string PROPERTIES_TEMPLATE = @"
		private {0} _{1};
		/// <summary>
		/// {2}
		/// </summary>
		[UmbracoInfo(""{3}"", DisplayName = ""{4}"", Mandatory = {5})]
		[Property()]
		[System.Runtime.Serialization.DataMemberAttribute()]
		public virtual {0} {1}
		{{
			get
			{{
				return this._{1};
			}}
			set
			{{
				if ((this._{1} != value))
				{{
					this.RaisePropertyChanging();
					this._{1} = value;
                    this.IsDirty = true;
					this.RaisePropertyChanged(""{1}"");
				}}
			}}
		}}";
		#endregion

		#region Child Relationships Template
		private readonly static string CHILD_RELATIONS_TEMPLATE = @"
		private AssociationTree<{0}> _{0}s;
		public AssociationTree<{0}> {0}s
		{{
			get
			{{
				if ((this._{0}s == null))
				{{
					this._{0}s = this.ChildrenOfType<{0}>();
				}}
				return this._{0}s;
			}}
			set
			{{
				this._{0}s = value;
			}}
		}}";
		#endregion

		#region Child Relationship Abstraction Template
		private readonly static string CHILD_RELATIONS_ABSTRACTION_TEMPLATE = @"
		IEnumerable<I{0}> I{1}.{0}s 
		{{
			get
			{{
				return this.{0}s.OfType<I{0}>();
			}}
		}}";
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			btnGenerate.Text = ui.Text("create");
		}

		protected void btnGenerate_Click(object sender, EventArgs e)
		{
			var includeInterfaces = ddlGenerationMode.SelectedValue == "abs";

			var poco = string.Format(POCO_TEMPLATE,
				this.txtNamespace.Text,
				this.txtDataContextName.Text,
				includeInterfaces ? ", I" + this.txtDataContextName.Text + "DataContext" : string.Empty,
				GenerateDataContextCollections(includeInterfaces),
				GenerateClasses(includeInterfaces)
			);

			// As we save in a new folder under Media, we need to ensure it exists
			EnsureExportFolder();
			string pocoFile = Path.Combine(IO.SystemDirectories.Media + EXPORT_FOLDER, this.txtDataContextName.Text + ".txt");

			using (var writer = new StreamWriter(IO.IOHelper.MapPath(pocoFile)))
			{
				writer.Write(poco);
			}

			lnkPoco.NavigateUrl = pocoFile;

			pnlButtons.Visible = false;
			pane_files.Visible = true;

			if (includeInterfaces)
			{
				var abstraction = string.Format(POCO_ABSTRACTION_TEMPLATE,
					this.txtNamespace.Text,
					this.txtDataContextName.Text,
					GenerateDataContextAbstractCollections(),
					GenerateClassAbstraction()
				);

				string abstractionFile = Path.Combine(IO.SystemDirectories.Media + EXPORT_FOLDER, "I" + this.txtDataContextName.Text + ".txt");

				using (var writer = new StreamWriter(IO.IOHelper.MapPath(abstractionFile)))
				{
					writer.Write(abstraction);
				}

				lnkAbstractions.NavigateUrl = abstractionFile;
				lnkAbstractions.Enabled = true;
			}
		}

		private string GenerateClassAbstraction()
		{
			var sb = new StringBuilder();

			foreach (var dt in this.DocTypes)
			{
				var baseType = "DocTypeBase";
				if (dt.MasterContentType > 0)
				{
					var parent = DocTypes.First(d => d.Id == dt.MasterContentType);
					baseType = GenerateTypeName(parent.Alias);
				}

				sb.AppendLine(string.Format(INTERFACE_TEMPLATE,
					GenerateTypeName(dt.Alias),
					GenerateAbstractProperties(dt),
					GenerateAbstractRelations(dt),
					baseType
					)
				);
			}

			return sb.ToString();
		}

		private string GenerateAbstractRelations(DocumentType dt)
		{
			var sb = new StringBuilder();

			var children = dt.AllowedChildContentTypeIDs;
			foreach (var child in DocTypes.Where(d => children.Contains(d.Id)))
			{
				sb.AppendLine(string.Format("IEnumerable<I{0}> {0}s {{ get; }}",
					GenerateTypeName(child.Alias)
					)
				);
			}

			return sb.ToString();
		}

		private string GenerateAbstractProperties(DocumentType dt)
		{
			var sb = new StringBuilder();

			// zb-00036 #29889 : fix property types getter
            foreach (var pt in 
                dt.getVirtualTabs.Where(x => x.ContentType == dt.Id).SelectMany(x => x.GetPropertyTypes(dt.Id))
                .Concat(dt.PropertyTypes.Where(x => x.ContentTypeId == dt.Id && x.TabId == 0))
                )
			{
				sb.AppendLine(string.Format("{0} {1} {{ get; set; }}",
					GetDotNetType(pt),
					GenerateTypeName(pt.Alias)
					)
				);
			}

			return sb.ToString();
		}

		private string GenerateDataContextAbstractCollections()
		{
			var sb = new StringBuilder();
			foreach (var dt in this.DocTypes)
			{
				sb.AppendLine(string.Format("IEnumerable<I{0}> {0}s {{ get; }}", GenerateTypeName(dt.Alias)));
			}
			return sb.ToString();
		}

		private string GenerateClasses(bool includeInterfaces)
		{
			var sb = new StringBuilder();

			foreach (var dt in DocTypes)
			{
				string className = GenerateTypeName(dt.Alias);

				var baseType = "DocTypeBase";
				if (dt.MasterContentType > 0)
				{
					var parent = DocTypes.First(d => d.Id == dt.MasterContentType);
					baseType = GenerateTypeName(parent.Alias);
				}

				sb.Append(string.Format(CLASS_TEMPLATE,
					dt.Alias,
					className,
					includeInterfaces ? ", I" + className : string.Empty,
					GenerateProperties(dt),
					GenerateChildRelationships(dt),
					includeInterfaces ? GenerateAbstractionImplementation(dt) : string.Empty,
					FormatForComment(dt.Description),
					baseType
					)
				);
			}

			return sb.ToString();
		}

		private string GenerateAbstractionImplementation(DocumentType dt)
		{
			var sb = new StringBuilder();

			var children = dt.AllowedChildContentTypeIDs;
			foreach (var child in DocTypes.Where(d => children.Contains(d.Id)))
			{
				sb.Append(string.Format(CHILD_RELATIONS_ABSTRACTION_TEMPLATE,
					GenerateTypeName(child.Alias),
					GenerateTypeName(dt.Alias)
					)
				);
			}
			return sb.ToString();
		}

		private object GenerateChildRelationships(DocumentType dt)
		{
			var sb = new StringBuilder();
			var children = dt.AllowedChildContentTypeIDs;
			foreach (var child in DocTypes.Where(d => children.Contains(d.Id)))
			{
				sb.Append(string.Format(CHILD_RELATIONS_TEMPLATE,
					GenerateTypeName(child.Alias)
					)
				);
			}
			return sb.ToString();
		}

		private object GenerateProperties(DocumentType dt)
		{
			var sb = new StringBuilder();
			// zb-00036 #29889 : fix property types getter
			foreach (var pt in 
                dt.getVirtualTabs.Where(x => x.ContentType == dt.Id).SelectMany(x => x.GetPropertyTypes(dt.Id))
                .Concat(dt.PropertyTypes.Where(x => x.ContentTypeId == dt.Id && x.TabId == 0))
                )
			{
				sb.Append(string.Format(PROPERTIES_TEMPLATE,
					GetDotNetType(pt),
					GenerateTypeName(pt.Alias),
					FormatForComment(pt.Description),
					pt.Alias,
					pt.Name,
					pt.Mandatory.ToString().ToLower()
					)
				);
			}

			return sb.ToString();
		}

		private string GetDotNetType(PropertyType pt)
		{
			Guid id = pt.DataTypeDefinition.DataType.Id;
			if (!dataTypeMapping.ContainsKey(id))
			{
				var defaultData = pt.DataTypeDefinition.DataType.Data as DefaultData;
				if (defaultData != null) //first lets see if it inherits from DefaultData, pretty much all do
				{
					switch (defaultData.DatabaseType)
					{
						case DBTypes.Integer:
							dataTypeMapping.Add(id, typeof(int));
							break;
						case DBTypes.Date:
							dataTypeMapping.Add(id, typeof(DateTime));
							break;
						case DBTypes.Nvarchar:
						case DBTypes.Ntext:
							dataTypeMapping.Add(id, typeof(string));
							break;
						default:
							dataTypeMapping.Add(id, typeof(object));
							break;
					}
				}
				else //hmm so it didn't, lets try something else
				{
					var dbType = BusinessLogic.Application.SqlHelper.ExecuteScalar<string>(@"SELECT [t0].[dbType] FROM [cmsDataType] AS [t0] WHERE [t0].[controlId] = @p0", BusinessLogic.Application.SqlHelper.CreateParameter("@p0", id));

					if (!string.IsNullOrEmpty(dbType)) //can I determine from the DB?
					{
						switch (dbType.ToUpper())
						{
							case "INTEGER":
								dataTypeMapping.Add(id, typeof(int));
								break;
							case "DATE":
								dataTypeMapping.Add(id, typeof(DateTime));
								break;
							case "NTEXT":
							case "NVARCHAR":
								dataTypeMapping.Add(id, typeof(string));
								break;
							default:
								dataTypeMapping.Add(id, typeof(object));
								break;
						}
					}
					else
					{
						//ok, you've got a really freaky data type, so you get an Object back :P
						dataTypeMapping.Add(id, typeof(object));
					}
				}
			}
			//if it's a valueType and it's not a mandatory field we'll make it nullable. And let's be lazy and us something like 'int?' rather than
			//the fully layed out version :P
			if (!pt.Mandatory && dataTypeMapping[id].IsValueType)
				return dataTypeMapping[id].Name + "?";

			//here we can use a standard type name
			return dataTypeMapping[id].Name;
		}

		private string GenerateDataContextCollections(bool includeInterfaces)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var dt in DocTypes)
			{
				string className = GenerateTypeName(dt.Alias);
				sb.Append(string.Format(TREE_TEMPLATE,
					className
					)
				);

				if (includeInterfaces)
				{
					sb.AppendLine(string.Format(TREE_ABSTRACTION_TEMPLATE,
						className,
						txtDataContextName.Text
						)
					);
				}
			}
			return sb.ToString();
		}

		private static string GenerateTypeName(string alias)
		{
			string s = Casing.SafeAlias(alias);
			return s[0].ToString().ToUpper() + s.Substring(1, s.Length - 1);
		}

		private static string FormatForComment(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return s;
			}
			return s.Replace("\r\n", "\r\n///");
		}

		private static void EnsureExportFolder()
		{
			string packagesDirectory = IO.SystemDirectories.Media + EXPORT_FOLDER;
			if (!System.IO.Directory.Exists(IOHelper.MapPath(packagesDirectory)))
				System.IO.Directory.CreateDirectory(IOHelper.MapPath(packagesDirectory));
		}
	}
}
