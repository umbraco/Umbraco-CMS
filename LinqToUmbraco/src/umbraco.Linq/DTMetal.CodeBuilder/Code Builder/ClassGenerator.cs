using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.VisualBasic;
using Microsoft.CSharp;
using System.Globalization;
using System.Reflection;
using umbraco.Linq.DTMetal.CodeBuilder.DataType;
using VB = Microsoft.VisualBasic;

namespace umbraco.Linq.DTMetal.CodeBuilder
{
    public enum GenerationLanguage
    {
        CSharp,
        VB
    }

    internal enum SerializationMode
    {
        None, Unidirectional
    }

    internal sealed class ClassGeneratorArgs
    {
        public string Namespace { get; set; }
        public CodeDomProvider Provider { get; set; }
        public string DtmlPath { get; set; }
        public XDocument Dtml { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed class ClassGenerator
    {
        internal ClassGeneratorArgs Args { get; set; }
        internal bool PluralizeCollections { get; set; }
        internal SerializationMode SerializationMode { get; set; }
        private CodeCompileUnit Code { get; set; }

        internal bool IsCSharpCodeProvider()
        {
            return (string.Compare(this.Args.Provider.FileExtension, "CS", StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Creates a new CodeBuilder.
        /// </summary>
        /// <param name="xmlPath">The path to the DTML file</param>
        /// <param name="lang">The language to generate with.</param>
        /// <returns></returns>
        public static ClassGenerator CreateBuilder(string xmlPath, string ns, GenerationLanguage lang)
        {
            var args = new ClassGeneratorArgs()
            {
                DtmlPath = xmlPath,
                Namespace = ns
            };
            switch (lang)
            {
                case GenerationLanguage.VB:
                    args.Provider = new VBCodeProvider();
                    break;

                case GenerationLanguage.CSharp:
                default:
                    args.Provider = new CSharpCodeProvider();
                    break;
            }

            return new ClassGenerator(args);
        }

        internal ClassGenerator(ClassGeneratorArgs args)
        {
            this.Args = args;
        }

        public void Save()
        {
            if (this.Code == null)
            {
                this.GenerateCode();
            }

            var dtml = new FileInfo(this.Args.DtmlPath);

            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (StreamWriter sourceWriter = new StreamWriter(dtml.FullName.Replace("dtml", "designer." + this.Args.Provider.FileExtension)))
            {
                this.Args.Provider.GenerateCodeFromCompileUnit(this.Code, sourceWriter, options);
            }
        }

        internal byte[] SaveForVs()
        {
            if (this.Code == null)
            {
                this.GenerateCode();
            }

            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";

            StringBuilder code = new StringBuilder();
            TextWriter tw = new StringWriter(code, CultureInfo.InvariantCulture);
            this.Args.Provider.GenerateCodeFromCompileUnit(this.Code, tw, null);
            tw.Flush();

            Encoding enc = Encoding.GetEncoding(tw.Encoding.WindowsCodePage);
            byte[] preamble = enc.GetPreamble();
            int preambleLength = preamble.Length;
            byte[] body = enc.GetBytes(code.ToString());
            Array.Resize<byte>(ref preamble, preambleLength + body.Length);
            Array.Copy(body, 0, preamble, preambleLength, body.Length);

            // Return generated code.
            return preamble;
        }

        public void GenerateCode()
        {
            if (Args == null)
            {
                throw new ArgumentNullException("Args");
            }

            if (!string.IsNullOrEmpty(this.Args.DtmlPath))
            {
                var dtml = new FileInfo(this.Args.DtmlPath);
                if (!dtml.Exists)
                {
                    throw new FileNotFoundException(String.Format(Strings.DtmlNotFound, this.Args.DtmlPath));
                }
                else
                {
                    this.Args.Dtml = XDocument.Load(this.Args.DtmlPath);
                }
            }

            ValidateSchema();

            this.PluralizeCollections = (bool)this.Args.Dtml.Root.Attribute("PluralizeCollections");
            this.SerializationMode = (SerializationMode)Enum.Parse(typeof(SerializationMode), (string)this.Args.Dtml.Root.Attribute("Serialization"));

            IEnumerable<DocType> docTypes = XmlToClasses(this.Args.Dtml);

            this.Code = new CodeCompileUnit();
            CodeNamespace ns = GenerateNamespace(this.Args.Namespace);

            CodeTypeDeclaration dataContext = CreateDataContext(this.Args.Dtml, docTypes);

            ns.Types.Add(dataContext);

            CreateDocTypes(docTypes, ns);

            this.Code.Namespaces.Add(ns);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private CodeTypeDeclaration CreateDataContext(XDocument dtmlXml, IEnumerable<DocType> docTypes)
        {
            string methodName = "OnCreated";
            CodeExpressionStatement statement = new CodeExpressionStatement(new CodeMethodInvokeExpression(null, methodName, new CodeExpression[0]));

            string dataContextName = dtmlXml.Root.Attribute("DataContextName").Value;
            //ensure the naming is standard
            if (!dataContextName.ToUpper().Contains("DATACONTEXT"))
            {
                dataContextName += "DataContext";
            }
            CodeTypeDeclaration dataContext = new CodeTypeDeclaration(dataContextName);
            dataContext.BaseTypes.Add("umbracoDataContext");
            dataContext.IsClass = true;
            dataContext.IsPartial = true;

            string partialOnCreated = string.Empty;
            if (IsCSharpCodeProvider())
            {
                partialOnCreated = " partial void " + methodName + "();\r\n";
            }
            else
            {
                partialOnCreated = " Partial Private Void " + methodName + "()\r\nEnd Sub\r\n";
            }
            CodeSnippetTypeMember onCreated = new CodeSnippetTypeMember(partialOnCreated);
            CodeRegionDirective region = new CodeRegionDirective(CodeRegionMode.Start, "Partials");
            onCreated.StartDirectives.Add(region);
            onCreated.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Partials"));
            dataContext.Members.Add(onCreated);

            //constructor with no arguments
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.BaseConstructorArgs.Add(new CodePropertyReferenceExpression());
            ctor.Statements.Add(statement);
            dataContext.Members.Add(ctor);

            //constructor that takes an umbracoDataProvider
            ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("umbracoDataProvider", "provider"));
            ctor.BaseConstructorArgs.Add(new CodePropertyReferenceExpression(null, "provider"));
            ctor.Statements.Add(statement);
            dataContext.Members.Add(ctor);

            //Generate the Tree<TDocType> for each docType
            foreach (var dt in docTypes)
            {
                string name = dt.TypeName;
                if (this.PluralizeCollections)
                {
                    name = PluraliseName(dt.TypeName);
                }
                var t = new CodeTypeReference("Tree");
                t.TypeArguments.Add(dt.TypeName);

                CodeMemberProperty p = new CodeMemberProperty();
                p.Name = name;
                p.Type = t;
                p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                p.HasGet = true;
                p.HasSet = false;

                p.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeThisReferenceExpression(),
                                "LoadTree",
                                new CodeTypeReference[] { 
                                    new CodeTypeReference(dt.TypeName) 
                                }),
                                new CodeExpression[0])
                            )
                        );

                dataContext.Members.Add(p);
            }
            return dataContext;
        }

        internal IEnumerable<DocType> XmlToClasses(XDocument dtmlXml)
        {
            return dtmlXml.Descendants("DocumentType").Select(x => new DocType
            {
                Alias = (string)x.Element("Alias"),
                Description = (string)x.Element("Description"),
                Id = (int)x.Element("Id"),
                Name = (string)x.Element("Name"),
                ParentId = (int)x.Attribute("ParentId"),
                TypeName = Normalise((string)x.Element("Alias")),
                Properties = x.Descendants("Property").Select(p => new DocTypeProperty
                {
                    Alias = (string)p.Element("Alias"),
                    ControlId = new Guid((string)p.Element("ControlId")),
                    DatabaseType = Type.GetType((string)p.Element("Type")),
                    Description = (string)p.Element("Description"),
                    Id = (int)p.Element("Id"),
                    Name = (string)p.Element("Name"),
                    Mandatory = (bool)p.Element("Mandatory"),
                    RegularExpression = (string)p.Element("RegularExpression"),
                    TypeName = Normalise((string)p.Element("Alias"))
                }).ToList(),
                Associations = x.Descendants("Association").Select(a => new DocTypeAssociation
                {
                    AllowedId = (int)a
                }).ToList()
            });
        }

        private void ValidateSchema()
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            //read the resorce for the XSD to validate against
            Assembly assembly = Assembly.GetExecutingAssembly();
            schemas.Add("", System.Xml.XmlReader.Create(assembly.GetManifestResourceStream(assembly.GetName().Name + ".DocTypeML.xsd")));

            //we'll have a list of all validation exceptions to put them to the screen
            List<XmlSchemaException> exList = new List<XmlSchemaException>();

            //some funky in-line event handler. Lambda loving goodness ;)
            this.Args.Dtml.Validate(schemas, (o, e) => { exList.Add(e.Exception); });

            if (exList.Count > 0)
            {
                //dump out the exception list
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(umbraco.Linq.DTMetal.CodeBuilder.Strings.XsdValidationFailureHeading);
                foreach (var item in exList)
                {
                    sb.AppendLine(" * " + item.Message + " - " + item.StackTrace);
                }
                throw new XmlSchemaException(sb.ToString());
            }
        }

        private void CreateDocTypes(IEnumerable<DocType> docTypes, CodeNamespace ns)
        {
            foreach (var docType in docTypes)
            {
                string genName = docType.TypeName;

                CodeCompileUnit currUnit = new CodeCompileUnit();

                currUnit.Namespaces.Add(ns);

                //create class
                CodeTypeDeclaration currClass = new CodeTypeDeclaration(genName);
                //create the custom attribute
                CodeAttributeDeclarationCollection classAttributes = new CodeAttributeDeclarationCollection(
                    new CodeAttributeDeclaration[] {
                        new CodeAttributeDeclaration("umbracoInfo",
                            new CodeAttributeArgument(new CodePrimitiveExpression(docType.Alias)),
                            new CodeAttributeArgument("Id", new CodePrimitiveExpression(docType.Id))),
                        new CodeAttributeDeclaration(new CodeTypeReference(typeof(DataContractAttribute))),
                        new CodeAttributeDeclaration("DocType")
                    });
                //add the address to the class
                currClass.CustomAttributes.AddRange(classAttributes);

                currClass.IsClass = true;
                //add the summary decoration
                currClass.Comments.AddRange(GenerateSummary(docType.Description));
                //set up the type
                currClass.TypeAttributes = TypeAttributes.Public;
                if (docType.ParentId > 0)
                {
                    currClass.BaseTypes.Add(new CodeTypeReference(docTypes.Single(d => d.Id == docType.ParentId).TypeName)); //docType inheritance
                }
                else
                {
                    currClass.BaseTypes.Add(new CodeTypeReference("DocTypeBase")); //base class 
                }
                currClass.IsPartial = true;

                currClass.Members.AddRange(GenerateConstructors());

                #region Doc Type Properties
                foreach (var docTypeProperty in docType.Properties)
                {
                    CodeMemberField valueField = new CodeMemberField();
                    valueField.Attributes = MemberAttributes.Private;
                    valueField.Name = "_" + docTypeProperty.TypeName;
                    valueField.Type = new CodeTypeReference(docTypeProperty.DatabaseType);
                    currClass.Members.Add(valueField);

                    //store the umbraco data in an attribute.
                    CodeMemberProperty p = new CodeMemberProperty();
                    p.CustomAttributes.AddRange(GenerateDocTypePropertyAttributes(docTypeProperty));

                    p.Name = docTypeProperty.TypeName;
                    p.Type = new CodeTypeReference(docTypeProperty.DatabaseType);
                    p.Attributes = MemberAttributes.Public;
                    p.HasGet = true;
                    p.HasSet = false;
                    p.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), valueField.Name))
                        );

                    #region Set statement
                    //have a conditional statment so we can use the INotifyChanging and INotifyChanged events
                    CodeExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), valueField.Name);
                    CodeExpression right = new CodePropertySetValueReferenceExpression();

                    CodeExpression cond = GenerateInequalityConditionalStatement(left, right);

                    //Build the statements to execute when we are changing the property value
                    //The order is:
                    // - RaisePropertyChanging event
                    // - Assign the property
                    // - RaisePropertyChanged event
                    var trues = new CodeStatement[] {
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), 
                            "RaisePropertyChanging"
                            )
                        ),
                        new CodeAssignStatement(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), valueField.Name),
                            new CodePropertySetValueReferenceExpression()
                        ),
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                                new CodeThisReferenceExpression(), 
                                "RaisePropertyChanged",
                                new CodePrimitiveExpression(docTypeProperty.TypeName)
                            )
                        )
                    };

                    CodeConditionStatement condition = new CodeConditionStatement(cond, trues);
                    //enforce the validation from umbraco. It's there for a reason ;)
                    if (!string.IsNullOrEmpty(docTypeProperty.RegularExpression))
                    {
                        p.SetStatements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(
                                        null,
                                        "ValidateProperty",
                                        new CodePrimitiveExpression(docTypeProperty.RegularExpression),
                                        new CodePropertySetValueReferenceExpression())
                                        )
                                    );
                    }
                    p.SetStatements.Add(condition);
                    #endregion

                    //comment the property with the description from umbraco
                    p.Comments.AddRange(GenerateSummary(docTypeProperty.Description));
                    currClass.Members.Add(p);

                    CodeMemberProperty retypedProperty = CreateCustomProperty(docTypeProperty, valueField.Name);
                    if (retypedProperty != null)
                    {
                        currClass.Members.Add(retypedProperty);
                    }
                }
                #endregion

                #region Child Associations
                foreach (var child in docType.Associations)
                {
                    var realDocType = docTypes.SingleOrDefault(dt => dt.Id == child.AllowedId);

                    //put a check that a docType is actually returned
                    //This will cater for the bug of when you don't select to generate a 
                    //docType but it is a child of the current
                    if (realDocType != null)
                    {
                        CodeMemberField childMember = new CodeMemberField();
                        string name = realDocType.TypeName;
                        if (this.PluralizeCollections)
                        {
                            name = PluraliseName(realDocType.TypeName);
                        }
                        childMember.Attributes = MemberAttributes.Private;
                        childMember.Name = "_" + name;
                        var t = new CodeTypeReference("AssociationTree");
                        t.TypeArguments.Add(realDocType.TypeName);
                        childMember.Type = t;
                        currClass.Members.Add(childMember);

                        CodeMemberProperty p = new CodeMemberProperty();
                        p.Name = name;
                        p.Type = t;
                        p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                        p.HasGet = true;
                        p.HasSet = true;

                        CodeExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), childMember.Name);
                        CodeExpression right = new CodePrimitiveExpression(null);

                        CodeExpression cond = GenerateEqualityConditionalStatement(left, right);

                        var trues = new CodeConditionStatement(cond, new CodeAssignStatement(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), childMember.Name),
                            new CodeMethodInvokeExpression(
                                new CodeMethodReferenceExpression(
                                    new CodeThisReferenceExpression(),
                                    "ChildrenOfType",
                                    new CodeTypeReference[] {
                                        new CodeTypeReference(realDocType.TypeName)
                                    })
                                )
                            )
                        );

                        p.GetStatements.Add(trues);
                        p.GetStatements.Add(new CodeMethodReturnStatement(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), childMember.Name))
                            );

                        p.SetStatements.Add(
                            new CodeAssignStatement(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), childMember.Name),
                            new CodePropertySetValueReferenceExpression()
                            )
                        );

                        currClass.Members.Add(p);
                    }
                } 
                #endregion

                ns.Types.Add(currClass);
            }
        }

        private CodeMemberProperty CreateCustomProperty(DocTypeProperty docTypeProperty, string privateVariableName)
        {
            var internalTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(DataTypeRetyper).IsAssignableFrom(t) && t.GetCustomAttributes(typeof(DataTypeAttribute), true).Length == 1);

            var currType = internalTypes.SingleOrDefault(t => t.GetCustomAttributes(typeof(DataTypeAttribute), true).Cast<DataTypeAttribute>().First().ControlId == docTypeProperty.ControlId);
            if (currType != null)
            {
                var retyper = (DataTypeRetyper)Activator.CreateInstance(currType);

                CodeMemberProperty p = new CodeMemberProperty();
                p.Type = new CodeTypeReference(retyper.MemberType);
                p.Name = retyper.MemberName(docTypeProperty.TypeName);
                p.HasGet = true;
                p.HasSet = false;

                new Switch(retyper)
                .Case<YesNoRetyper>(b => {
                    p.GetStatements.Add(new CodeMethodReturnStatement(
                        GenerateEqualityConditionalStatement(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), privateVariableName)
                                , new CodePrimitiveExpression(0)
                            )
                        )
                    );
                }, true)
                ;

                return p;
            }

            return null;
        }

        private CodeExpression GenerateInequalityConditionalStatement(CodeExpression left, CodeExpression right)
        {
            //Build a binary conditional operation (an IF)
            CodeExpression cond;
            //if (GenerationLanaguage == Language.CSharp)
            //{
            cond = new CodeBinaryOperatorExpression(
                    left,
                    CodeBinaryOperatorType.IdentityInequality,
                    right
                );
            //}
            //else
            //{
            //    cond = new CodeBinaryOperatorExpression(
            //            new CodeBinaryOperatorExpression(
            //                left,
            //                CodeBinaryOperatorType.IdentityEquality,
            //                right
            //            ),
            //            CodeBinaryOperatorType.ValueEquality,
            //            new CodePrimitiveExpression(false)
            //        );
            //}
            return cond;
        }

        private CodeExpression GenerateEqualityConditionalStatement(CodeExpression left, CodeExpression right)
        {
            //Build a binary conditional operation (an IF)
            CodeExpression cond;
            //if (GenerationLanaguage == Language.CSharp)
            //{
            cond = new CodeBinaryOperatorExpression(
                    left,
                    CodeBinaryOperatorType.IdentityEquality,
                    right
                );
            //}
            //else
            //{
            //    cond = new CodeBinaryOperatorExpression(
            //            new CodeBinaryOperatorExpression(
            //                left,
            //                CodeBinaryOperatorType.IdentityInequality,
            //                right
            //            ),
            //            CodeBinaryOperatorType.ValueEquality,
            //            new CodePrimitiveExpression(false)
            //        );
            //}
            return cond;
        }

        private CodeAttributeDeclaration[] GenerateDocTypePropertyAttributes(DocTypeProperty docTypeProperty)
        {
            List<CodeAttributeDeclaration> attrs = new List<CodeAttributeDeclaration>();

            CodeAttributeDeclaration umbInfoAtt = new CodeAttributeDeclaration("umbracoInfo",
                                    new CodeAttributeArgument(new CodePrimitiveExpression(docTypeProperty.Alias)),
                                    new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(docTypeProperty.Name)),
                                    new CodeAttributeArgument("Mandatory", new CodePrimitiveExpression(docTypeProperty.Mandatory))
                                    );

            attrs.Add(umbInfoAtt);
            attrs.Add(new CodeAttributeDeclaration("Property"));
            if (this.SerializationMode == SerializationMode.Unidirectional)
            {
                CodeAttributeDeclaration dataMemberAtt = new CodeAttributeDeclaration(new CodeTypeReference(typeof(DataMemberAttribute)),
                                 new CodeAttributeArgument("Name", new CodePrimitiveExpression(docTypeProperty.TypeName))
                                );
            }

            return attrs.ToArray();
        }

        private static CodeTypeMember[] GenerateConstructors()
        {
            CodeConstructor defaultConstructor = new CodeConstructor();
            defaultConstructor.Attributes = MemberAttributes.Public;

            return new CodeTypeMember[] { defaultConstructor };
        }

        private static CodeNamespace GenerateNamespace(string name)
        {
            CodeNamespace ns = new CodeNamespace(name);
            //ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("umbraco.Linq.Core"));
            ns.Imports.Add(new CodeNamespaceImport("umbraco.Linq.Core.Node"));
            ns.Imports.Add(new CodeNamespaceImport("System.Linq"));
            return ns;
        }

        private static CodeCommentStatement[] GenerateSummary(string summaryBody)
        {
            return new CodeCommentStatement[] {
                    new CodeCommentStatement("<summary>", true),
                    new CodeCommentStatement(summaryBody, true),
                    new CodeCommentStatement("</summary>", true)
                };
        }

        private static bool IsVowel(char c)
        {
            switch (c)
            {
                case 'O':
                case 'U':
                case 'Y':
                case 'A':
                case 'E':
                case 'I':
                case 'o':
                case 'u':
                case 'y':
                case 'a':
                case 'e':
                case 'i':
                    return true;
            }
            return false;
        }

        internal static string PluraliseName(string name)
        {
            if ((name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase)) || (name.EndsWith("ss", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase)))
            {
                name = name + "es";
                return name;
            }
            if ((name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && (name.Length > 1)) && !IsVowel(name[name.Length - 2]))
            {
                name = name.Remove(name.Length - 1, 1);
                name = name + "ies";
                return name;
            }
            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name = name + "s";
            }
            return name;
        }

        internal string Normalise(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input");
            }

            var invalids = new string[] { "_", "-", ".", "$", "@", "*" };

            input = input.Trim().ToLower();

            foreach (var i in invalids)
            {
                input = input.Replace(i, " ");
            }

            var correctCasedInput = VB.Strings.StrConv(input, VB.VbStrConv.ProperCase, 0);

            var correctCasedAsArray = correctCasedInput.Split(' ').Where(s => !string.IsNullOrEmpty(s));

            //var firstItem = correctCasedAsArray[0];
            StringBuilder ret = new StringBuilder();
            var foundChar = false;
            foreach (var item in correctCasedAsArray)
            {
                foreach (var c in item)
                {
                    if (Char.IsLetter(c))
                    {
                        ret.Append(c);
                        foundChar = true;
                    }
                    else if (Char.IsDigit(c) && foundChar)
                    {
                        ret.Append(c);
                    }
                }
            }

            if (string.IsNullOrEmpty(ret.ToString()))
            {
                throw new IndexOutOfRangeException("No valid characters found within the string being normalised");
            }

            return ret.ToString();
        }
    }
}
